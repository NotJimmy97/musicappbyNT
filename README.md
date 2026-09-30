# MusicApp Desktop (musicappbyNT)

Ứng dụng phát nhạc Desktop Native hiệu năng cao được phát triển trên nền tảng Windows Presentation Foundation (WPF) với .NET Framework 4.6.1. Hệ thống vận hành dựa trên kiến trúc phân lớp hướng module (Modular Monolith), bao gồm trạm trung gian cục bộ OWIN Self-Host Backend-for-Frontend (BFF), engine xử lý âm thanh độ trễ thấp NAudio kết hợp bộ lọc cân bằng âm sắc 10 băng tần DSP (10-Band Graphic DSP Equalizer), cơ sở dữ liệu cục bộ SQLite tối ưu hóa chế độ ghi trước WAL, cùng động cơ đề xuất bài hát thông minh Smart Shuffle dựa trên phân phối xác suất Boltzmann.

---

## 1. Thiết Kế Kiến Trúc Hệ Thống (System Architecture)

Hệ thống tuân thủ nghiêm ngặt mô hình MVVM (Model-View-ViewModel) kết hợp kiến trúc dịch vụ nội tiến trình (In-Process Services) và tầng lưu trữ dữ liệu bền vững (Persistence Layer), đảm bảo tính cô lập nghiệp vụ, loại trừ hiện tượng khóa luồng giao diện người dùng (WPF Dispatcher) và cho phép kiểm thử đơn vị độc lập.

```
[ Lớp Trình Diễn WPF Desktop (MVVM Pattern) ]
                       |
                       v
   +---------------------------+---------------------------+---------------------------+
   |                           |                           |                           |
   v                           v                           v                           v
[ Dịch Vụ Cục Bộ OWIN BFF ]    [ Engine Âm Thanh DSP ]     [ Tầng Lõi Nghiệp Vụ Core ] [ Tầng Lưu Trữ SQLite ]
- Máy chủ HTTP cổng 5245       - Bộ lọc BiQuad Peaking EQ  - Động cơ Smart Shuffle     - SQLite Engine (WAL Mode)
- Tìm kiếm & định tuyến nguồn  - Cầu nối SampleAggregator  - Phân tích cú pháp LRC     - Track / Playlist Repo
- HTTP 206 Partial Stream      - Phân tích phổ FFT 16 cột  - Quét thư mục LocalLibrary - Queue / Interaction Repo
- Chuẩn hóa chuỗi tiếng Việt   - Giải mã WaveOutEvent      - Bộ nhớ đệm tệp âm thanh   - Quản trị giao dịch ACID
```

---

### 1.1. Luồng Dữ Liệu Nghiệp Vụ (End-to-End Data Flows)

#### A. Luồng Tìm Kiếm & Nạp Danh Mục Trực Tuyến (Catalog & Search Flow)
1. Người dùng nhập từ khóa trên thanh tìm kiếm hoặc chọn bộ lọc phân loại thể loại (Pill Filter).
2. `MainViewModel` tiếp nhận sự kiện, điều phối bộ đếm trễ `CancellationTokenSource` (Debounce 300ms) nhằm triệt tiêu các yêu cầu dư thừa khi người dùng nhập liệu liên tục.
3. ViewModel kích hoạt `IMusicApiClient.SearchTracksAsync()` gửi yêu cầu HTTP GET đến máy chủ nội bộ `http://localhost:5245/api/v1/tracks/search?q={keyword}`.
4. Tại tầng BFF, `TrackController` điều phối `MusicSourceRouter` tổng hợp kết quả từ nguồn quốc tế Jamendo và danh mục âm nhạc Việt Nam tuyển chọn.
5. Giải thuật `RemoveDiacritics()` chuyển đổi chuỗi ký tự tiếng Việt về dạng chuẩn hóa phân tách (`FormD`), cho phép tìm kiếm chính xác đối với cả dữ liệu có dấu và không dấu mà không tạo độ trễ xử lý.
6. Kết quả trả về qua đối tượng `SearchResponseDto`, được ánh xạ sang `TrackItemViewModel` và hiển thị trên giao diện thông qua cơ chế ràng buộc `ObservableCollection`.

#### B. Luồng Phát Nhạc Trực Tuyến & Phân Đoạn Mạng (Streaming Playback Flow)
1. Khi kích hoạt lệnh phát, `MainViewModel` chuyển thông tin bài hát sang `NowPlayingViewModel`.
2. Dịch vụ âm thanh `NAudioService.InitializeAsync(streamUrl)` được kích hoạt trên luồng nền (`Task.Run`).
3. Dịch vụ phân tích giao thức URL:
   - Với URL mạng (HTTP/HTTPS): Khởi tạo `MediaFoundationReader` kết nối đến endpoint proxy stream của BFF (`/api/v1/tracks/{id}/stream`).
   - BFF hoạt động như một HTTP Streaming Proxy, gửi tiếp yêu cầu đến máy chủ phát gốc với tiêu đề HTTP `Range: bytes={start}-{end}`.
   - Nhận luồng nhị phân từng phần (`206 Partial Content`), chuyển tiếp tức thời về `MediaFoundationReader` mà không ghi đệm toàn bộ tệp vào RAM.
