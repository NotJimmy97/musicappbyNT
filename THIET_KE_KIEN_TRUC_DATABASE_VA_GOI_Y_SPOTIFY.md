# THIẾT KẾ KIẾN TRÚC DATABASE, CACHE ĐA NGUỒN SPOTIFY & HỆ THỐNG GỢI Ý BÀI HÁT
## DÀNH RIÊNG CHO MUSICAPP (.NET FRAMEWORK 4.6.1 / WPF NATIVE)

---

| Thuộc tính | Chi tiết kỹ thuật |
| :--- | :--- |
| **Dự án** | MusicApp (.NET Framework 4.6.1 / WPF Native / MVVM / Local BFF OWIN) |
| **Mục tiêu cốt lõi** | 1. Database bền vững tốc độ cao (SQLite WAL Mode) <br> 2. Kiến trúc lưu trữ đa nguồn học hỏi từ Spotify (CAS Chunk Cache + Deduplication) <br> 3. Hệ thống gợi ý bài hát thông minh (Client-side Affinity Recommendation) <br> 4. Tăng tốc độ tối đa cho ứng dụng (Zero-Latency, Virtualization, Memory Freeze) |
| **Tài liệu tham chiếu** | `PLAN_NANG_CAP_TOAN_DIEN.md`, `TECHNICAL_DESIGN_DOCUMENT.md` |
| **Độ tin cậy** | Chuẩn Senior .NET Systems Architect |

---

## 1. HỌC HỎI KIẾN TRÚC LƯU TRỮ ĐA NGUỒN CỦA SPOTIFY (SPOTIFY STORAGE PARADIGM)

Spotify không chỉ là một ứng dụng phát nhạc trực tuyến đơn thuần; bản chất của Spotify Desktop Client là một **Hybrid Content Delivery & Caching System** xử lý song song cả nhạc cục bộ (Local Files) và nhạc trực tuyến (Streaming CDN).

```
                             KIẾN TRÚC SPOTIFY STORAGE ĐƯA VÀO MUSICAPP
                             
+--------------------------------------------------------------------------------------------------+
|                                    UNIFIED TRACK MODEL (WPF UI)                                  |
|                 URI: "local:{hash}" | "jamendo:{id}" | "vn:{id}" | "cache:{hash}"                |
+--------------------------------------------------------------------------------------------------+
                                                 │
                                                 ▼
+--------------------------------------------------------------------------------------------------+
|                                  TRACK DEDUPLICATION & IDENTITY                                  |
|       Chuẩn hóa Metadata (Fuzzy Title + Artist Fingerprint) -> Hợp nhất Metadata đa nguồn        |
+--------------------------------------------------------------------------------------------------+
                                                 │
                 ┌───────────────────────────────┴───────────────────────────────┐
                 ▼                                                               ▼
+------------------------------------+                         +-----------------------------------+
|       METADATA & STATE TIER        |                         |         AUDIO BINARY TIER         |
|      (SQLite Embedded WAL Mode)    |                         |  (Content-Addressable Disk Cache) |
|                                    |                         |                                   |
| - Danh mục bài hát (Tracks)        |                         | - Local Files (Zero-copy stream)  |
| - Playlists & Hàng đợi (Queue)     |                         | - Streaming HTTP 206 Chunks       |
| - Lịch sử nghe & Tương tác click   |                         | - Ring Buffer Cache (1GB LRU)     |
| - Ma trận trọng số sở thích        |                         | - Cover Art WebP/JPG 120px        |
+------------------------------------+                         +-----------------------------------+
```

### 1.1. Định danh bài hát hợp nhất (Unified Track Identity & Deduplication)
Trong Spotify, một bài hát có thể đến từ 3 nguồn:
1. `Local File`: File `.mp3`, `.flac` trên máy người dùng.
2. `Jamendo CDN`: Nguồn nhạc quốc tế miễn phí Creative Commons.
3. `Vietnamese Music CDN`: Nguồn nhạc Việt Nam (ZingMP3 / NhacCuaTui / Archive.org).

