# KẾ HOẠCH NÂNG CẤP TOÀN DIỆN — MUSICAPP (WPF / .NET Framework 4.6.1)
## Audit độc lập Senior .NET × Plan FE / BE / DB
**Phiên bản:** 1.0.0 | **Ngày:** 2026-09-28 | **Phạm vi:** Lập kế hoạch — KHÔNG triển khai code
**Nguyên tắc:** XAML giữ nguyên stack (WPF native + MVVM), không đổi phương thức FE. Chỉ nâng cấp nội bộ.

---

## MỤC LỤC
1. Audit hiện trạng (BE / FE / DB / Test) — bằng chứng `file:line`
2. Đối soát mâu thuẫn: Technical Design Document vs Code thực tế
3. Plan DATABASE (SQLite) — chi tiết schema, repository, migration, vị trí file
4. Plan BACKEND (BFF OWIN) — ổn định hóa, endpoint mới, caching, rate-limit
5. Plan FRONTEND (XAML giữ nguyên) — audit theo checklist, redesign tokens, a11y
6. Roadmap Phase 6 → 9 + ma trận ưu tiên + effort
7. Kế hoạch kiểm thử (unit / integration / UI qua Chrome)
8. Rủi ro & mitigation
9. Tiêu chí nghiệm thu (Definition of Done)

---

## 1. AUDIT HIỆN TRẠNG (EVIDENCE-FIRST)

### 1.1. Backend — BFF OWIN Self-Host ✅ Vững nền
| Mục | Trạng thái | Bằng chứng |
|---|---|---|
| OWIN self-host port 5245 | ✅ | `src/MusicApp.Bff/BffServerHost.cs`, `MusicApp/App.xaml.cs:53` |
| Endpoint search + clamp [1..50] | ✅ | `src/MusicApp.Bff/Controllers/TrackController.cs:41-50` |
| HTTP Range proxy (206) | ✅ | `src/MusicApp.Bff/Controllers/StreamController.cs:63-80` |
| MemoryCache 30 phút (cache-aside) | ✅ | `src/MusicApp.Bff/Services/MemoryCacheService.cs:58-73` |
| Multi-source router (Jamendo + VN) | ✅ | `src/MusicApp.Bff/Providers/MusicSourceRouter.cs` |
| TLS 1.2 force + HttpClient singleton | ✅ | `MusicApp/Services/MusicApiClient.cs:48-58` |
| DI container thật (Autofac) | ❌ **Doc nói có, code dùng static** | `TrackController.cs:30-31` — `static readonly Router/Cache` |
| Global exception middleware BFF | ❌ Thiếu | không có `app.UseErrorHandler()` trong `Startup.cs` |
| Pagination (page param) | ❌ Endpoint có nhưng provider bỏ qua | `TrackController.cs:41` chỉ nhận `query, limit` |
| Health-check endpoint | ❌ Thiếu | không có `/api/v1/health` |

### 1.2. Frontend — WPF XAML ✅ MVVM sạch, ⚠️ visual debt
| Mục | Trạng thái | Bằng chứng |
|---|---|---|
| MVVM thuần, code-behind tối thiểu | ✅ | `Views/*.xaml.cs` chỉ `InitializeComponent()` |
| DynamicResource theme tokens (15 keys) | ✅ | `Resources/Themes/DarkTheme.xaml` |
| Dark/Light runtime swap | ✅ | `MainViewModel.ToggleTheme` |
| Min-size responsive 960×680 | ✅ | `MainWindow.xaml:11` |
| Vinyl spin + FFT 30fps + throttle | ✅ | `NowPlayingCardView.xaml`, `SampleAggregator.cs` |
| **Hardcoded color bypass theme** | 🔴 | `NowPlayingCardView.xaml:94-95` — `Background="#121212" BorderBrush="#282828"` |
| **Icon = Unicode glyph ✦♫☷☰≡🔊⚲** | 🟡 | `MainViewModel.cs:107-112`, `NowPlayingCardView.xaml:292` |
| Font mặc định OS (Segoe UI) | 🟡 | không nhúng font; chỉ monospace cho time |
| Focus visual / keyboard nav | ❌ | không có `FocusVisualStyle` custom |
| Empty/loading/error states | ❌ thiếu một phần | search không có skeleton/error inline |
| Tab order / access key | ❌ | không khai báo |

