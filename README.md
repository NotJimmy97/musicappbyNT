# MusicApp Desktop (musicappbyNT)

Ứng dụng phát nhạc Desktop Native được phát triển trên nền tảng Windows Presentation Foundation (WPF) với .NET Framework 4.6.1. Dự án vận hành dựa trên kiến trúc phân lớp tách biệt, bao gồm dịch vụ trung gian cục bộ OWIN Self-Host Backend-for-Frontend (BFF), engine giải mã và xử lý âm thanh độ trễ thấp NAudio, cùng hệ thống bộ lọc cân bằng âm sắc đồ họa 10 băng tần DSP (10-Band Graphic DSP Equalizer).

---

## 1. Tổng Quan Kiến Trúc Hệ Thống

Hệ thống tuân thủ mô hình Modular Monolith kết hợp cùng kiến trúc MVVM nghiêm ngặt và cơ chế giao tiếp In-Process BFF, đảm bảo tính đóng gói, khả năng kiểm thử độc lập và loại bỏ hiện tượng khóa luồng giao diện người dùng (UI Thread):

```
[ Lớp Trình Diễn WPF Desktop (MVVM Pattern) ]
                       |
                       v
   +---------------------------+---------------------------+
   |                           |                           |
   v                           v                           v
[ Dịch Vụ Cục Bộ OWIN BFF ]    [ Engine Âm Thanh NAudio DSP ] [ Tầng Lõi Nghiệp Vụ Core ]
- Self-host HTTP (cổng 5245)  - Bộ lọc BiQuad Peaking EQ   - Bộ phân tích LrcParser
- API Tìm kiếm bài hát         - Cầu nối SampleAggregator    - Quét thư viện LocalLibrary
- Proxy Stream phân đoạn       - Phân tích phổ FFT 16 cột    - Quản lý hàng đợi PlayQueue
- Định tuyến nguồn nhạc        - Giải mã offline AudioFile   - Hợp đồng DTO & Domain Model
                               - Stream trực tuyến MediaFnd
```

---

## 2. Các Tính Năng Kỹ Thuật Nổi Bật

### 2.1. Cấu Trúc Khung Nhìn 2 Cột & Hệ Thống Điều Hướng Động
- Thanh điều hướng Sidebar cố định (chiều rộng 220px) thiết kế theo phong cách Spotify, phân chia danh mục logic: Khám Phá, Nhạc Việt Nam, Thư Viện Cá Nhân, Hàng Đợi, Lời Bài Hát, Bộ Chỉnh Âm (EQ).
- Khung hiển thị nội dung động sử dụng `ContentControl` kết hợp hệ thống `DataTemplate` ánh xạ trực tiếp từ trạng thái `CurrentViewName` của ViewModel.
- Đèn báo trạng thái kết nối thời gian thực của dịch vụ BFF nội bộ.

### 2.2. Trạm Trung Gian Cục Bộ OWIN BFF & Truyền Dòng Âm Thanh
- Máy chủ HTTP tự lưu trữ (Self-host) lắng nghe tại địa chỉ `http://localhost:5245`.
- Proxy truyền dòng âm thanh hỗ trợ đầy đủ tiêu chuẩn HTTP Range Requests (`206 Partial Content`), phục vụ tua nhanh và nhận dữ liệu từng khối nhị phân mà không cần tải toàn bộ tệp vào bộ nhớ.
- Bộ định tuyến `MusicSourceRouter` tích hợp nguồn nhạc quốc tế và kho nhạc Việt Nam tuyển chọn.
- Thuật toán loại bỏ dấu tiếng Việt giúp tìm kiếm nhanh, không phân biệt hoa thường và không độ trễ mạng.

### 2.3. Quét Thư Viện Nhạc Cục Bộ & Phát Nhạc Offline
- Dịch vụ `LocalLibraryService` duyệt thư mục an toàn theo giải thuật hàng đợi (Breadth-First Search), xử lý triệt để các ngoại lệ phân quyền và đường dẫn dài (`UnauthorizedAccessException`, `PathTooLongException`, `SecurityException`).
- Đọc và giải mã siêu dữ liệu ID3 (Tiêu đề, Nghệ sĩ, Album, Thể loại, Thời lượng) và ảnh bìa đính kèm thông qua thư viện `TagLibSharp 2.2.0`.
- Chống rò rỉ bộ nhớ (Memory Leak) bằng phương thức `BitmapImage.Freeze()` ngay khi chuyển đổi dữ liệu hình ảnh.
- Tích hợp luồng giải mã âm thanh offline trực tiếp qua `NAudio.Wave.AudioFileReader`.