**Vấn đề:** Nếu người dùng vừa có bài *"Nàng Thơ - Hoàng Dũng"* trên ổ cứng, vừa tìm thấy bài đó trên mạng, Spotify không coi đó là 2 thực thể xa lạ.
**Giải pháp:** Áp dụng **Fuzzy Fingerprint Key**:
$$\text{TrackKey} = \text{Normalize}(\text{Title}) + "::" + \text{Normalize}(\text{Artist})$$
Trong đó `Normalize` loại bỏ dấu tiếng Việt, bỏ ký tự đặc biệt, bỏ các đuôi phụ (e.g. `[Official MV]`, `(Remix)`, `(Beat)`, `ft. ...`).
* Khi phát: Ưu tiên phát từ `Local File` (độ trễ 0ms, không tốn băng thông internet).
* Nếu không có local: Chuyển tiếp stream qua CDN và tự động nạp vào **Local Disk Cache**.

### 1.2. Cơ chế Disk Cache 2 tầng (Content-Addressable Storage - CAS)
Spotify lưu cache nhạc trên đĩa dưới dạng các file mã hóa hoặc chunk nhị phân theo mã băm SHA-256 (`%LOCALAPPDATA%\Spotify\Storage\Data\*`).
Đối với MusicApp:
* Khi `StreamController` proxy một luồng stream HTTP 206 (Range request), luồng byte nhận được từ CDN vừa được truyền tới Audio Engine (`NAudio`), vừa được ghi nền (Background Worker) vào file cache nhị phân:
  `%LOCALAPPDATA%\MusicApp\Cache\{track_hash}.audio`
* Một bảng SQLite `stream_cache` quản lý vòng đời tệp:
  * `track_hash`: Mã băm SHA-1 của URL/TrackId.
  * `file_size_bytes`: Kích thước file cache.
  * `last_accessed_at`: Thời gian nghe gần nhất.
  * **Chính sách dọn dẹp LRU (Least Recently Used)**: Khi thư mục cache vượt quá giới hạn cấu hình (ví dụ: `1024 MB`), tự động dọn dẹp các bài hát có `last_accessed_at` cũ nhất.
* **Kết quả vượt trội**: Lần thứ 2 người dùng bấm nghe lại bài hát trực tuyến đó, ứng dụng phát **ngay lập tức trong 5ms** từ ổ cứng, hoàn toàn hoạt động offline mà không cần kết nối mạng!

---

## 2. HỆ THỐNG GỢI Ý BÀI HÁT THÔNG MINH THEO CLICK & HÀNH VI (RECOMMENDATION ENGINE)

Một ứng dụng nghe nhạc hiện đại không bắt người dùng phải lục lọi tìm từng bài. Spotify chinh phục thế giới bằng tính năng *Discover Weekly* và *Autoplay*.
Trên môi trường Desktop WPF độc lập (không có cụm server AI đám mây), chúng ta triển khai **Hệ thống Gợi ý dựa trên Nội dung & Ma trận Tương tác Cục bộ (Client-Side Content-Based & Affinity Scoring Engine)**.

### 2.1. Ma trận Trọng số Hành vi (Implicit Feedback Matrix)
Mỗi thao tác của người dùng trên giao diện là một tín hiệu phản ánh mức độ yêu thích:

| Hành vi người dùng (User Action) | Điểm số tác động ($\Delta S$) | Ý nghĩa giải thuật |
| :--- | :---: | :--- |
| **Bấm Thích (♥ Heart / Favorite)** | **+10.0** | Tín hiệu khẳng định yêu thích cao nhất. |
| **Thêm vào Playlist cá nhân** | **+8.0** | Tín hiệu người dùng muốn nghe lặp lại lâu dài. |
| **Nghe trọn vẹn bài hát (> 85% thời lượng)** | **+5.0** | Bài hát hợp gu, giữ chân người nghe tốt. |
| **Nghe > 30 giây** | **+2.0** | Thỏa mãn ngưỡng nghe thực tế (Spotify Standard). |
| **Click chọn từ danh sách Search / Explore** | **+1.5** | Người dùng chủ động tò mò hoặc tìm kiếm. |
| **Bỏ qua (Skip) sau khi nghe < 10 giây** | **-4.0** | Không thích bài hát này / Không đúng tâm trạng. |
| **Chặn / Xóa khỏi Playlist** | **-10.0** | Tín hiệu bài hát gây khó chịu. |