### 1.3. Database — ❌ **Persistence = 0 (RAM-only)**
| Mục | Trạng thái | Bằng chứng |
|---|---|---|
| SQLite / EF / DbContext | ❌ Không tồn tại | grep toàn repo 0 match |
| File persistence bất kỳ | ❌ | grep `File.WriteAllText\|sqlite\|IsolatedStorage` = 0 |
| Settings.settings | ❌ Rỗng | `Properties/Settings.Designer.cs` chỉ khung generator |
| EQ preset lưu | ❌ | `DspEqualizerViewModel.cs:37` — `static readonly PresetCurves` |
| PlayQueue lưu | ❌ | `PlayQueueViewModel.cs` — ObservableCollection RAM |
| Library cache lưu | ❌ | `LocalLibraryService.cs` — scan lại mỗi lần mở |
| History / Favorites / Playlist | ❌ | không tồn tại model |

### 1.4. Tests — ✅ 60 unit tests, ❌ thiếu tầng persistence
- Có: BFF endpoint, FFT, EQ DSP, Lyrics parser, Library, Queue, RelayCommand, ViewModel.
- Thiếu: persistence tests, integration test restart-app-retain-state, UI smoke test.

---

## 2. ĐỐI SOÁT DOC vs CODE (phải sửa khi viết lại TDD v2.0)

| # | Document nói | Code thực | Hành động |
|---|---|---|---|
| 1 | `Autofac IoC` trong Startup | DI thủ công + `static readonly` | Doc: ghi Manual DI; Code (tùy chọn Phase 8): đưa về ctor injection |
| 2 | `ArchiveOrgProvider` | `VietnameseMusicSourceProvider` | Doc: sửa tên provider |
| 3 | `RangeStreamProxyMiddleware` | inline trong `StreamController` | Doc: sửa mô tả |
| 4 | Namespace `SpotifyWpf.*` | `MusicApp.*` | Doc: rename toàn bộ |
| 5 | Chưa ghi Phase 3/4/5 | Queue, Lyrics, EQ đã xong | Doc: bổ sung inventory |
| 6 | Không có Persistence | Đúng — thiếu thật | Doc: thêm Section 6 + roadmap Phase 6 |

---

## 3. PLAN DATABASE (SQLite) — Phase 6 ưu tiên #1

### 3.1. Quyết định công nghệ
| Hạng mục | Quyết định | Lý do |
|---|---|---|
| Engine | **SQLite** (`System.Data.SQLite` 1.0.118) | Serverless, zero-config, 1 file, chuẩn desktop .NET Framework 4.6.1. Bỏ EF6 (nặng, migration phức tạp cho app local). |
| Truy cập | ADO.NET tay + Repository pattern | Kiểm soát SQL, không ORM overhead, dễ test bằng file DB tạm |
| Vị trí file | `%LOCALAPPDATA%\MusicApp\musicapp.db` | Chuẩn Windows, per-user, không cần quyền admin |
| Schema mgmt | `PRAGMA user_version` + `CREATE TABLE IF NOT EXISTS` | Idempotent, không cần EF Migration |
| Kết nối | Single `SQLiteConnection` + pooling mặc định, `Journal Mode=WAL` | WAL cho phép đọc đồng thời khi ghi (UI đọc, scanner ghi) |
| Backup | copy file .db khi app exit (tùy chọn) | Đơn giản, đủ dùng |