4. Đồng thời, `LocalAudioCacheService` thực hiện tải và lưu trữ tệp âm thanh vào bộ nhớ đệm cục bộ theo mã băm SHA-1 của URL nhằm phục vụ phát ngoại tuyến cho các phiên kế tiếp.
5. Chuỗi âm thanh chuyển trạng thái sang `PlaybackState.Playing`.

#### C. Luồng Quản Trị Dữ Liệu Bền Vững & Bài Hát Yêu Thích (Persistence Flow)
1. Cơ sở dữ liệu SQLite nội bộ (`musicapp.db`) được cấu hình tự động khi khởi động qua `DatabaseInitializer`.
2. Hệ thống kích hoạt cơ chế ghi nhật ký trước (Write-Ahead Logging - WAL), kích thước bộ nhớ đệm 64MB và thực thi khóa ngoại (Foreign Keys).
3. Mỗi bài hát được định danh duy nhất thông qua khóa logic chuẩn hóa sinh bởi `TrackIdentityHelper.GenerateTrackKey(title, artist)`.
4. Khi phát bài hát, `NowPlayingViewModel` kích hoạt tác vụ ngầm ghi nhận hoặc cập nhật `TrackEntity` vào bảng `tracks` và nạp trạng thái yêu thích (`is_favorite`).
5. Thao tác đánh dấu yêu thích (Heart) thực thi cập nhật nguyên tử trên SQLite và đồng bộ điểm tương tác (`affinity_score`).

#### D. Luồng Gợi Ý Thông Minh & Smart Shuffle (Recommendation Engine Flow)
1. Mọi tương tác của người dùng (`play_complete`, `play_30s`, `favorite`, `unfavorite`, `skip`, `add_playlist`) được ghi nhận bất đồng bộ vào bảng `user_interactions`.
2. `RecommendationEngine` tính toán trọng số tương tác (`delta`), tự động cập nhật trường `affinity_score` của bài hát trong bảng `tracks`.
3. Khi kích hoạt chế độ Smart Shuffle, giải thuật phân phối xác suất Boltzmann (Softmax Sampling) được kích hoạt trên tập hợp bài hát ứng viên:
   $$\Pr(i) = \frac{e^{\frac{S_i}{T}}}{\sum_{j} e^{\frac{S_j}{T}}}$$
   Trong đó $S_i$ là điểm `affinity_score`, $T$ là tham số nhiệt độ (Temperature) điều hòa giữa việc chọn bài hát quen thuộc (Exploitation) và khám phá bài hát mới (Exploration).

#### E. Luồng Quét & Phát Nhạc Ngoại Tuyến (Offline Local Library Flow)
1. Người dùng chọn thư mục thông qua hộp thoại chọn thư mục trong `LocalLibraryScannerView`.
2. `LocalLibraryViewModel` gọi `LocalLibraryService.ScanDirectoryAsync(path, progress, cancellationToken)`.
3. Dịch vụ áp dụng giải thuật duyệt theo chiều rộng (BFS Queue-based) quét đệ quy an toàn, xử lý triệt để ngoại lệ phân quyền (`UnauthorizedAccessException`) và giới hạn độ dài đường dẫn (`PathTooLongException`).
4. Thư viện `TagLibSharp` trích xuất thông tin siêu dữ liệu ID3 và mảng byte ảnh bìa.
5. `FrozenImageConverter` chuyển đổi ảnh thành `BitmapImage` và thực thi ngay phương thức `Freeze()`, đảm bảo tính bất biến (Immutable), cho phép chia sẻ an toàn giữa các luồng và giải phóng mảng byte thô trung gian.
6. Khi phát bài hát cục bộ, `NAudioService` khởi tạo `AudioFileReader` giải mã trực tiếp từ đĩa cứng.