### 2.2. Giải thuật Điểm số Quan hệ (Affinity Score Algorithm)
Mỗi bài hát $T$ trong kho dữ liệu sẽ được chấm điểm độ phù hợp theo công thức:

$$\text{AffinityScore}(T) = w_A \cdot \text{Score}(\text{Artist}_T) + w_G \cdot \text{Score}(\text{Genre}_T) + w_H \cdot \text{PlayHistoryWeight}(T) - \lambda \cdot \text{FatiguePenalty}(T)$$

* **$\text{Score}(\text{Artist}_T)$**: Tổng điểm tương tác của người dùng với tất cả các bài hát của cùng nghệ sĩ đó.
* **$\text{Score}(\text{Genre}_T)$**: Điểm số thể loại (Pop, Ballad, Rock, Acoustic, Lofi...).
* **$\text{FatiguePenalty}(T)$ (Tránh nhàm chán)**: Nếu một bài hát vừa được nghe trong vòng 2 giờ qua, điểm số bị trừ nặng để tránh trường hợp thuật toán phát đi phát lại 1 bài.
* **Tự động suy luận Thể loại (Genre Inferencing)**: Với nhạc local không có tag Genre, thuật toán phân loại dựa trên phổ FFT (ví dụ: Bass năng lượng cao $\rightarrow$ EDM/Dance; Vocal tần số trung 1kHz-3kHz nổi bật $\rightarrow$ Ballad/Pop).

### 2.3. Ba tính năng thông minh cho người dùng
1. **Màn hình "Gợi ý cho bạn" (Recommended For You)**:
   * Tự động sinh danh sách 25 bài hát có $\text{AffinityScore}$ cao nhất trong toàn bộ thư viện (kết hợp cả Local và Online đã cache).
2. **Radio Bài Hát (Track Radio / Infinite Autoplay)**:
   * Khi hàng đợi phát nhạc kết thúc (hết bài cuối cùng), thay vì dừng im lặng, hệ thống tự động tìm 5 bài hát có độ tương đồng âm học và cùng nghệ sĩ gần nhất với bài vừa phát để tiếp tục chơi nhạc không ngừng nghỉ (tính năng Autoplay của Spotify).
3. **Smart Shuffle (Xáo trộn thông minh thay vì ngẫu nhiên mù)**:
   * Thuật toán Shuffle truyền thống (Fisher-Yates) là ngẫu nhiên thuần túy, có thể nhảy vào một bài hát người dùng rất ghét.
   * **Smart Shuffle** sử dụng phương pháp bốc thăm theo phân phối xác suất Boltzmann (Softmax Sampling): các bài hát điểm cao có xác suất được bốc trúng gấp 4 lần bài hát điểm thấp, đồng thời giữ 15% xác suất cho các bài mới chưa nghe để khám phá.

---

## 3. THIẾT KẾ SCHEMA DATABASE CHI TIẾT (SQLITE WAL MODE)

Cơ sở dữ liệu được đặt tại: `%LOCALAPPDATA%\MusicApp\musicapp.db`.
Áp dụng cấu hình tối ưu hiệu năng cao:
```sql
PRAGMA journal_mode = WAL;          -- Cho phép Đọc và Ghi song song không khóa nhau
PRAGMA synchronous = NORMAL;        -- Giảm thời gian chờ fsync đĩa từ 50ms xuống 0.2ms
PRAGMA cache_size = -64000;         -- Cấp phát 64MB RAM làm bộ nhớ đệm trang của SQLite
PRAGMA temp_store = MEMORY;         -- Mọi bảng tạm và sắp xếp chạy 100% trên RAM
PRAGMA foreign_keys = ON;           -- Ràng buộc toàn vẹn khóa ngoại
```