### 2.4. Hàng Đợi Phát Nhạc & Kéo Thả Sắp Xếp Tương Tác
- Hỗ trợ kéo thả trực quan sắp xếp thứ tự ưu tiên phát nhạc bằng thư viện `gong-wpf-dragdrop 2.3.2`.
- Danh sách "Phát Tiếp Theo" hỗ trợ đảo vị trí, xóa bài hát đơn lẻ, xóa toàn bộ hàng đợi và phát ngay lập tức.
- Cơ chế tự động chuyển bài (Auto-Advance): `NowPlayingViewModel` tự động lấy bài hát đầu hàng đợi khi bài hát hiện tại phát hết thời lượng.
- Nút thêm nhanh vào hàng đợi (`+`) xuất hiện đồng nhất trên mọi danh sách bài hát.

### 2.5. Lời Bài Hát Đồng Bộ Thời Gian Thực (.LRC)
- Bộ phân tích cú pháp `.LRC` hiệu năng cao bằng biểu thức chính quy (Regex), hỗ trợ định dạng chuẩn `[mm:ss.xx]`, nhiều mốc thời gian trên một dòng, thẻ bù trừ độ trễ `[offset:+/-ms]` và loại bỏ thẻ siêu dữ liệu rác.
- Thuật toán tìm kiếm nhị phân $O(\log N)$ xác định dòng lời bài hát tương ứng với mốc thời gian hiện tại.
- Cơ chế lọc sự kiện ngưỡng (Event Gating): chỉ phát tín hiệu cập nhật khi chỉ số dòng lời thực sự thay đổi, ngăn chặn hiện tượng nghẽn luồng WPF Dispatcher khi mốc thời gian phát thay đổi liên tục.
- Giao diện hiển thị lời phong cách karaoke tối màu, làm nổi bật dòng đang hát bằng cỡ chữ phóng to và màu xanh thương hiệu, tự động cuộn mượt căn giữa màn hình và hỗ trợ bấm vào dòng lời để tua nhạc trực tiếp.
- Cơ chế phân giải kép: ưu tiên tệp `.lrc` cùng thư mục trên ổ đĩa (`{trackPath}.lrc`), tự động chuyển sang dữ liệu lời nhúng sẵn đối với các tác phẩm tuyển chọn.

### 2.6. Bộ Cân Bằng Âm Sắc Đồ Họa 10 Băng Tần DSP (10-Band Graphic DSP Equalizer)
- Chuỗi xử lý tín hiệu `ISampleProvider` triển khai 10 bộ lọc Peaking EQ theo chuẩn tần số trung tâm quãng tám ISO:
  - 32 Hz (Âm siêu trầm / Sub-Bass)
  - 64 Hz (Âm trầm / Bass)
  - 125 Hz (Âm trầm cao / Low-Mid)
  - 250 Hz (Âm trung thấp / Low Midrange)
  - 500 Hz (Âm trung / Midrange)
  - 1000 Hz (Âm trung cao / High-Mid)
  - 2000 Hz (Hiện diện / Presence)
  - 4000 Hz (Hiện diện cao / Presence)
  - 8000 Hz (Âm sáng / Brilliance)
  - 16000 Hz (Âm khí / Air)