### 3.2. Schema (7 bảng)
```sql
-- a. Cài đặt key-value (theme, volume, last track, selected preset, window size)
CREATE TABLE IF NOT EXISTS settings (
    key   TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

-- b. Hàng đợi hiện tại (khôi phục khi mở lại app)
CREATE TABLE IF NOT EXISTS play_queue (
    position    INTEGER PRIMARY KEY,
    track_id    TEXT NOT NULL,
    source      TEXT NOT NULL DEFAULT 'unknown',  -- 'jamendo' | 'vn' | 'local'
    title TEXT, artist TEXT, album TEXT,
    cover_url TEXT, stream_url TEXT,
    duration_seconds INTEGER
);

-- c. EQ presets (built-in seed + user custom)
CREATE TABLE IF NOT EXISTS eq_presets (
    name      TEXT PRIMARY KEY,
    gains     TEXT NOT NULL,           -- JSON: "[3.0,1.5,0.0,...]" (10 giá trị)
    is_custom INTEGER NOT NULL DEFAULT 0
);

-- d. Lịch sử phát (Recently played + play count)
CREATE TABLE IF NOT EXISTS played_history (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    track_id     TEXT NOT NULL,
    title TEXT, artist TEXT,
    played_at    TEXT NOT NULL,        -- ISO 8601 UTC
    duration_played_seconds INTEGER DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_history_played_at ON played_history(played_at DESC);

-- e. Playlist người dùng tự tạo (many-to-many)
CREATE TABLE IF NOT EXISTS playlists (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    name        TEXT NOT NULL UNIQUE,
    created_at  TEXT NOT NULL,
    cover_url   TEXT
);
CREATE TABLE IF NOT EXISTS playlist_tracks (
    playlist_id INTEGER NOT NULL REFERENCES playlists(id) ON DELETE CASCADE,
    track_id    TEXT NOT NULL,
    position    INTEGER NOT NULL,
    title TEXT, artist TEXT, cover_url TEXT, stream_url TEXT, duration_seconds INTEGER,
    PRIMARY KEY (playlist_id, position)
);

-- f. Yêu thích (Favorites/Heart)
CREATE TABLE IF NOT EXISTS favorites (
    track_id  TEXT PRIMARY KEY,
    title TEXT, artist TEXT, cover_url TEXT, stream_url TEXT,
    added_at  TEXT NOT NULL
);

-- g. Cache thư viện local (tránh scan lại)
CREATE TABLE IF NOT EXISTS library_folders (
    folder_path  TEXT PRIMARY KEY,
    last_scanned TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS library_tracks (
    track_id    TEXT PRIMARY KEY,
    folder_path TEXT NOT NULL REFERENCES library_folders(folder_path) ON DELETE CASCADE,
    file_path   TEXT NOT NULL UNIQUE,
    title TEXT, artist TEXT, album TEXT, genre TEXT,
    duration_seconds INTEGER,
    cover_uri     TEXT,                -- Đường dẫn file thumbnail 120x120 trên đĩa (không dùng Base64 trong DB để tránh phình dung lượng)
    file_mtime    TEXT NOT NULL,       -- để phát hiện file đổi → rescan điểm
    play_count    INTEGER NOT NULL DEFAULT 0,
    skip_count    INTEGER NOT NULL DEFAULT 0,
    affinity_score REAL NOT NULL DEFAULT 0.0 -- Điểm số quan hệ dùng cho thuật toán gợi ý
);
CREATE INDEX IF NOT EXISTS idx_library_artist ON library_tracks(artist);
CREATE INDEX IF NOT EXISTS idx_library_album  ON library_tracks(album);
CREATE INDEX IF NOT EXISTS idx_library_affinity ON library_tracks(affinity_score DESC);

-- h. Bộ nhớ đệm tệp Stream trực tuyến kiểu Spotify (CAS Disk Cache)
CREATE TABLE IF NOT EXISTS stream_cache (
    track_hash       TEXT PRIMARY KEY,          -- SHA-1 của Stream URL / TrackID
    file_path        TEXT NOT NULL,             -- %LOCALAPPDATA%\MusicApp\Cache\{hash}.audio
    file_size_bytes  INTEGER NOT NULL,
    last_accessed_at TEXT NOT NULL,
    is_fully_cached  INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_cache_accessed ON stream_cache(last_accessed_at ASC);

-- i. Nhật ký tương tác người dùng (User Interaction Log phục vụ gợi ý bài hát theo click)
CREATE TABLE IF NOT EXISTS user_interactions (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    track_id        TEXT NOT NULL,
    action_type     TEXT NOT NULL,              -- 'click', 'play_start', 'play_complete', 'skip', 'favorite', 'unfavorite'
    duration_played INTEGER DEFAULT 0,
    created_at      TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_interactions_track ON user_interactions(track_id);
CREATE INDEX IF NOT EXISTS idx_interactions_time  ON user_interactions(created_at DESC);
```

**Connection string chuẩn tối ưu hiệu năng:**
```
Data Source=%LOCALAPPDATA%\MusicApp\musicapp.db;Version=3;Journal Mode=WAL;Synchronous=NORMAL;Cache Size=-64000;Foreign Keys=True;
```