#### F. Luồng Đồng Bộ Lời Bài Hát Thời Gian Thực (Synchronized Lyrics Flow)
1. `LyricsService.LoadLyricsForTrackAsync(track)` kiểm tra tệp `.lrc` đồng hành trên đĩa (`{trackPath}.lrc` hoặc `{title}.lrc`). Trường hợp không có tệp đĩa, nạp dữ liệu lời nhúng sẵn trong danh mục.
2. `LrcParser` bóc tách từng dòng lời, giải mã các thẻ mốc thời gian đa điểm `[mm:ss.xx]`, áp dụng độ lệch thời gian `[offset:+/-ms]` và sắp xếp theo trật tự thời gian tăng dần.
3. Bộ đếm `_positionTimer` trong `NowPlayingViewModel` phát tín hiệu mốc thời gian hiện tại mỗi 100ms.
4. `LyricsViewModel.UpdatePosition()` áp dụng giải thuật tìm kiếm nhị phân $O(\log N)$ định vị chính xác dòng lời tương ứng.
5. Cơ chế lọc sự kiện ngưỡng (Event Gating): Chỉ khi chỉ số dòng hát thực sự thay đổi (`newIndex != _activeLineIndex`), sự kiện kích hoạt mới được phát đi.
6. Code-behind `LyricsSyncView.xaml.cs` tính toán tọa độ thẳng đứng và điều phối `ScrollViewer.ScrollToVerticalOffset()` đưa dòng lời đang hát vào trung tâm màn hình với hiệu ứng chuyển động mượt mà.

---

### 1.2. Kiến Trúc Xử Lý Tín Hiệu Âm Thanh DSP (Audio Graph Pipeline)

Quá trình xử lý tín hiệu số (DSP) được xâu chuỗi tuần tự theo đồ thị âm thanh hướng luồng:

```
+-----------------------------------------------------------------------+
| Nguồn Giải Mã: AudioFileReader (Local) / MediaFoundationReader (HTTP) |
+-----------------------------------------------------------------------+
                                   | (Mẫu số thực 32-bit Float IEEE)
                                   v
+-----------------------------------------------------------------------+
| DspEqualizerSampleProvider (Bộ Cân Bằng 10 Băng Tần DSP)              |
| - 10 bộ lọc Peaking EQ chuẩn ISO (32Hz đến 16kHz)                     |
| - Cách ly bộ nhớ bộ lọc theo kênh âm thanh (Stereo: 20 bộ)           |
| - Cập nhật hệ số tại chỗ qua SetPeakingEq (Zero Heap Allocation)      |
| - Bộ kẹp biên độ chống méo tràn số (Soft Limiter: [-1.0, 1.0])        |
+-----------------------------------------------------------------------+
                                   |
                                   v
+-----------------------------------------------------------------------+
| SampleAggregator (Cầu Nối Thu Thập Mẫu Tín Hiệu)                     |
| - Vùng đệm xoay vòng tĩnh (Ring Buffer 2048 mẫu, Zero Allocation)     |
| - Phát hiện đủ khung tín hiệu -> Chuyển tiếp tới FftCalculator        |
+-----------------------------------------------------------------------+
                 |                               |
                 v (Tín hiệu đã qua cân âm)      v (Sao chép khung 2048 mẫu)
+--------------------------------+  +-----------------------------------+
| WaveOutEvent (Thiết Bị Phát)   |  | FftCalculator (Xử Lý Phổ FFT)     |
| - Độ trễ cấu hình: 100ms       |  | - Cửa sổ Hanning chống rò phổ     |
| - Bộ đệm: 3 buffers            |  | - FFT radix-2 2048 điểm           |
| - Xuất tín hiệu trực tiếp      |  | - Gom nhóm 16 cột logarit         |
+--------------------------------+  +-----------------------------------+
                                                     |
                                                     v (Điều tiết 30fps / 33ms)
                                    +-----------------------------------+
                                    | Giao Diện Phổ Sóng Realtime       |
                                    | - Visualizer 16 cột (Spotify Green|
                                    |   hoặc Rainbow Mode)              |
                                    +-----------------------------------+
```

**Nguyên lý thiết kế chuỗi âm thanh:**
1. **Thứ tự mắt xích**: `DspEqualizerSampleProvider` nằm trước `SampleAggregator` để bảo đảm mọi can thiệp tăng/giảm dải tần đều phản ánh tức thời lên phổ sóng FFT 16 cột của giao diện người dùng.
2. **Cách ly bộ nhớ đa kênh (Stereo Channel Isolation)**: Thuật toán lọc nhị thức (BiQuad) phụ thuộc vào trạng thái trễ lịch sử ($x_1, x_2, y_1, y_2$). Đối với luồng âm thanh Stereo, hai kênh Trái và Phải đan xen liên tục ($L, R, L, R...$). Hệ thống duy trì ma trận độc lập `BiQuadFilter[channels, bands]`, triệt tiêu hoàn toàn hiện tượng méo pha hoặc biến dạng tín hiệu.
3. **Cập nhật tham số không cấp phát (Zero Allocation)**: Lệnh thay đổi độ lợi dải tần thực thi phương thức `filter.SetPeakingEq` trên thể hiện bộ lọc hiện hữu, không sinh thêm đối tượng trên bộ nhớ Heap trong luồng âm thanh ưu tiên cao.

---

### 1.3. Mô Hình Đa Luồng & Quản Trị Bộ Nhớ (Concurrency & Memory Model)