```sql
-- ==============================================================================
-- 1. BẢNG CẤU HÌNH & TRẠNG THÁI HỆ THỐNG
-- ==============================================================================
CREATE TABLE IF NOT EXISTS app_settings (
    key   TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

-- ==============================================================================
-- 2. KHO BÀI HÁT TỔNG HỢP (UNIFIED TRACK CATALOG)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS tracks (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    track_key        TEXT NOT NULL UNIQUE,      -- Normalized Fingerprint: "title::artist"
    source_type      TEXT NOT NULL,             -- 'local' | 'jamendo' | 'vn'
    source_id        TEXT NOT NULL,             -- FilePath hoặc Online Stream ID
    title            TEXT NOT NULL,
    artist           TEXT NOT NULL,
    album            TEXT,
    genre            TEXT,
    duration_seconds INTEGER NOT NULL DEFAULT 0,
    bitrate          INTEGER DEFAULT 128,
    cover_uri        TEXT,                      -- Đường dẫn file ảnh cache cục bộ
    file_mtime       TEXT,                      -- ISO 8601 (Dùng cho Incremental Scan)
    play_count       INTEGER NOT NULL DEFAULT 0,
    skip_count       INTEGER NOT NULL DEFAULT 0,
    is_favorite      INTEGER NOT NULL DEFAULT 0, -- 0: false, 1: true
    affinity_score   REAL NOT NULL DEFAULT 0.0, -- Điểm yêu thích cập nhật liên tục
    last_played_at   TEXT,
    created_at       TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_tracks_artist ON tracks(artist);
CREATE INDEX IF NOT EXISTS idx_tracks_album ON tracks(album);
CREATE INDEX IF NOT EXISTS idx_tracks_favorite ON tracks(is_favorite);
CREATE INDEX IF NOT EXISTS idx_tracks_affinity ON tracks(affinity_score DESC);

-- ==============================================================================
-- 3. QUẢN LÝ THƯ MỤC SCAN LOCAL (QUÉT GIA TĂNG TỐC ĐỘ CAO)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS library_folders (
    folder_path     TEXT PRIMARY KEY,
    last_scanned_at TEXT NOT NULL,
    total_files     INTEGER NOT NULL DEFAULT 0
);

-- ==============================================================================
-- 4. BỘ NHỚ ĐỆM TỆP STREAM TRỰC TUYẾN (SPOTIFY DISK CACHE)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS stream_cache (
    track_hash       TEXT PRIMARY KEY,          -- SHA-1 của Stream URL / TrackID
    file_path        TEXT NOT NULL,             -- %LOCALAPPDATA%\MusicApp\Cache\{hash}.audio
    file_size_bytes  INTEGER NOT NULL,
    last_accessed_at TEXT NOT NULL,
    is_fully_cached  INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_cache_accessed ON stream_cache(last_accessed_at ASC);

-- ==============================================================================
-- 5. DANH SÁCH PHÁT (CUSTOM PLAYLISTS & M:N RELATIONS)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS playlists (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    name        TEXT NOT NULL UNIQUE,
    description TEXT,
    cover_uri   TEXT,
    created_at  TEXT NOT NULL,
    updated_at  TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS playlist_tracks (
    playlist_id INTEGER NOT NULL REFERENCES playlists(id) ON DELETE CASCADE,
    track_id    INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,
    position    INTEGER NOT NULL,
    added_at    TEXT NOT NULL,
    PRIMARY KEY (playlist_id, track_id)
);
CREATE INDEX IF NOT EXISTS idx_playlist_pos ON playlist_tracks(playlist_id, position ASC);

-- ==============================================================================
-- 6. HÀNG ĐỢI PHÁT NHẠC HIỆN TẠI (PLAY QUEUE PERSISTENCE)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS play_queue (
    position INTEGER PRIMARY KEY,
    track_id INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE
);

-- ==============================================================================
-- 7. LỊCH SỬ TƯƠNG TÁC (USER INTERACTION LOG CHO RECOMMENDATION)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS user_interactions (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    track_id        INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,
    action_type     TEXT NOT NULL,              -- 'click', 'play_start', 'play_complete', 'skip', 'favorite', 'unfavorite'
    duration_played INTEGER DEFAULT 0,          -- Số giây đã nghe trước khi chuyển bài
    created_at      TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_interactions_track ON user_interactions(track_id);
CREATE INDEX IF NOT EXISTS idx_interactions_time ON user_interactions(created_at DESC);

-- ==============================================================================
-- 8. BỘ LỌC CÂN BẰNG ÂM SẮC (EQ PRESETS PERSISTENCE)
-- ==============================================================================
CREATE TABLE IF NOT EXISTS eq_presets (
    name        TEXT PRIMARY KEY,
    gains_json  TEXT NOT NULL,                  -- "[0.0, 3.5, -2.0, ...]" (10 bands)
    is_custom   INTEGER NOT NULL DEFAULT 0
);
```