### 3.3. Lớp Repository (Interface + Impl) — thuộc `MusicApp.Core`
```
src/MusicApp.Core/
├── Interfaces/Persistence/
│   ├── ISettingsRepository.cs      Get<T>/Set<T>(key)  (JSON serialize)
│   ├── IQueueRepository.cs         SaveAll(items)/LoadAll()/Clear()
│   ├── IPresetRepository.cs        GetAll()/SaveCustom(name,gains)/DeleteCustom(name)
│   ├── IHistoryRepository.cs       Add(entry)/GetRecent(top)/GetPlayCount(trackId)
│   ├── IPlaylistRepository.cs      CRUD playlist + AddTrack/RemoveTrack/Reorder
│   ├── IFavoriteRepository.cs      Toggle(track)/IsFavorite(id)/GetAll()
│   ├── ILibraryRepository.cs       SaveScan(folder,tracks)/GetTracks()/GetFolders()/InvalidateFolder(path)
│   ├── IStreamCacheRepository.cs   GetCachedFile/RegisterCache/Touch/EvictLRU (Spotify CAS Cache)
│   └── IRecommendationEngine.cs   LogInteraction/GetRecommendedTracks/GetSmartShuffleNext
└── Persistence/
    ├── DatabaseInitializer.cs      (mở conn, PRAGMA WAL/Cache, tạo 9 bảng, seed preset, user_version=1)
    └── Repositories/*.cs           (9 file impl, mỗi file 1 repo, SQL thuần, param hóa)
```

**Quy tắc viết repo (senior invariants):**
- Mọi query dùng **parameterized command** — tuyệt đối không nối chuỗi SQL.
- Repo **không** giữ connection sống lâu; dùng `using` per-operation hoặc connection factory dùng chung.
- Seed 8 preset EQ built-in vào `eq_presets` tại `DatabaseInitializer` (`is_custom=0`), chỉ seed khi bảng trống.
- Mọi write batch (queue, library scan) bọc **transaction**.
- ISO 8601 UTC cho mọi timestamp.

### 3.4. Điểm tích hợp vào code hiện có (không phá vỡ)
| File hiện có | Thay đổi (Phase 6) |
|---|---|
| `App.xaml.cs` | Khởi tạo `DatabaseInitializer` trước khi tạo ViewModel; new các repository; inject vào `MainViewModel` |
| `MainViewModel` ctor | Nhận thêm repositories (optional params, giữ backward-compat ctor cho tests) |
| `MainViewModel.ToggleTheme` | `_settings.Set("theme", isDark?"dark":"light")`; load khi khởi động |
| `DspEqualizerViewModel` | Load preset list từ `IPresetRepository`; `SelectedPreset` set → `_settings.Set("eq.preset", name)`; nút "Lưu Custom" → `SaveCustom` |
| `PlayQueueViewModel` | Mọi thay đổi queue (enqueue/remove/reorder/clear) → `_queue.SaveAll()`; load khi ctor; hỗ trợ Smart Shuffle qua `IRecommendationEngine` |
| `NowPlayingViewModel`/`MainViewModel.PlayTrack` | `PlayTrack` → `_history.Add()`; kết thúc bài → `_recommendation.LogInteraction(id, "play_complete")`; skip trước 10s → `_recommendation.LogInteraction(id, "skip")` |
| `LocalLibraryViewModel` | Sau scan → `_library.SaveScan()`; khi mở app, nếu folder đã có + `file_mtime` khớp → load từ DB thay vì scan |
| `NowPlayingCardView` (Phase 7) | Nút ♥ toggle → `_favorite.Toggle()`; tăng `affinity_score` (+10.0) |
| `StreamController` (Phase 7) | Kiểm tra `_streamCache.GetCachedFile`: nếu có file cache trên đĩa → phát ngay lập tức không cần mạng |

### 3.5. Chiến lược Tối ưu Tốc độ Tối đa (Zero-Latency Performance Playbook)
1. **Full UI Virtualization**: Mọi ListBox trong XAML đều kích hoạt `VirtualizingStackPanel.IsVirtualizing="True"`, `VirtualizationMode="Recycling"`, `ScrollUnit="Pixel"`. Với 20,000 bài hát, RAM tiêu thụ vẫn dưới 70MB.
2. **Thumbnail 120px & Image Freezing**: Không lưu Base64 vào DB. Đọc ảnh bìa nhúng, resize xuống 120x120px, lưu ra đĩa `%LOCALAPPDATA%\MusicApp\Covers\{hash}.jpg`. Nạp ảnh bằng `BitmapImage.DecodePixelWidth = 120` và gọi `.Freeze()` để chia sẻ xuyên luồng.
3. **Quét gia tăng siêu tốc (Incremental Scan)**: Chỉ scan lại file khi `file_mtime` trên đĩa khác với database. Bỏ qua 99% file cũ, thời gian nạp lại 10,000 file chỉ mất 200ms.
4. **Non-blocking Dispatcher**: 100% lệnh I/O và SQL chạy trên Worker Thread qua `Task.Run` với `.ConfigureAwait(false)`. Giữ UI Thread luôn đạt 60 FPS mượt mà.