- Cách ly hoàn toàn trạng thái bộ nhớ bộ lọc giữa các kênh âm thanh ($2 \times 10 = 20$ bộ lọc cho luồng Stereo) bằng thuật toán `NAudio.Dsp.BiQuadFilter` với hệ số phẩm chất $Q = 1.4142$ (băng thông 1 octave).
- Cập nhật tham số hệ số bộ lọc tại chỗ qua phương thức `filter.SetPeakingEq`: đảm bảo không phát sinh bất kỳ cấp phát bộ nhớ Heap nào trong suốt quá trình phát nhạc.
- Bộ giới hạn mềm (Soft Limiter) kẹp biên độ mẫu tín hiệu trong ngưỡng nghiêm ngặt $[-1.0f, +1.0f]$, triệt tiêu hiện tượng méo tràn số nhị phân (Digital Wrap-around Distortion) khi tăng âm lượng quá mức.
- Thứ tự mắt xích âm thanh: `Nguồn giải mã -> DspEqualizerSampleProvider -> SampleAggregator -> WaveOutEvent`. Tín hiệu sau khi chỉnh âm đi thẳng vào bộ tổng hợp FFT, giúp phổ quang phổ 16 cột hiển thị ngay lập tức sự thay đổi năng lượng tần số theo thời gian thực.
- Cấu hình âm sắc định sẵn (Presets): Phẳng (Flat), Rock, Pop, Jazz, Cổ Điển (Classical), Tăng Trầm (Bass Boost), Tăng Giọng Hát (Vocal Boost), tự động nhận diện chế độ Tùy Chỉnh (Custom).

### 2.7. Trình Phân Tích Phổ Tần Số FFT & Đĩa Than Quay
- Phân tích phổ thời gian thực 16 cột theo thang logarit tham chiếu từ tài liệu thiết kế Spotify-Readme.
- Bộ điều tiết tần suất gửi sự kiện giới hạn ở mức 30 khung hình/giây (33ms) nhằm duy trì độ mượt 60 khung hình/giây của giao diện WPF.
- Hoạt cảnh xoay đĩa than liên tục khi bài hát đang ở trạng thái phát.
- Hỗ trợ chuyển đổi toàn diện giữa Giao diện Tối (Dark Theme) và Giao diện Sáng (Light Theme).

---

## 3. Danh Mục Công Nghệ & Thư Viện Sử Dụng

| Phân Hệ / Thành Phần | Công Nghệ Áp Dụng | Phiên Bản | Mục Đích Sử Dụng |
|---|---|---|---|
| Nền tảng thực thi | .NET Framework | 4.6.1 | Đảm bảo tương thích hệ điều hành Windows gốc |
| Ngôn ngữ lập trình | C# | 7.3 | Cú pháp xác định, kiểm soát kiểu dữ liệu tĩnh |
| Giao diện người dùng | WPF / XAML | 4.6.1 | Dựng giao diện tăng tốc phần cứng DirectX |
| Xử lý âm thanh | NAudio | 1.10.0 | Quản lý thiết bị xuất, định tuyến đồ thị âm thanh, bộ lọc DSP |
| Trích xuất thẻ ID3 | TagLibSharp | 2.2.0 | Đọc thông tin bài hát và trích xuất ảnh bìa tệp cục bộ |
| Tương tác kéo thả | gong-wpf-dragdrop | 2.3.2 | Kéo thả sắp xếp danh sách hàng đợi trong WPF |
| Máy chủ nội bộ | Microsoft.Owin.SelfHost | 4.2.2 | Khởi tạo máy chủ HTTP In-Process không cần IIS |
| Giao diện API | Microsoft.AspNet.WebApi.OwinSelfHost | 5.2.9 | Xây dựng Controller phục vụ tìm kiếm và stream nhạc |
| Kiểm thử tự động | MSTest v2 | 15.9.1 | Thực thi bộ kiểm thử đơn vị và cổng kiểm chứng kiến trúc |

---

## 4. Cấu Trúc Mã Nguồn Dự Án