---

## 4. CHIẾN LƯỢC TĂNG TỐC ĐỘ ỨNG DỤNG TỐI ĐA (ZERO-LATENCY PERFORMANCE PLAYBOOK)

Để ứng dụng chạy mượt mà ngay cả khi thư viện có **20,000 bài hát**, phải đồng thời loại bỏ 5 nút thắt hiệu năng cố hữu của WPF và .NET:

### 4.1. Ảo hóa Giao diện Triệt để (Full UI Virtualization)
* **Vấn đề**: WPF nếu không ảo hóa sẽ tạo ra hàng chục nghìn đối tượng `ListBoxItem` và `TextBlock` trong bộ nhớ khi mở thư viện lớn, làm ứng dụng "đứng hình" 10-15 giây và ngốn 2GB RAM.
* **Giải pháp**: Bật Virtualization cấp độ tối đa trong XAML cho toàn bộ `ListBox` và `ItemsControl`:
  ```xml
  <ListBox VirtualizingStackPanel.IsVirtualizing="True"
           VirtualizingStackPanel.VirtualizationMode="Recycling"
           VirtualizingStackPanel.ScrollUnit="Pixel"
           VirtualizingStackPanel.IsVirtualizingWhenGrouping="True"
           ScrollViewer.CanContentScroll="True">
  ```
* **Hiệu quả**: Dù danh sách có 50,000 bài, WPF chỉ dựng đúng ~25 phần tử đang hiển thị trên màn hình. RAM tiêu thụ giảm từ **1,500 MB xuống còn 65 MB**!

### 4.2. Quản lý Ảnh Bìa Chống Tràn Bộ Nhớ (Thumbnail Caching & Image Freeze)
* **Vấn đề**: File MP3 chất lượng cao thường nhúng ảnh bìa 3000x3000px dung lượng 5MB-10MB. Nếu giải mã 100 ảnh vào RAM, ứng dụng lập tức văng lỗi `OutOfMemoryException`.
* **Giải pháp chuẩn Spotify**:
  1. Khi quét thư mục: Đọc ảnh bìa nhúng, resize ngay lập tức xuống kích thước thumbnail chuẩn **120x120 pixel** (chỉ tốn ~15KB RAM mỗi ảnh).
  2. Lưu ảnh thumbnail này vào thư mục đệm `%LOCALAPPDATA%\MusicApp\Covers\{hash}.jpg`.
  3. Khi nạp ảnh vào WPF, bắt buộc đặt kích thước giải mã và đóng băng đối tượng (Freeze) trên Worker Thread:
     ```csharp
     var bitmap = new BitmapImage();
     bitmap.BeginInit();
     bitmap.UriSource = new Uri(coverPath);
     bitmap.DecodePixelWidth = 120; // CHỈ giải mã ở kích thước 120px, không nạp full size
     bitmap.CacheOption = BitmapCacheOption.OnLoad;
     bitmap.EndInit();
     bitmap.Freeze(); // Đưa vào chế độ bất biến, cho phép chia sẻ xuyên luồng và GPU render cực nhanh
     ```