| Luồng (Thread) | Nhiệm Vụ & Phạm Vi Hoạt Động | Cơ Chế Đồng Bộ & Kiểm Soát Tài Nguyên |
|---|---|---|
| **WPF UI Thread (Dispatcher)** | Dựng Visual Tree, phản hồi thao tác click, kéo thả, điều khiển animation quay đĩa than và cập nhật độ cao cột phổ. | Nhận dữ liệu phổ qua sự kiện có điều tiết (Throttling 33ms). Sử dụng `DispatcherPriority.Render` và `DispatcherPriority.Background`. |
| **Audio Playback Thread** | Luồng ưu tiên cao của NAudio, liên tục gọi phương thức `Read(buffer, offset, count)` cấp dữ liệu cho phần cứng. | Khóa `lock (_lock)` phạm vi hẹp micro-giây khi chuyển đổi bài hoặc cập nhật dải tần EQ. Tuyệt đối cấm cấp phát Heap trong luồng này. |
| **BFF Web API Thread Pool** | Tiếp nhận yêu cầu HTTP từ client nội bộ, proxy dữ liệu stream từ nguồn ngoài. | Xử lý bất đồng bộ hoàn toàn (`async/await`) với `Stream.CopyToAsync()`, giải phóng luồng khi chờ I/O mạng. |
| **Database I/O Thread** | Xử lý truy vấn đọc/ghi SQLite, cập nhật tương tác và lưu trữ danh sách phát. | Thực thi trên `Task.Run()` qua cấu hình `ConfigureAwait(false)`, bảo vệ tính toàn vẹn giao dịch với SQLite WAL mode. |
| **Background Scanner Task** | Quét đĩa cứng đọc tập tin và trích xuất thẻ ID3 của thư viện cục bộ. | Chạy trên `Task.Run()`, báo cáo tiến độ qua `IProgress<ScanProgress>`, hỗ trợ hủy an toàn bằng `CancellationToken`. |

---

## 2. Các Phân Hệ & Tính Năng Kỹ Thuật

### 2.1. Giao Diện Người Dùng & Điều Hướng Động (Dynamic UI & Navigation)
- Thanh điều hướng Sidebar cố định (220px) phân chia 7 phân khu chức năng: Khám Phá (Explore), Bài Hát Việt (Vietnamese Tracks), Yêu Thích (Favorites), Playlist Cá Nhân, Thư Viện Cục Bộ (Local Library), Hàng Đợi Phát (Play Queue), Bộ Cân Bằng Âm Thanh (DSP Equalizer).
- Khung hiển thị nội dung động sử dụng `ContentControl` kết hợp hệ thống `DataTemplate` ánh xạ trực tiếp từ thuộc tính `CurrentViewName` của `MainViewModel`.
- Hỗ trợ chuyển đổi toàn diện giữa Giao diện Tối (Dark Theme) và Giao diện Sáng (Light Theme).
- Đèn LED trạng thái kết nối thời gian thực giám sát máy chủ nội bộ OWIN BFF.

### 2.2. Trạm Dịch Vụ Cục Bộ OWIN BFF (Local Backend-for-Frontend)
- Máy chủ HTTP tự lưu trữ (Self-host) nội tiến trình lắng nghe tại `http://localhost:5245`.
- Proxy truyền dòng âm thanh hỗ trợ đầy đủ tiêu chuẩn HTTP Range Requests (`206 Partial Content`), cho phép tua tức thời mà không cần nạp toàn bộ tệp vào RAM.
- Bộ định tuyến `MusicSourceRouter` tích hợp nguồn Jamendo quốc tế và kho nhạc Việt Nam chọn lọc.
- Giải thuật loại bỏ dấu tiếng Việt giúp tìm kiếm nhanh, không phân biệt hoa thường và không phụ thuộc kết nối Internet.

### 2.3. Cơ Sở Dữ Liệu SQLite & Tầng Lưu Trữ Bền Vững (Persistence Layer)
- Động cơ SQLite vận hành ở chế độ WAL (Write-Ahead Logging) với cấu hình `Synchronous = NORMAL`, `Foreign Keys = True`, `Cache Size = -64000` (64MB RAM cache).
- Quản trị toàn diện thông qua mô hình Repository Pattern:
  - `TrackRepository`: Lưu trữ, cập nhật metadata, lượt phát, số lần bỏ qua, điểm affinity, trạng thái favorite và tìm kiếm full-text.
  - `PlaylistRepository`: Tạo, sửa, xóa danh sách phát cá nhân, thêm/bớt bài hát với thứ tự sắp xếp được bảo toàn.
  - `QueueRepository`: Lưu trữ và khôi phục hàng đợi bài hát khi người dùng mở lại ứng dụng.
  - `InteractionRepository`: Nhật ký ghi nhận lịch sử tương tác phục vụ thuật toán học máy cục bộ.
  - `StreamCacheRepository`: Quản lý siêu dữ liệu tệp âm thanh đã tải về lưu trong thư mục Cache của ứng dụng.