```
MusicApp/
├── MusicApp.sln                               # Tệp giải pháp Master Visual Studio
├── README.md                                  # Tài liệu kiến trúc và hướng dẫn sử dụng
├── .gitignore                                 # Quy tắc loại trừ tệp nhị phân cho .NET & VS
├── MusicApp/                                  # Ứng dụng Desktop WPF (Tầng Trình Diễn)
│   ├── App.xaml / App.xaml.cs                 # Khởi động ứng dụng, nạp theme, giải phóng tài nguyên
│   ├── MainWindow.xaml / MainWindow.xaml.cs   # Khung giao diện chính (bố cục 2 cột)
│   ├── Converters/                            # Bộ chuyển đổi dữ liệu (FrozenImage, Visibility, v.v.)
│   ├── Resources/Themes/                      # Bảng màu DarkTheme.xaml và LightTheme.xaml
│   ├── ViewModels/                            # Tầng Presentation Logic (MVVM)
│   │   ├── MainViewModel.cs                   # Điều phối danh mục, điều hướng chính, lệnh tìm kiếm
│   │   ├── NowPlayingViewModel.cs             # Điều khiển phát, dòng thời gian, âm lượng, dữ liệu FFT
│   │   ├── LocalLibraryViewModel.cs           # Quản lý quét thư mục máy tính và lọc danh sách
│   │   ├── PlayQueueViewModel.cs              # Quản lý hàng đợi và logic kéo thả
│   │   ├── LyricsViewModel.cs                 # Quản lý trạng thái và tính toán dòng lời hiển thị
│   │   ├── DspEqualizerViewModel.cs           # Quản lý 10 băng tần DSP và cấu hình âm sắc mẫu
│   │   └── EqualizerBandViewModel.cs          # Mô hình dữ liệu cho từng thanh trượt tần số
│   └── Views/                                 # Các UserControl giao diện thành phần
│       ├── SidebarNavigationView.xaml         # Thanh điều hướng cố định bên trái (220px)
│       ├── NowPlayingCardView.xaml            # Thanh điều khiển phát cố định bên dưới
│       ├── LocalLibraryScannerView.xaml       # Giao diện quét và duyệt thư viện nhạc nội bộ
│       ├── PlayQueueView.xaml                 # Giao diện hàng đợi bài hát Up Next
│       ├── LyricsSyncView.xaml                # Giao diện hiển thị lời bài hát cuộn tự động
│       └── DspEqualizerView.xaml              # Bảng điều khiển bộ cân bằng âm thanh 10 cần gạt
├── src/
│   ├── MusicApp.Core/                         # Tầng lõi nghiệp vụ độc lập nền tảng
│   │   ├── Common/                            # Lớp cơ sở ObservableObject, RelayCommand, AsyncRelayCommand
│   │   ├── Dtos/                              # Đối tượng truyền dữ liệu SearchResponseDto, TrackDto
│   │   ├── Interfaces/                        # Hợp đồng IAudioService, IDspEqualizerService, ILyricsService
│   │   ├── Models/                            # Mô hình dữ liệu TrackModel, LyricLine, PlaybackState
│   │   └── Services/                          # Dịch vụ LrcParser, LocalLibraryService, LyricsService
│   ├── MusicApp.AudioEngine/                  # Tầng xử lý tín hiệu âm thanh và thiết bị xuất
│   │   ├── Dsp/                               # Các khối xử lý DSP số hóa
│   │   │   ├── BiQuadFilter                   # Thuật toán bộ lọc Peaking EQ nhị thức
│   │   │   ├── DspEqualizerSampleProvider.cs  # Khối xử lý 10 băng tần đa kênh âm thanh
│   │   │   ├── SampleAggregator.cs            # Khối trích mẫu phục vụ phân tích phổ FFT
│   │   │   ├── FftCalculator.cs               # Thuật toán biến đổi Fourier rời rạc 16 cột
│   │   │   └── SpectrumBin.cs                 # Cấu trúc lưu trữ giá trị cột phổ
│   │   ├── Stream/                            # BufferedHttpWaveStream hỗ trợ đọc phân đoạn mạng
│   │   └── NAudioService.cs                   # Triển khai IAudioService, quản lý thiết bị WaveOutEvent
│   └── MusicApp.Bff/                          # Tầng dịch vụ trung gian cục bộ OWIN
│       ├── Controllers/                       # TrackController cung cấp API tìm kiếm và stream
│       ├── Providers/                         # Router định tuyến, nguồn Jamendo và nguồn nhạc Việt Nam
│       ├── Startup.cs                         # Cấu hình định tuyến Web API trên nền OWIN
│       └── BffServerHost.cs                   # Quản lý vòng đời khởi chạy máy chủ nội bộ
└── tests/
    └── MusicApp.Tests/                        # Toàn bộ kiểm thử tự động (60 test cases)
        ├── BffEndpointTests.cs                # Kiểm thử API tìm kiếm và truyền dữ liệu mạng Range
        ├── DspEqualizerTests.cs               # Kiểm thử hệ số DSP, độ tăng giảm âm và cổng Gate 5
        ├── FftCalculatorTests.cs              # Kiểm thử tính toán phổ FFT
        ├── LocalLibraryTests.cs               # Kiểm thử đọc ID3 và thuật toán duyệt BFS thư mục
        ├── LyricsTests.cs                     # Kiểm thử đọc tệp LRC và cổng Gate 4 chuyển dòng
        ├── PlayQueueTests.cs                  # Kiểm thử sắp xếp hàng đợi và cổng Gate 3 phát tiếp
        ├── RelayCommandTests.cs               # Kiểm thử thực thi lệnh MVVM
        └── ViewModelTests.cs                  # Kiểm thử điều hướng, lọc danh mục và đổi theme
```