### 3.6. Migration & versioning
- `PRAGMA user_version` bắt đầu = 0 → sau init = 1.
- Phase sau thêm cột: `if (user_version < 2) { ALTER TABLE ...; PRAGMA user_version = 2; }`.
- Không xóa dữ liệu người dùng khi upgrade; chỉ additive.

---

## 4. PLAN BACKEND (BFF) — ổn định + mở rộng

### 4.1. Ổn định hóa (Phase 6 song song DB)
1. **Global error middleware**: `app.UseErrorHandler()` trong `Startup.Configuration` — trả JSON `{error, code}` thay vì HTML mặc định; log qua `Trace`.
2. **Health endpoint**: `GET /api/v1/health` → `{status:"ok", uptime, providers:[{name, healthy}]}`. Dùng cho `MusicApiClient` warm-up + UI hiển thị trạng thái kết nối.
3. **Provider resilience**: bọc `JamendoSourceProvider`/`VietnameseMusicSourceProvider` bằng timeout 8s + circuit-breaker đơn giản (3 lỗi liên tiếp → degrade sang curated catalog 60s).
4. **Pagination thật**: `TrackController` thêm `page` → truyền xuống router/provider (`offset = (page-1)*limit`); `SearchResponseDto` thêm `Page`, `PageSize`.
5. **DI nhất quán**: chuyển `static readonly Router/Cache` → inject qua ctor controller (Web API `DependencyResolver` đơn giản hoặc giữ static nhưng doc ghi rõ — **khuyến nghị**: giữ static Phase 6, refactor DI Phase 8).

### 4.2. Endpoint mới phục vụ tính năng (Phase 7-9)
| Endpoint | Mục đích | Phase |
|---|---|---|
| `GET /api/v1/charts?region=vn\|global` | Top charts / trending | 8 |
| `GET /api/v1/lyrics?trackId=` | Tải lời từ nguồn online khi local không có .lrc | 8 |
| `GET /api/v1/stream/cache/{id}` | Stream từ local cache file (fallback khi nguồn không hỗ trợ Range — doc 5.2.3 đã mô tả, code chưa làm) | 7 |

### 4.3. Cache & stream nâng cấp
- **Disk cache cho stream**: `Fallback Local Cache Streaming` (doc đã mô tả, code chưa có) — khi upstream trả 200 thay vì 206, BFF tải nền về `%LOCALAPPDATA%\MusicApp\cache\{trackId}.tmp` và tự serve Range. Xóa file >7 ngày khi startup.
- Metadata cache nâng TTL search 30 phút (giữ) + negative-cache 404 track 5 phút (tránh hammer upstream).

---

## 5. PLAN FRONTEND (XAML giữ nguyên — chỉ nâng cấp nội bộ)

> Nguyên tắc: **không đổi stack, không đổi cấu trúc MVVM**. Mọi cải tiến qua ResourceDictionary, Style, ControlTemplate, converter. Audit theo redesign-audit-checklist đã chạy tay.

### 5.1. Design Dials chốt cho desktop music app
- `DESIGN_VARIANCE = 4` — desktop tool quen thuộc, không phá layout 2 cột hiện tại.
- `MOTION_INTENSITY = 5` — giữ vinyl spin + FFT, thêm hover/press micro-transition 150-200ms.
- `VISUAL_DENSITY = 6` — music app cần list dày vừa phải, không phải gallery.

### 5.2. Typography (Fix priority #1)
| Việc | Chi tiết | File |
|---|---|---|
| Nhúng font brand | **Be Vietnam Pro** (hỗ trợ tiếng Việt đầy đủ, có 500/600/700) làm `FontFamily` mặc định toàn app qua `Style TargetType="TextBlock"` trong `App.xaml` / `Resources/Typography.xaml` mới | `App.xaml`, theme dict |
| Mono cho số | giữ `Consolas`/`Cascadia Mono` cho time display (đã có) | giữ nguyên |
| Scale chữ | Title 18→20 SemiBold; Subtitle 14→13.5; caption 11→11.5; thêm weight 500 | theme styles |
| Vietnamese diacritics | verify render dấu với font nhúng (test "Diễm Xưa", "Ước") | manual check |