### 2.4. Động Cơ Gợi Ý Cục Bộ & Smart Shuffle (Boltzmann Sampling)
- Hệ thống tự động ghi nhận điểm quan hệ (Affinity Score) cho từng bài hát dựa trên thói quen nghe nhạc:
  - Nghe trọn vẹn bài hát (`play_complete`): +5.0 điểm.
  - Nghe trên 30 giây (`play_30s`): +2.0 điểm.
  - Đánh dấu yêu thích (`favorite`): +10.0 điểm.
  - Bỏ yêu thích (`unfavorite`): -10.0 điểm.
  - Thêm vào danh sách phát (`add_playlist`): +8.0 điểm.
  - Nhấn chuột chọn bài (`click`): +1.5 điểm.
  - Bỏ qua bài hát (`skip`): -4.0 điểm.
- Thuật toán Smart Shuffle tính toán xác suất lựa chọn bài hát tiếp theo dựa trên phân phối Boltzmann/Softmax, loại bỏ sự lặp lại đơn điệu của hàm ngẫu nhiên thuần túy (`random()`).

### 2.5. Quét Thư Viện Cục Bộ & Bộ Nhớ Đệm Âm Thanh Ngoại Tuyến
- Dịch vụ `LocalLibraryService` duyệt thư mục máy tính an toàn theo giải thuật hàng đợi BFS, xử lý triệt để các ngoại lệ đường dẫn dài và quyền truy cập.
- Trích xuất thông tin siêu dữ liệu ID3 (Tiêu đề, Nghệ sĩ, Album, Thể loại, Thời lượng) và ảnh bìa qua `TagLibSharp`.
- Dịch vụ `LocalAudioCacheService` tự động lưu trữ các bài hát trực tuyến đã phát vào thư mục `%LocalAppData%\MusicApp\Cache`, sử dụng mã băm SHA-1 của URL làm khóa định danh.

### 2.6. Hàng Đợi Phát Nhạc & Tương Tác Kéo Thả (Interactive Play Queue)
- Hỗ trợ thao tác kéo thả trực quan để sắp xếp lại thứ tự ưu tiên các bài hát trong hàng đợi thông qua thư viện `gong-wpf-dragdrop`.
- Cung cấp các thao tác quản trị hàng đợi: đảo vị trí bài hát, xóa bài đơn lẻ, xóa toàn bộ hàng đợi, phát ngay lập tức từ hàng đợi.
- Cơ chế tự động phát tiếp (Auto-Advance): Tự động lấy bài tiếp theo từ hàng đợi khi bài hát hiện tại kết thúc; ưu tiên hàng đợi thủ công trước khi chuyển sang danh sách mặc định hoặc gợi ý Smart Shuffle.

### 2.7. Lời Bài Hát Đồng Bộ Thời Gian Thực (.LRC)
- Bộ phân tích cú pháp `.LRC` hiệu năng cao bằng biểu thức chính quy Regex, hỗ trợ định dạng chuẩn `[mm:ss.xx]`, nhiều mốc thời gian trên một dòng và thẻ bù trừ độ trễ `[offset:+/-ms]`.
- Giải thuật tìm kiếm nhị phân $O(\log N)$ định vị dòng lời theo mốc thời gian phát hiện tại.
- Cơ chế lọc sự kiện ngưỡng (Event Gating): Chỉ kích hoạt thông báo cập nhật khi chỉ số dòng lời thực sự thay đổi, bảo vệ luồng giao diện WPF Dispatcher khỏi tình trạng quá tải thông điệp.
- Giao diện hiển thị lời phong cách karaoke tối màu, tự động cuộn mượt căn giữa màn hình và hỗ trợ bấm vào dòng lời để tua nhạc trực tiếp.

### 2.8. Bộ Cân Bằng Âm Sắc Đồ Họa 10 Băng Tần DSP (10-Band Graphic DSP Equalizer)
- Chuỗi xử lý `ISampleProvider` triển khai 10 bộ lọc Peaking EQ theo dải tần quãng tám tiêu chuẩn ISO: 32Hz, 64Hz, 125Hz, 250Hz, 500Hz, 1kHz, 2kHz, 4kHz, 8kHz, 16kHz.
- Cách ly hoàn toàn bộ nhớ trạng thái bộ lọc giữa 2 kênh Stereo ($2 \times 10 = 20$ bộ lọc BiQuadFilter).
- Bộ kẹp biên độ mềm (Soft Limiter) giữ tín hiệu trong ngưỡng $[-1.0f, +1.0f]$, chống hiện tượng vỡ tiếng kỹ thuật số.
- Thiết lập sẵn các cấu hình âm sắc chuẩn mực: Phẳng (Flat), Rock, Pop, Jazz, Cổ Điển (Classical), Tăng Trầm (Bass Boost), Tăng Giọng Hát (Vocal Boost), tự động nhận diện chế độ Tùy Chỉnh (Custom).