---

## 5. Hướng Dẫn Biên Dịch & Khởi Chạy

### 5.1. Yêu Cầu Môi Trường
1. Hệ điều hành Windows 10 hoặc Windows 11.
2. Visual Studio 2017, 2019 hoặc 2022 (bản Community, Professional hoặc Enterprise).
3. Gói phát triển .NET Framework 4.6.1 Developer Pack.
4. Công cụ dòng lệnh MSBuild phiên bản 15.0 trở lên.

### 5.2. Biên Dịch Bằng Dòng Lệnh (MSBuild)
Mở cửa sổ PowerShell hoặc Terminal tại thư mục gốc của dự án:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" MusicApp.sln /t:Build /p:Configuration=Debug /v:m
```

Kết quả biên dịch chuẩn mực: `0 Error(s)`, `0 Warning(s)`.

### 5.3. Thực Thi Bộ Kiểm Thử Tự Động (VSTest)
Chạy toàn bộ 60 bài kiểm thử đơn vị bao phủ toàn diện các tầng BFF, AudioEngine, Core và ViewModel:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" tests\MusicApp.Tests\bin\Debug\MusicApp.Tests.dll
```

Kết quả thực thi chuẩn:
```text
Total tests: 60. Passed: 60. Failed: 0. Skipped: 0.
Test Run Successful.
```

### 5.4. Khởi Chạy Ứng Dụng Desktop
Chạy tệp thực thi đã được biên dịch:

```powershell
.\MusicApp\bin\Debug\MusicApp.exe
```

Khi khởi chạy, ứng dụng tự động mở máy chủ OWIN BFF tại `http://localhost:5245`, kết nối thiết bị âm thanh qua NAudio, khởi tạo danh mục bài hát và hiển thị giao diện người dùng.

---

## 6. Tiêu Chuẩn Kỹ Thuật & Quản Trị Rủi Ro

- **Tuân thủ mô hình MVVM thuần túy**: Toàn bộ tệp code-behind (`.xaml.cs`) chỉ chứa các thao tác trực tiếp với cây giao diện Visual Tree (như tính toán cuộn `ScrollViewer`). Không cho phép chứa bất kỳ logic nghiệp vụ hoặc xử lý dữ liệu nào tại tầng này.
- **Không cấp phát bộ nhớ trong luồng âm thanh (Zero Heap Allocation)**: Luồng xử lý âm thanh sử dụng vùng đệm vòng lặp tĩnh. Việc tính toán bộ lọc BiQuad và biến đổi phổ FFT không tạo ra đối tượng mới trên bộ nhớ Heap, triệt tiêu hoàn toàn hiện tượng khựng tiếng do bộ thu gom rác (Garbage Collector) gây ra.
- **An toàn bộ nhớ hình ảnh (Memory Safety)**: Mọi dữ liệu hình ảnh chuyển đổi sang `BitmapImage` đều được đóng băng bằng `Freeze()` để đảm bảo an toàn truy cập đa luồng và giải phóng ngay bộ đệm thô.
- **Tương thích ngược**: Mã nguồn được kiểm soát chặt chẽ để tương thích hoàn toàn với nền tảng .NET Framework 4.6.1 và trình biên dịch C# 7.3.