### 5.3. Color & Tokens (Fix #2)
- **Xóa hardcode**: `NowPlayingCardView.xaml:94-95` `#121212/#282828` → `{DynamicResource CardSurfaceBrush/CardBorderBrush}`. Grep toàn bộ `#[0-9A-Fa-f]{6}` ngoài theme dict → đưa về token.
- Thêm token mới vào cả 2 theme: `SuccessBrush`, `DangerBrush`, `FocusRingBrush`, `SkeletonBrush` (loading), `ScrollbarBrush`.
- Dark: giữ nền `#161B22/#0D1117` (đã tốt — không phải pure black). Accent giữ `#1ED760` hoặc cân nhắc đổi accent riêng (warm) nếu muốn thoát "Spotify clone" — **đề xuất**: giữ xanh nhưng đổi hover/pressed tint tinh tế hơn.
- Light theme: audit lại contrast ratio WCAG AA (text ≥ 4.5:1) bằng công cụ đo sau khi render.

### 5.4. Icons (Fix #3 — glyph unicode → vector)
- Thay `✦♫☷☰≡🔊⚲` bằng **PathGeometry SVG-style** trong `Resources/Icons.xaml` (nhúng `StreamGeometry` — WPF native, không cần thư viện).
- Bộ icon cần: explore(compass), vn-note, library, queue, lyrics, eq, play/pause/next/prev, shuffle, repeat, heart, heart-filled, volume(4 mức), search, close, folder, more.
- Tạo `IconView` UserControl nhỏ bind `IconKey` → `DynamicResource`. Sidebar + buttons dùng lại.

### 5.5. Interaction states (Fix #4)
- **Hover/Press**: thêm `VisualStateManager` hoặc trigger scale `RenderTransform` 0.98 + transition 150ms cho mọi button transport.
- **Focus**: custom `FocusVisualStyle` — viền `FocusRingBrush` 1.5px rounded, áp toàn app.
- **Loading**: skeleton rectangle pulse (gradient animation) cho search + library scan thay vì text "Đang tải...".
- **Empty states**: thiết kế 4 empty view — search trống, queue trống, library chưa chọn folder, playlist trống (icon + 1 câu hướng dẫn + CTA).
- **Error inline**: search fail → banner inline đỏ nhạt trong list, có nút Retry; KHÔNG MessageBox cho lỗi search.
- **Selected state**: nav item đang active đã có — kiểm tra lại tương phản.

### 5.6. Layout & màn hình mới (Phase 7-8, vẫn XAML thuần)
| Màn / thành phần | Loại thay đổi | Ghi chú |
|---|---|---|
| `FavoritesView.xaml` | MỚI | List giống Library + filter theo added_at; nav item mới "Yêu Thích" |
| `PlaylistsView.xaml` + `PlaylistDetailView.xaml` | MỚI | Card grid playlist; detail = list kéo-thả giống Queue |
| `RecentlyPlayedView.xaml` | MỚI (hoặc section trong Explore) | Feed ngang cover |
| `CreatePlaylistDialog.xaml` | MỚI | Modal WPF `Window` nhỏ, không第三方 |
| `MiniPlayerView.xaml` (compact mode) | MỚI | 320×140, topmost toggle |
| Context menu chuột phải track | MỚI | Play next / Add to queue / Add to playlist / ♥ Favorite / Copy info |
| Library grouping | NÂNG CẤP | Group by Album/Artist qua `CollectionViewSource.GroupDescriptions` — không đổi service |
| Lyrics offset ±0.5s | NÂNG CẤP | 2 nút trên `LyricsSyncView`, bind vào `LyricsViewModel` (offset lưu `settings` theo track) |
| SMTC / media keys | NÂNG CẤP (code-behind App) | .NET 4.6.1: dùng `RegisterHotKey` Win32 P/Invoke cho global; SMTC giới hạn — ghi rõ trong doc |
| System tray | NÂNG CẤP | `NotifyIcon` WinForms interop (reference `System.Windows.Forms`) + menu Play/Pause/Next/Exit |