### 2.9. Phân Tích Phổ Tần Số FFT & Đĩa Than Quay
- Phân tích phổ thời gian thực 16 cột theo thang logarit tham chiếu từ chuẩn hiển thị âm thanh.
- Bộ điều tiết tần suất gửi sự kiện giới hạn ở mức 30 khung hình/giây (33ms) nhằm bảo đảm độ ổn định 60fps của giao diện người dùng.
- Hoạt cảnh xoay đĩa than vinyl vật lý khi nhạc đang phát, tự động dừng lại khi tạm dừng.
- Chế độ hiển thị màu sắc visualizer đa dạng: Màu xanh thương hiệu (Spotify Green) hoặc chế độ quang phổ biến sắc (Rainbow Mode).

---

## 3. Danh Mục Công Nghệ & Thư Viện

| Phân Hệ / Thành Phần | Công Nghệ Áp Dụng | Phiên Bản | Vai Trò & Mục Đích Kỹ Thuật |
|---|---|---|---|
| Nền tảng thực thi | .NET Framework | 4.6.1 | Đảm bảo khả năng tương thích hệ điều hành Windows gốc |
| Ngôn ngữ lập trình | C# | 7.3 | Cú pháp xác định, kiểm soát kiểu dữ liệu tĩnh nghiêm ngặt |
| Giao diện người dùng | WPF / XAML | 4.6.1 | Dựng giao diện tăng tốc phần cứng DirectX, ràng buộc dữ liệu MVVM |
| Xử lý âm thanh | NAudio | 1.10.0 | Quản lý thiết bị WaveOut, luồng giải mã, chuỗi bộ lọc DSP |
| Cơ sở dữ liệu cục bộ | System.Data.SQLite.Core | 1.0.118.0 | Lưu trữ dữ liệu quan hệ cục bộ, chế độ WAL hiệu năng cao |
| Trích xuất thẻ ID3 | TagLibSharp | 2.2.0 | Đọc thông tin siêu dữ liệu âm nhạc và ảnh bìa tệp đĩa |
| Kéo thả giao diện | gong-wpf-dragdrop | 2.3.2 | Hỗ trợ kéo thả sắp xếp danh sách hàng đợi trong WPF |
| Xử lý định dạng JSON | Newtonsoft.Json | 13.0.3 | Tuần tự hóa và giải tuần tự hóa cấu hình, DTO dữ liệu mạng |
| Máy chủ nội bộ | Microsoft.Owin.SelfHost | 4.2.2 | Khởi tạo máy chủ HTTP In-Process không phụ thuộc dịch vụ ngoài |
| Giao diện API | Microsoft.AspNet.WebApi.OwinSelfHost | 5.2.9 | Xây dựng Web API Controller phục vụ tìm kiếm và streaming |
| Kiểm thử tự động | MSTest v2 | 15.9.1 | Bộ khung thực thi kiểm thử đơn vị và kiểm thử tích hợp |

---

## 4. Cấu Trúc Mã Nguồn Dự Án