### 4.3. Quét Gia tăng Siêu tốc (Incremental Scanner - 0.2s Reload)
* **Lần đầu tiên**: Quét toàn bộ thư mục và lưu vào bảng `tracks` bằng `SQLiteTransaction` theo batch 500 bài. Thời gian nạp 10,000 bài từ ổ cứng chỉ mất ~3.5 giây.
* **Những lần khởi động tiếp theo**: 
  * Hoàn toàn **KHÔNG QUÉT ĐĨA**. 
  * Nạp trực tiếp 10,000 bài từ SQLite WAL mode lên UI trong **dưới 80 mili-giây**!
* **Khi người dùng bấm "Quét lại" (Rescan)**:
  * Sử dụng kỹ thuật đối soát nhanh: Lấy danh sách tệp trên đĩa và so sánh thời gian sửa đổi `File.GetLastWriteTimeUtc(path)` với cột `file_mtime` trong DB.
  * Chỉ phân tích tag ID3 cho những file mới thêm vào hoặc file bị đổi giờ sửa đổi. Bỏ qua 99.9% các file không đổi. Quá trình kiểm tra 10,000 file diễn ra trong vòng 200ms.

### 4.4. Độc lập Hoàn toàn Luồng Giao diện (Non-blocking UI Dispatcher)
* Quy tắc bất biến: **Không bao giờ gọi hàm I/O đĩa, truy vấn SQLite, hoặc gọi HTTP trên UI Thread.**
* Mọi thao tác đều thực thi thông qua `Task.Run` hoặc `async/await` với `.ConfigureAwait(false)`:
  * SQLite Repository trả về DTO nguyên bản.
  * Chỉ khi nạp vào `ObservableCollection` trên ViewModel mới đưa qua `Application.Current.Dispatcher.InvokeAsync`.
* Giữ cho tần số khung hình giao diện luôn ổn định ở mức **60 FPS**, đĩa vinyl xoay mượt mà và thanh Equalizer không bao giờ bị giật lag.

---

## 5. THIẾT KẾ CÁC INTERFACE & LỚP CỐT LÕI (C# ARCHITECTURAL CONTRACTS)

Toàn bộ các Interface này được đặt tại `src/MusicApp.Core/Interfaces/Persistence/`:

```csharp
namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Quản lý toàn bộ kho bài hát hợp nhất và tính toán điểm số gợi ý
    /// </summary>
    public interface ITrackRepository
    {
        Task<TrackEntity> GetByIdAsync(int id);
        Task<TrackEntity> GetByTrackKeyAsync(string trackKey);
        Task<IEnumerable<TrackEntity>> GetAllLocalTracksAsync();
        Task<IEnumerable<TrackEntity>> GetFavoritesAsync();
        Task<IEnumerable<TrackEntity>> SearchAsync(string keyword, int limit = 50);
        Task<IEnumerable<TrackEntity>> GetRecommendationsAsync(int limit = 25);
        Task<IEnumerable<TrackEntity>> GetSimilarTracksAsync(int trackId, int limit = 10);
        Task BatchInsertOrUpdateAsync(IEnumerable<TrackEntity> tracks);
        Task UpdateAffinityScoreAsync(int trackId, double deltaScore);
        Task ToggleFavoriteAsync(int trackId);
        Task IncrementPlayCountAsync(int trackId);
    }

    /// <summary>
    /// Quản lý danh sách phát tùy biến của người dùng
    /// </summary>
    public interface IPlaylistRepository
    {
        Task<IEnumerable<PlaylistEntity>> GetAllPlaylistsAsync();
        Task<PlaylistEntity> GetByIdAsync(int playlistId);
        Task<PlaylistEntity> CreatePlaylistAsync(string name, string description = null);
        Task DeletePlaylistAsync(int playlistId);
        Task AddTrackToPlaylistAsync(int playlistId, int trackId);
        Task RemoveTrackFromPlaylistAsync(int playlistId, int trackId);
        Task ReorderTracksAsync(int playlistId, IEnumerable<int> orderedTrackIds);
        Task<IEnumerable<TrackEntity>> GetTracksInPlaylistAsync(int playlistId);
    }

    /// <summary>
    /// Quản lý bộ nhớ đệm luồng âm thanh trực tuyến (Spotify CAS Cache)
    /// </summary>
    public interface IStreamCacheRepository
    {
        Task<string> GetCachedFilePathAsync(string trackHash);
        Task RegisterCacheFileAsync(string trackHash, string filePath, long sizeBytes, bool isFull);
        Task TouchCacheAccessAsync(string trackHash);
        Task EvictOldestCacheAsync(long targetFreedBytes);
        Task<long> GetTotalCacheSizeAsync();
    }

    /// <summary>
    /// Ghi nhận hành vi người dùng để cập nhật mô hình học sở thích
    /// </summary>
    public interface IInteractionRepository
    {
        Task LogInteractionAsync(int trackId, string actionType, int durationPlayed);
        Task<IEnumerable<UserInteractionEntity>> GetRecentInteractionsAsync(int limit = 100);
    }
}
```