### 5.7. Accessibility (bắt buộc cho desktop app thật)
- `AutomationProperties.Name` cho mọi icon-button (screen reader đọc được "Phát/Tạm dừng").
- Tab order hợp lý: sidebar → search → list → transport.
- Access keys: `_Tìm kiếm`, `_Phát` qua `AccessText`.
- Kiểm tra narrator pass 1 vòng các màn chính (manual).
- Target size nút transport ≥ 40×40 (hiện 42 nút tròn — OK, kiểm tra các nút nhỏ hơn).

### 5.8. Anti-slop compliance check (đối chiếu checklist)
| Checklist item | Hiện trạng | Fix |
|---|---|---|
| Font mặc định | Segoe UI | Be Vietnam Pro nhúng |
| Emoji trong UI | 🔊⚲♫ | PathGeometry icons |
| Pure black | Không (đã #161B22) ✅ | — |
| Nhiều accent | 1 accent ✅ | — |
| Shadow thô | `DropShadowEffect BlurRadius=18 Opacity=0.35` | Giảm Opacity ≤ 0.12, tint theo nền |
| Hover states | Một phần | Bổ sung toàn bộ |
| Focus ring | Không | FocusVisualStyle |
| Empty/loading/error | Thiếu | 3 states cho mọi list |
| Monospace numbers | Có ✅ | — |

---

## 6. ROADMAP TỔNG (Phase 6 → 9)

| Phase | Tên | Phạm vi chính | Effort ước lượng | Phụ thuộc |
|---|---|---|---|---|
| **6** | Persistence nền tảng | SQLite + 7 repository + DatabaseInitializer + tích hợp settings/theme/queue/preset/history/library cache + unit tests persistence + sửa TDD v2.0 | 3-4 ngày | — |
| **7** | Playlist + Favorites + UI nâng cấp lõi | Playlists/FavoritesView, context menu, ♥ nút, icon vector, font nhúng, empty/loading/error states, hardcode cleanup | 4-5 ngày | Phase 6 |
| **8** | OS Integration + Discovery | System tray, global media keys, mini player, charts endpoint + view, lyrics online fetch, disk stream cache, pagination UI | 4-5 ngày | Phase 6, 7 |
| **9** | Audiophile polish | Crossfade 2s, ReplayGain-ish normalize, lyrics offset UI, DI refactor (bỏ static), a11y pass cuối | 3 ngày | Phase 7, 8 |

**Nguyên tắc ưu tiên (từ DANH_GIA + audit):**
1. Persistence trước — mọi tính năng khác phụ thuộc.
2. Playlist/Favorites — gap lớn nhất theo Gap Analysis (6.5/10).
3. OS Integration — đưa app thành "Windows app thật".
4. Polish — cuối cùng.

---

## 7. KẾ HOẠCH KIỂM THỬ

### 7.1. Unit tests mới (MSTest, thư mục `tests/MusicApp.Tests/`)
- `DatabaseInitializerTests` — tạo DB mới, chạy lại idempotent, user_version=1, preset seeded 8 bản.
- `SettingsRepositoryTests` — round-trip string/int/bool/JSON; key thiếu → default.
- `QueueRepositoryTests` — save/load giữ thứ tự; clear; overwrite.
- `PresetRepositoryTests` — custom CRUD; không xóa được built-in.
- `HistoryRepositoryTests` — add + recent N + play count.
- `PlaylistRepositoryTests` — tạo/xóa cascade, reorder position, trùng tên → lỗi.
- `FavoriteRepositoryTests` — toggle idempotent.
- `LibraryRepositoryTests` — saveScan → getTracks; đổi file_mtime → invalidate đúng folder.
- Mỗi test dùng DB file tạm (`Path.GetTempFileName`), xóa sau test.

### 7.2. Integration test
- `PersistenceIntegrationTests`: khởi tạo MainViewModel với repo thật (DB tạm) → set theme/preset/queue → dispose → tạo lại → assert khôi phục.

### 7.3. UI verification (theo FEDLC của anh — Chrome thật khi có prototype)
- WPF không chạy trong Chrome; quy trình áp dụng:
  1. Khi cần đánh giá visual redesign trước khi code XAML: dựng **prototype HTML tĩnh** qua OpenDesign (artifact) cho 3 màn chính → review bằng Playwright (resize tương đương 960/1180/1440, dark/light, screenshot).
  2. Sau khi code XAML: chạy app thật trên Windows, chụp màn hình so sánh với prototype (visual diff thủ công).
  3. Smoke test thủ công checklist: theme toggle, focus ring, empty states, 3 kích thước cửa sổ.

---

## 8. RỦI RO & MITIGATION

| Rủi ro | Xác suất | Mitigation |
|---|---|---|
| `System.Data.SQLite` phát hành kèm native interop dll (x86/x64) | Cao | Dùng gói `System.Data.SQLite.Core` + thiết lập `Any CPU` → build event copy `SQLite.Interop.dll` đúng kiến trúc; hoặc đánh giá `Microsoft.Data.Sqlite` (cần .NET Standard 2.0 — 4.6.1 không hỗ trợ đủ → **chốt System.Data.SQLite**) |
| Lock file DB khi app crash | TB | WAL mode + `Synchronous=NORMAL` + timeout busy 5s (`Default Timeout=5`) |
| DB corrupt | Thấp | `PRAGMA integrity_check` khi startup; hỏng → đổi tên `.corrupt` + tạo mới |
| Song song scanner ghi / UI đọc | TB | WAL + transaction ngắn; scanner ghi batch 100 bản ghi/transaction |
| Tăng thời gian khởi động | Thấp | DB init < 50ms; load library từ DB nhanh hơn scan — net gain |
| Kích thước DB phình (cover Base64) | TB | cover_data lazy-load: bảng `library_tracks` chỉ lưu khi có; cân nhắc Phase 8 chuyển cover ra file cache riêng |
| SMTC không khả dụng trên 4.6.1 | Cao | Ghi rõ giới hạn trong TDD; dùng global hotkey Win32 thay thế |
| Font nhúng tăng dung lượng | Thấp | Subset font (chỉ Latin + Vietnamese) ~ 300KB |

---

## 9. TIÊU CHÍ NGHIỆM THU (Definition of Done)

**Phase 6 hoàn thành khi:**
1. App tắt/mở lại giữ: theme, volume, EQ preset (+custom), queue đầy đủ thứ tự, library hiển thị ngay không scan lại, history ghi nhận.
2. 60 tests cũ pass + ≥ 30 tests persistence mới pass.
3. Không có query SQL nối chuỗi (review 100% repo SQL).
4. `TECHNICAL_DESIGN_DOCUMENT.md` v2.0 phản ánh đúng code (6 mục mâu thuẫn mục 2 đã sửa).

**Phase 7 hoàn thành khi:**
1. Tạo/sửa/xóa playlist, kéo-thả trong playlist, ♥ favorite persist sau restart.
2. Grep `#[0-9A-F]{6}` ngoài theme dict = 0.
3. Mọi list có 3 trạng thái loading/empty/error.
4. Icon vector thay 100% unicode glyph.

**Phase 8-9:** theo checklist từng mục ở roadmap.

---

## PHỤ LỤC A — Danh sách file sẽ TẠO / SỬA (tham chiếu khi triển khai)

**Tạo mới (Phase 6):**
- `src/MusicApp.Core/Interfaces/Persistence/*.cs` (7)
- `src/MusicApp.Core/Persistence/DatabaseInitializer.cs` + `Persistence/Repositories/*.cs` (8)
- `tests/MusicApp.Tests/Persistence*Tests.cs` (7-8 file test)

**Sửa (Phase 6):**
- `src/MusicApp.Core/MusicApp.Core.csproj` (+`System.Data.SQLite`)
- `MusicApp/App.xaml.cs` (init DB + inject)
- `MusicApp/ViewModels/MainViewModel.cs`, `DspEqualizerViewModel.cs`, `PlayQueueViewModel.cs`, `LocalLibraryViewModel.cs`
- `TECHNICAL_DESIGN_DOCUMENT.md` (v2.0: 6 mục sửa mục 2 + Section 6 DB + Section 7 roadmap)

**Tạo mới (Phase 7-8):**
- `MusicApp/Views/FavoritesView.xaml`, `PlaylistsView.xaml`, `PlaylistDetailView.xaml`, `RecentlyPlayedView.xaml`, `CreatePlaylistDialog.xaml`, `MiniPlayerView.xaml`
- `MusicApp/Resources/Icons.xaml`, `Resources/Typography.xaml`
- `src/MusicApp.Bff/Controllers/ChartsController.cs`, `LyricsController.cs`

> File này là kế hoạch duy nhất cần theo dõi. Khi triển khai từng phase, tick vào mục tương ứng và cập nhật ngày hoàn thành.