```
MusicApp/
├── MusicApp.sln                               # Tệp giải pháp Master Visual Studio
├── README.md                                  # Tài liệu kiến trúc và hướng dẫn kỹ thuật
├── .gitignore                                 # Quy tắc loại trừ tệp nhị phân cho .NET & VS
├── MusicApp/                                  # Ứng dụng Desktop WPF (Presentation Layer)
│   ├── App.xaml / App.xaml.cs                 # Điểm khởi đầu ứng dụng, cấu hình Theme, dọn dẹp tài nguyên
│   ├── MainWindow.xaml / MainWindow.xaml.cs   # Khung giao diện chính (bố cục 2 cột)
│   ├── Converters/                            # Bộ chuyển đổi dữ liệu XAML (FrozenImage, Visibility, v.v.)
│   ├── Resources/Themes/                      # Bảng màu DarkTheme.xaml và LightTheme.xaml
│   ├── ViewModels/                            # Tầng Presentation Logic (MVVM)
│   │   ├── MainViewModel.cs                   # Điều phối danh mục, điều hướng chính, lệnh tìm kiếm
│   │   ├── NowPlayingViewModel.cs             # Điều khiển phát, dòng thời gian, âm lượng, dữ liệu FFT, persistence
│   │   ├── LocalLibraryViewModel.cs           # Quản lý quét thư mục máy tính và áp dụng bộ lọc
│   │   ├── PlayQueueViewModel.cs              # Quản lý hàng đợi và logic kéo thả
│   │   ├── PlaylistsViewModel.cs              # Quản lý danh sách các playlist cá nhân
│   │   ├── PlaylistDetailViewModel.cs         # Quản lý chi tiết bài hát trong từng playlist
│   │   ├── FavoritesViewModel.cs              # Quản lý danh sách các bài hát yêu thích
│   │   ├── LyricsViewModel.cs                 # Quản lý trạng thái và tính toán dòng lời hiển thị
│   │   ├── DspEqualizerViewModel.cs           # Quản lý 10 băng tần DSP và cấu hình âm sắc mẫu
│   │   ├── EqualizerBandViewModel.cs          # Mô hình dữ liệu cho từng thanh trượt dải tần
│   │   ├── EqualizerBarViewModel.cs           # Mô hình dữ liệu cho từng cột sóng visualizer
│   │   ├── NavigationItemViewModel.cs         # Mô hình dữ liệu cho mục điều hướng Sidebar
│   │   └── TrackItemViewModel.cs              # ViewModel đại diện cho bài hát trên giao diện
│   └── Views/                                 # Các UserControl giao diện thành phần
│       ├── SidebarNavigationView.xaml         # Thanh điều hướng cố định bên trái (220px)
│       ├── NowPlayingCardView.xaml            # Thanh điều khiển phát nhạc cố định bên dưới
│       ├── LocalLibraryScannerView.xaml       # Giao diện quét và duyệt thư viện nhạc cục bộ
│       ├── PlayQueueView.xaml                 # Giao diện hàng đợi bài hát Up Next
│       ├── PlaylistsView.xaml                 # Giao diện quản lý danh sách playlist
│       ├── PlaylistDetailView.xaml            # Giao diện chi tiết nội dung playlist
│       ├── FavoritesView.xaml                 # Giao diện danh sách bài hát yêu thích
│       ├── CreatePlaylistDialog.xaml          # Hộp thoại tạo mới danh sách phát
│       ├── LyricsSyncView.xaml                # Giao diện hiển thị lời bài hát cuộn tự động
│       └── DspEqualizerView.xaml              # Bảng điều khiển bộ cân bằng âm thanh 10 cần gạt
├── src/
│   ├── MusicApp.Core/                         # Tầng lõi nghiệp vụ độc lập nền tảng
│   │   ├── Common/                            # Lớp cơ sở ObservableObject, RelayCommand, TrackIdentityHelper
│   │   ├── Dtos/                              # Đối tượng truyền dữ liệu SearchResponseDto, TrackDto
│   │   ├── Interfaces/                        # Hợp đồng IAudioService, IDspEqualizerService, ILyricsService...
│   │   │   └── Persistence/                   # Hợp đồng ITrackRepository, IPlaylistRepository, IQueueRepository...
│   │   ├── Models/                            # Thực thể TrackEntity, PlaylistEntity, LyricLine, PlaybackState...
│   │   ├── Persistence/                       # Khởi tạo SQLite và triển khai các Repository
│   │   │   ├── DatabaseInitializer.cs         # Thiết lập schema bảng, index và migration SQLite
│   │   │   └── Repositories/                  # TrackRepository, PlaylistRepository, QueueRepository...
│   │   └── Services/                          # LrcParser, LocalLibraryService, LyricsService, RecommendationEngine...
│   ├── MusicApp.AudioEngine/                  # Tầng xử lý tín hiệu âm thanh và thiết bị xuất
│   │   ├── Dsp/                               # BiQuadFilter, DspEqualizerSampleProvider, FftCalculator...
│   │   ├── Stream/                            # BufferedHttpWaveStream hỗ trợ đọc phân đoạn mạng
│   │   └── NAudioService.cs                   # Triển khai IAudioService, quản lý thiết bị WaveOutEvent
│   └── MusicApp.Bff/                          # Tầng dịch vụ trung gian cục bộ OWIN
│       ├── Controllers/                       # TrackController cung cấp API tìm kiếm và stream
│       ├── Providers/                         # Router định tuyến, nguồn Jamendo và nhạc Việt Nam
│       ├── Startup.cs                         # Cấu hình định tuyến Web API trên nền OWIN
│       └── BffServerHost.cs                   # Quản lý vòng đời khởi chạy máy chủ nội bộ
└── tests/
    └── MusicApp.Tests/                        # Bộ kiểm thử tự động (75 test cases)
        ├── BffEndpointTests.cs                # Kiểm thử API tìm kiếm và truyền dữ liệu mạng Range
        ├── DatabaseInitializerTests.cs       # Kiểm thử khởi tạo bảng và tính lũy đẳng của SQLite
        ├── DspEqualizerTests.cs               # Kiểm thử hệ số DSP, độ tăng giảm âm và cổng Gate 5
        ├── FftCalculatorTests.cs              # Kiểm thử tính toán biến đổi phổ FFT 16 cột
        ├── LocalAudioCacheServiceTests.cs     # Kiểm thử băm SHA-1 và lưu trữ cache luồng âm thanh
        ├── LocalLibraryTests.cs               # Kiểm thử đọc thẻ ID3 và thuật toán duyệt BFS thư mục
        ├── LyricsTests.cs                     # Kiểm thử đọc tệp LRC và cổng Gate 4 chuyển dòng lời
        ├── PlaylistRepositoryTests.cs         # Kiểm thử tạo, thêm và truy vấn bài hát trong playlist
        ├── PlayQueueTests.cs                  # Kiểm thử sắp xếp hàng đợi và cổng Gate 3 phát tiếp
        ├── QueueRepositoryTests.cs            # Kiểm thử lưu trữ và khôi phục hàng đợi từ SQLite
        ├── RecommendationEngineTests.cs       # Kiểm thử điểm affinity và thuật toán Smart Shuffle
        ├── RelayCommandTests.cs               # Kiểm thử thực thi lệnh MVVM
        ├── TrackRepositoryTests.cs            # Kiểm thử CRUD bài hát, toggle favorite, tính toán affinity
        └── ViewModelTests.cs                  # Kiểm thử điều hướng, lọc danh mục và đổi theme
```