---

## 6. LỘ TRÌNH TRIỂN KHAI THEO PHASES (ROADMAP CHUẨN XÁC)

Dựa trên cấu trúc kế hoạch tổng thể `PLAN_NANG_CAP_TOAN_DIEN.md`, chúng ta bổ sung các nội dung kiến trúc trên vào lộ trình thực thi:

### Giai đoạn 6.1: Nền tảng SQLite WAL & Unified Track Schema (Tuần 1)
1. Thêm gói NuGet `System.Data.SQLite.Core` (v1.0.118) vào [src/MusicApp.Core](file:///home/nhattu/WorkSpace/MyProjects/musicappbyNT/src/MusicApp.Core/MusicApp.Core.csproj).
2. Viết `DatabaseInitializer.cs`: Thiết lập kết nối SQLite, tự động tạo 8 bảng, kích hoạt chế độ WAL, seed 8 preset EQ mặc định.
3. Hiện thực hóa các Repositories: `TrackRepository`, `PlaylistRepository`, `SettingsRepository`, `StreamCacheRepository`.
4. Viết trọn bộ Unit Tests cho tầng Database trong `tests/MusicApp.Tests/`.

### Giai đoạn 6.2: Tích hợp Quét Gia Tăng & Hàng Đợi Bền Vững (Tuần 2)
1. Tích hợp `TrackRepository` vào [LocalLibraryService.cs](file:///home/nhattu/WorkSpace/MyProjects/musicappbyNT/src/MusicApp.Core/Services/LocalLibraryService.cs): Kích hoạt tính năng Quét gia tăng (Incremental Scan) và nạp tức thì từ DB khi mở app.
2. Tích hợp `PlayQueueRepository` vào [PlayQueueViewModel.cs](file:///home/nhattu/WorkSpace/MyProjects/musicappbyNT/MusicApp/ViewModels/PlayQueueViewModel.cs): Tự động lưu và khôi phục toàn bộ hàng đợi phát nhạc khi tắt/mở app.
3. Lưu trạng thái phiên làm việc (Session State: bài hát đang phát dở, vị trí giây, âm lượng).

### Giai đoạn 6.3: Động cơ Gợi ý & Trải nghiệm Spotify (Tuần 3)
1. Bổ sung bộ lắng nghe tương tác (Interaction Listener) trong `NowPlayingViewModel`: Bắt các sự kiện `PlayComplete`, `Skip`, `Heart`.
2. Hiện thực hóa thuật toán chấm điểm `AffinityScore` và thuật toán `Smart Shuffle`.
3. Bổ sung màn hình `FavoritesView.xaml` (Danh sách bài hát yêu thích) và `PlaylistsView.xaml` (Tạo và quản lý Playlist cá nhân).
4. Tích hợp cơ chế Cache tệp nhị phân trên đĩa cho `StreamController`.

### Giai đoạn 6.4: Tối ưu Tốc độ Toàn diện (Zero-Latency Polish) (Tuần 4)
1. Bật Virtualization và Recycling cho toàn bộ danh sách XAML.
2. Áp dụng bộ giải mã Thumbnail 120px và `BitmapImage.Freeze()` cho ảnh bìa.
3. Kiểm thử tải thư viện 10,000 bài hát để đảm bảo RAM luôn dưới 100MB và UI luôn đạt 60 FPS.