---

## 5. Hướng Dẫn Biên Dịch & Kiểm Thử

### 5.1. Yêu Cầu Môi Trường
1. Hệ điều hành: Windows 10 hoặc Windows 11 (64-bit).
2. Môi trường phát triển: Visual Studio 2017, 2019 hoặc 2022.
3. Nền tảng: .NET Framework 4.6.1 Developer Pack.
4. Công cụ dòng lệnh: MSBuild 15.0 trở lên.

### 5.2. Biên Dịch Dự Án (Build / Rebuild)
Mở PowerShell tại thư mục gốc của giải pháp và thực thi lệnh biên dịch:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" MusicApp.sln /t:Rebuild /p:Configuration=Debug /v:m
```

Kết quả biên dịch chuẩn xác: `0 Error(s)`, `0 Warning(s)`.

### 5.3. Thực Thi Bộ Kiểm Thử Tự Động (Unit Tests)
Thực thi toàn bộ 75 bài kiểm thử đơn vị bao phủ toàn diện các phân hệ:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" tests\MusicApp.Tests\bin\Debug\MusicApp.Tests.dll
```

Kết quả thực thi đạt chuẩn:
```text
Total tests: 75. Passed: 75. Failed: 0. Skipped: 0.
Test Run Successful.
Test execution time: ~5.2 Seconds
```

### 5.4. Khởi Chạy Ứng Dụng
Khởi chạy tệp thực thi đã biên dịch:

```powershell
.\MusicApp\bin\Debug\MusicApp.exe
```

Khi khởi động, ứng dụng tự động thực hiện chuỗi khởi tạo:
1. Mở máy chủ OWIN BFF tại địa chỉ `http://localhost:5245`.
2. Khởi tạo cơ sở dữ liệu SQLite tại `%LocalAppData%\MusicApp\musicapp.db`.
3. Kết nối thiết bị âm thanh phần cứng thông qua NAudio.
4. Nạp danh mục âm nhạc, đồng bộ hàng đợi gần nhất và dựng giao diện người dùng.

---

## 6. Tiêu Chuẩn Kỹ Thuật & Quản Trị Rủi Ro

- **Tuân thủ mô hình MVVM thuần túy**: Toàn bộ tệp code-behind (`.xaml.cs`) chỉ chứa các thao tác trực tiếp với cây giao diện Visual Tree (như tính toán cuộn `ScrollViewer` hoặc hiển thị dialog). Không chứa logic nghiệp vụ hoặc xử lý dữ liệu tại tầng này.
- **Không cấp phát bộ nhớ trong luồng âm thanh (Zero Heap Allocation)**: Luồng xử lý âm thanh sử dụng vùng đệm vòng lặp tĩnh. Việc tính toán bộ lọc BiQuad và biến đổi phổ FFT không tạo ra đối tượng mới trên bộ nhớ Heap, triệt tiêu hoàn toàn hiện tượng khựng tiếng do bộ thu gom rác (Garbage Collector) gây ra.
- **An toàn bộ nhớ hình ảnh (Memory Safety)**: Mọi dữ liệu hình ảnh chuyển đổi sang `BitmapImage` đều được đóng băng bằng `Freeze()` để đảm bảo an toàn truy cập đa luồng và giải phóng ngay bộ đệm thô.
- **Tính toàn vẹn dữ liệu quan hệ (ACID Compliance)**: Cơ sở dữ liệu SQLite được bảo vệ bằng chế độ WAL (Write-Ahead Logging) và các giao dịch nguyên tử (`SQLiteTransaction`), ngăn chặn triệt để tình trạng khóa cơ sở dữ liệu (Database Locked) khi ghi song song từ nhiều luồng.
- **Tương thích ngược**: Toàn bộ mã nguồn tuân thủ nghiêm ngặt chuẩn nền tảng .NET Framework 4.6.1 và ngôn ngữ C# 7.3.
