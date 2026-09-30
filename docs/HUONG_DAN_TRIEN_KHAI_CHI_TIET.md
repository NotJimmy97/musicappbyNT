# HƯỚNG DẪN TRIỂN KHAI CHI TIẾT DỰ ÁN MUSIC APP
## NỀN TẢNG: WPF NATIVE (.NET FRAMEWORK 4.6.1) / MVVM / LOCAL BFF OWIN
**Tài liệu tham chiếu:** `TECHNICAL_DESIGN_DOCUMENT.md`  
**Đối tượng thực hiện:** Kỹ sư phần mềm Desktop / Sinh viên chuyên ngành Kỹ thuật Phần mềm  
**Định dạng kiến trúc:** Model-View-ViewModel (MVVM) kết hợp Backend-for-Frontend (BFF) OWIN Self-Host  

---

## MỤC LỤC
1. Đánh giá Môi trường Phát triển: Visual Studio 2017 vs VS Code vs Visual Studio 2022
2. Kiến trúc Tổng thể & Kế hoạch Tái cấu trúc Solution
3. Danh mục Thư viện Phụ thuộc (NuGet Dependencies)
4. Quy trình Triển khai Chi tiết từng Bước (Step-by-Step Implementation Pipeline)
   - Bước 1: Khởi tạo và Cấu trúc Solution Đa dự án
   - Bước 2: Xây dựng Tầng Nhân (Core Layer)
   - Bước 3: Xây dựng Tầng Dịch vụ Cục bộ (Local BFF Layer với OWIN Self-Host)
   - Bước 4: Xây dựng Tầng Xử lý Âm thanh & Phân tích Tần số (Audio Engine & DSP)
   - Bước 5: Xây dựng Tầng Giao diện Người dùng (WPF Native Presentation Layer)
   - Bước 6: Bootstrap Hệ thống & Tích hợp Vòng đời Tiến trình
5. Kỹ thuật Phòng vệ (Defensive Programming) & Xử lý Rủi ro Đặc thù trên .NET 4.6.1
6. Quy trình Kiểm thử & Xác minh Tính đúng đắn (Verification Protocol)

---

## 1. ĐÁNH GIÁ MÔI TRƯỜNG PHÁT TRIỂN: VISUAL STUDIO 2017 VS VS CODE VS VISUAL STUDIO 2022

### 1.1. Thẩm định Hiện trạng Hệ thống Cục bộ
Qua kiểm tra tự động hệ thống phát triển cục bộ hiện tại:
- **Hệ thống đã cài đặt:** `Visual Studio Community 2017` (Phiên bản `15.9.37603.9`, build branch `d15.9`) tại đường dẫn `C:\Program Files (x86)\Microsoft Visual Studio\2017\Community`.
- **Trình biên dịch & Build Engine:** `MSBuild 15.0` (`C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe`) cùng bộ SDK `.NET Framework 4.6.1 Tools (resgen.exe, csc.exe)`.
- **Thử nghiệm biên dịch:** Dự án `MusicApp.sln` hiện tại được build thành công 100% không cảnh báo (0 Warning, 0 Error) bằng MSBuild 15.0.

### 1.2. Phân định Khái niệm: "Visual Studio Code" và "Visual Studio 2017"
- **Visual Studio Code (VS Code):** Là một trình biên tập mã nguồn nhẹ (Source Code Editor). VS Code không phát hành theo các phiên bản năm như "2017". Quan trọng hơn, VS Code **không hỗ trợ** trình thiết kế trực quan WPF (XAML Visual Designer), cơ chế gỡ lỗi (debugging) cho ứng dụng .NET Framework cổ điển (non-Core) gặp nhiều rào cản về cấu hình OmniSharp và MSBuild. Do đó, phát triển WPF native trên VS Code là phi thực tế và làm giảm nghiêm trọng năng suất làm việc.
- **Visual Studio 2017 (IDE):** Đây chính là môi trường đang có sẵn trên máy của bạn. Đây là một IDE đầy đủ (Integrated Development Environment), hỗ trợ toàn diện WPF XAML Designer, IntelliSense cho C# 7.0/7.3, trình quản lý gói NuGet và công cụ phân tích bộ nhớ.

### 1.3. Đánh giá Mức độ Thích hợp của Visual Studio 2017 đối với Dự án

#### Ưu điểm:
1. **Khả năng tương thích bản địa (Native Compatibility):** .NET Framework 4.6.1 ra đời trong giai đoạn Visual Studio 2015-2017. Tất cả các gói công cụ, bộ nạp assembly (Targeting Packs), và trình sinh mã BAML của WPF trên VS 2017 đều tương thích tuyệt đối.
2. **Sẵn sàng ngay lập tức:** Bạn không cần phải tải xuống thêm gigabyte dữ liệu hay thay đổi cấu hình máy tính hiện tại.

#### Hạn chế kỹ thuật so với các phiên bản mới hơn:
1. **Thiếu tính năng XAML Hot Reload:** Trong VS 2017, khi thay đổi giao diện XAML, bạn phải dừng ứng dụng (Stop Debugging) và biên dịch lại. Visual Studio 2022 hỗ trợ XAML Hot Reload và XAML Live Preview, cho phép sửa đổi giao diện trực tiếp trong lúc nhạc đang phát mà không làm gián đoạn ứng dụng.
2. **Kiến trúc 32-bit (devenv.exe):** VS 2017 bị giới hạn bộ nhớ ảo 4GB. Khi mở các giải pháp lớn kèm theo công cụ phân tích đồ họa, IDE có thể bị chậm.
3. **Trình quản lý gói NuGet:** VS 2017 mặc định sử dụng cơ chế `packages.config` thay vì cú pháp hiện đại `PackageReference`. `packages.config` dễ gây ô nhiễm thư mục dự án và khó giải quyết xung đột transitive dependencies nếu không cấu hình thận trọng.

### 1.4. Đề xuất Kiến trúc cho Nhà phát triển
- **Phương án Hiện hành (Khuyến nghị thực hiện ngay):** Giữ nguyên **Visual Studio Community 2017**. Phiên bản này hoàn toàn đáp ứng 100% tất cả các yêu cầu kỹ thuật của tài liệu TDD, từ biên dịch OWIN Self-Host, tích hợp NAudio, tới thiết kế giao diện WPF thuần XAML.
- **Phương án Nâng cấp Tùy chọn (Nếu có điều kiện băng thông):** Cài đặt thêm **Visual Studio 2022 Community (v17.x, 64-bit)**. Lý do nâng cấp duy nhất là để tận dụng tính năng **XAML Hot Reload** giúp việc tinh chỉnh hiệu ứng quay đĩa vinyl và thanh Equalizer diễn ra trực quan và nhanh chóng hơn. Mã nguồn viết ra trên VS 2022 hoàn toàn tương thích ngược 100% với VS 2017 nếu tuân thủ target `.NET Framework 4.6.1`.

---

## 2. KIẾN TRÚC TỔNG THỂ & KẾ HOẠCH TÁI CẤU TRÚC SOLUTION

Hiện tại dự án chỉ có một thư mục `MusicApp` đơn lẻ. Để tuân thủ nguyên lý Bounded Context (DDD) và cách ly phạm vi lỗi (Fault Isolation) theo TDD, Solution phải được mở rộng thành 4 dự án thành phần:

```text
MusicApp.sln
├── src/
│   ├── MusicApp.Core/             [.NET Framework 4.6.1 Class Library]
│   │   ├── Models/                (Domain Entities, Enums: PlaybackState)
│   │   ├── Dtos/                  (Data Transfer Objects cho API Contracts)
│   │   ├── Interfaces/            (IAudioService, IMusicApiClient, IMusicSourceProvider)
│   │   └── Common/                (ObservableObject, RelayCommand, AsyncCommand)
│   │
│   ├── MusicApp.Bff/              [.NET Framework 4.6.1 Class Library / Console Runner]
│   │   ├── Startup.cs             (OWIN HttpConfiguration, Routing, IoC)
│   │   ├── Controllers/           (TrackController, StreamController)
│   │   ├── Providers/             (JamendoSourceProvider)
│   │   ├── Middlewares/           (RangeStreamProxyMiddleware)
│   │   └── Services/              (MemoryCacheService)
│   │
│   ├── MusicApp.AudioEngine/      [.NET Framework 4.6.1 Class Library]
│   │   ├── NAudioService.cs       (WasapiOut, BufferedWaveProvider, Lifecycle)
│   │   ├── Stream/                (BufferedHttpWaveStream)
│   │   └── Dsp/                   (SampleAggregator, FftCalculator, SpectrumBin)
│   │
│   └── MusicApp/ (Client)         [.NET Framework 4.6.1 WPF Application]
│       ├── App.xaml / App.xaml.cs (TLS 1.2 Setup, BFF Background Thread Bootstrap)
│       ├── Resources/Themes/      (DarkTheme.xaml, LightTheme.xaml)
│       ├── ViewModels/            (MainViewModel, NowPlayingViewModel, TrackItemViewModel)
│       ├── Views/                 (MainWindow.xaml, NowPlayingCardView.xaml)
│       └── Converters/            (SecondsToTimeSpanConverter, PlaybackStateToVisibilityConverter)
└── tests/
    └── MusicApp.Tests/            [.NET Framework 4.6.1 Unit Test Project]
```

### Nguyên tắc Phụ thuộc giữa các Dự án (Dependency Direction)
- `MusicApp.Core` là tầng nhân: Không phụ thuộc vào bất kỳ dự án nào khác.
- `MusicApp.Bff` phụ thuộc vào: `MusicApp.Core`.
- `MusicApp.AudioEngine` phụ thuộc vào: `MusicApp.Core`.
- `MusicApp` (WPF Client) phụ thuộc vào: `MusicApp.Core`, `MusicApp.AudioEngine`, và tham chiếu khởi động tới `MusicApp.Bff`.
- Tuyệt đối không tạo phụ thuộc vòng (Circular Reference) giữa AudioEngine và Bff. Hai tầng này chỉ giao tiếp thông qua giao thức mạng HTTP Loopback cục bộ (`http://localhost:5245`).

---

## 3. DANH MỤC THƯ VIỆN PHỤ THUỘNG (NUGET DEPENDENCIES)

Dưới đây là danh sách các gói NuGet bắt buộc cài đặt cho từng dự án, tương thích chuẩn xác với `.NET Framework 4.6.1`:

| Tên Dự án | Gói NuGet cần cài đặt | Phiên bản khuyến nghị | Mục đích kỹ thuật |
| :--- | :--- | :--- | :--- |
| **MusicApp.Core** | *Không cần cài thêm gói ngoài* | Chuẩn GAC .NET 4.6.1 | Đảm bảo tính trung lập tuyệt đối của tầng nghiệp vụ lõi |
| **MusicApp.Bff** | `Microsoft.Owin` | `4.2.2` | Hạ tầng trừu tượng hóa máy chủ web OWIN |
| | `Microsoft.Owin.Hosting` | `4.2.2` | Quản lý tiến trình host ứng dụng OWIN |
| | `Microsoft.Owin.Host.HttpListener` | `4.2.2` | Bộ nạp HTTP Server cấp thấp trên nền Windows HttpListener |
| | `Microsoft.AspNet.WebApi.Owin` | `5.2.9` | Cầu nối tích hợp ASP.NET Web API 2 vào pipeline OWIN |
| | `Microsoft.AspNet.WebApi.Core` | `5.2.9` | Bộ điều khiển Controller, Định tuyến (Routing), Lọc dữ liệu |
| | `Newtonsoft.Json` | `13.0.3` | Tuần tự hóa / giải tuần tự hóa payload JSON hiệu năng cao |
| **MusicApp.AudioEngine** | `NAudio` | `1.10.0` hoặc `2.2.1` | Engine âm thanh: WASAPI, WaveOut, MP3 Frame Decompressor, FFT |
| **MusicApp** (Client) | `Newtonsoft.Json` | `13.0.3` | Xử lý DTO phản hồi từ Local BFF |
| **MusicApp.Tests** | `NUnit` | `3.13.3` | Khung kiểm thử tự động (Unit Testing Framework) |
| | `NUnit3TestAdapter` | `4.5.0` | Bộ tích hợp NUnit vào Test Explorer của Visual Studio |

---

## 4. QUY TRÌNH TRIỂN KHAI CHI TIẾT TỪNG BƯỚC

### BƯỚC 1: KHỞI TẠO VÀ CẤU TRÚC SOLUTION ĐA DỰ ÁN

#### Thao tác trên Visual Studio 2017:
1. Mở file `MusicApp.sln` bằng Visual Studio 2017.
2. Tạo thư mục cấu trúc logic trên Solution Explorer:
   - Nhấp chuột phải vào `Solution 'MusicApp'` -> Chọn **Add** -> **New Solution Folder** -> Đặt tên `src`.
   - Nhấp chuột phải vào `Solution 'MusicApp'` -> Chọn **Add** -> **New Solution Folder** -> Đặt tên `tests`.
   - Kéo project `MusicApp` hiện có vào thư mục `src`.
3. Tạo dự án `MusicApp.Core`:
   - Nhấp chuột phải vào thư mục `src` -> **Add** -> **New Project...**
   - Chọn cây thư mục: **Visual C#** -> **Class Library (.NET Framework)**.
   - Nhập Name: `MusicApp.Core`.
   - Đảm bảo Framework được chọn là: `.NET Framework 4.6.1`.
   - Nhấn **OK**. Xóa bỏ file `Class1.cs` mặc định.
4. Tạo dự án `MusicApp.Bff`:
   - Nhấp chuột phải vào thư mục `src` -> **Add** -> **New Project...**
   - Chọn: **Visual C#** -> **Class Library (.NET Framework)** (hoặc Console App nếu muốn chạy cửa sổ độc lập khi gỡ lỗi).
   - Nhập Name: `MusicApp.Bff`. Target: `.NET Framework 4.6.1`.
   - Nhấn **OK**. Xóa bỏ file `Class1.cs` mặc định.
5. Tạo dự án `MusicApp.AudioEngine`:
   - Nhấp chuột phải vào thư mục `src` -> **Add** -> **New Project...**
   - Chọn: **Visual C#** -> **Class Library (.NET Framework)**.
   - Nhập Name: `MusicApp.AudioEngine`. Target: `.NET Framework 4.6.1`.
   - Nhấn **OK**. Xóa bỏ file `Class1.cs` mặc định.
6. Thiết lập Project References:
   - Nhấp chuột phải vào `MusicApp.Bff` -> **Add** -> **Reference...** -> Chọn **Projects** -> Tích chọn `MusicApp.Core`.
   - Nhấp chuột phải vào `MusicApp.AudioEngine` -> **Add** -> **Reference...** -> Chọn **Projects** -> Tích chọn `MusicApp.Core`.
   - Nhấp chuột phải vào `MusicApp` (Client) -> **Add** -> **Reference...** -> Chọn **Projects** -> Tích chọn `MusicApp.Core`, `MusicApp.AudioEngine`, `MusicApp.Bff`.

---

### BƯỚC 2: XÂY DỰNG TẦNG NHÂN (CORE LAYER - `MusicApp.Core`)

Tầng này định nghĩa các quy chuẩn dữ liệu và hợp đồng trừu tượng, không chứa logic công nghệ cụ thể.

#### 2.1. Phân mục Thư mục trong `MusicApp.Core`:
- `Common/`
- `Models/`
- `Dtos/`
- `Interfaces/`

#### 2.2. Chi tiết các thành phần cần viết:
1. **`Common/ObservableObject.cs`:**
   - Triển khai `INotifyPropertyChanged`.
   - Sử dụng cơ chế `[CallerMemberName]` để thông báo tự động cho WPF Binding Engine mà không cần truyền chuỗi tên thuộc tính thủ công.
   - Viết phương thức `SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)` có kiểm tra `Equals(storage, value)` để triệt tiêu các thông báo trùng lặp gây lãng phí chu kỳ vẽ của UI.
2. **`Common/RelayCommand.cs` và `AsyncRelayCommand.cs`:**
   - Triển khai `ICommand`.
   - Dẫn xuất lệnh từ UI tới các hàm xử lý trong ViewModel.
   - Liên kết sự kiện `CanExecuteChanged` tới `CommandManager.RequerySuggested` để WPF tự động cập nhật trạng thái Enabled/Disabled của các nút bấm.
   - Cung cấp phương thức thủ công `RaiseCanExecuteChanged()`.
3. **`Models/PlaybackState.cs`:**
   - Khai báo enum máy trạng thái: `Stopped`, `Buffering`, `Playing`, `Paused`, `Faulted`.
4. **`Models/TrackModel.cs`:**
   - Đại diện cho thực thể bài hát trong ứng dụng: `Id`, `Title`, `Artist`, `Album`, `DurationSeconds`, `CoverImageUrl`, `StreamUrl`, `License`.
5. **`Dtos/SearchResponseDto.cs` và `Dtos/TrackDto.cs`:**
   - Các lớp dữ liệu truyền tải theo đúng hợp đồng JSON REST API được quy định trong TDD.
6. **`Interfaces/IAudioService.cs`:**
   - Khai báo hợp đồng điều khiển âm thanh:
     - Các phương thức: `Task InitializeAsync(string streamUrl)`, `void Play()`, `void Pause()`, `void Stop()`, `void Seek(TimeSpan position)`, `void SetVolume(float volume)`.
     - Các thuộc tính: `PlaybackState CurrentState`, `TimeSpan CurrentTime`, `TimeSpan TotalTime`.
     - Sự kiện: `event EventHandler<float[]> SpectrumDataReady;` (truyền mảng 16 bins tần số), `event EventHandler<PlaybackState> StateChanged;`.
7. **`Interfaces/IMusicApiClient.cs`:**
   - Khai báo hợp đồng gọi API tìm kiếm: `Task<SearchResponseDto> SearchTracksAsync(string query, int limit, CancellationToken ct)`.

---

### BƯỚC 3: XÂY DỰNG TẦNG DỊCH VỤ CỤC BỘ (LOCAL BFF - `MusicApp.Bff`)

Local BFF giải quyết bài toán cách ly an ninh API Key, xử lý Rate Limit và thực hiện Proxy luồng âm thanh có hỗ trợ HTTP Range.

#### 3.1. Cài đặt Gói NuGet cho `MusicApp.Bff`:
Mở **Tools** -> **NuGet Package Manager** -> **Package Manager Console**, chọn Default project là `MusicApp.Bff` và thực thi:
```powershell
Install-Package Microsoft.Owin.SelfHost -Version 4.2.2 -ProjectName MusicApp.Bff
Install-Package Microsoft.AspNet.WebApi.OwinSelfHost -Version 5.2.9 -ProjectName MusicApp.Bff
Install-Package Newtonsoft.Json -Version 13.0.3 -ProjectName MusicApp.Bff
```
Thêm tham chiếu GAC: Nhấp chuột phải `References` -> **Add Reference...** -> **Assemblies** -> **Framework** -> Tích chọn `System.Runtime.Caching` và `System.Net.Http`.

#### 3.2. Cấu trúc Triển khai trong `MusicApp.Bff`:
1. **`Startup.cs`:**
   - Cấu hình OWIN pipeline: Khởi tạo `HttpConfiguration`.
   - Cấu hình MapHttpAttributeRoutes() hoặc quy tắc route mặc định:
     `routeTemplate: "api/v1/{controller}/{action}/{id}", defaults: new { id = RouteParameter.Optional }`.
   - Cấu hình bộ nạp JSON Formatter: loại bỏ XML Formatter để API luôn trả về JSON sạch.
2. **`Services/MemoryCacheService.cs`:**
   - Sử dụng `System.Runtime.Caching.MemoryCache.Default`.
   - Thiết lập phương thức `GetOrCreateAsync<T>(string key, TimeSpan slidingExpiration, Func<Task<T>> factory)`.
   - Cache kết quả tìm kiếm trong 30 phút nhằm ngăn chặn việc vượt hạn ngạch 100 requests/phút của các API miễn phí.
3. **`Providers/JamendoSourceProvider.cs`:**
   - Giao tiếp với Jamendo REST API v3.0: `https://api.jamendo.com/v3.0/tracks/`.
   - Sử dụng Client ID công khai được cấp phép của Jamendo cho mục đích giáo dục / phi thương mại.
   - Đọc JSON trả về, chuẩn hóa sang định dạng `TrackDto`, gán `streamEndpoint = "/api/v1/stream/" + trackId`.
4. **`Controllers/TrackController.cs`:**
   - Kế thừa `ApiController`.
   - Endpoint: `[HttpGet] [Route("api/v1/search")] public async Task<IHttpActionResult> Search([FromUri] string query, [FromUri] int limit = 20, [FromUri] int page = 1)`.
   - Áp dụng kỹ thuật kiểm tra tham số đầu vào (Guard Clauses): Nếu query rỗng, trả về `BadRequest("Query parameter is required")`.
   - Gọi qua `MemoryCacheService` để lấy dữ liệu từ cache hoặc từ `JamendoSourceProvider`.
5. **`Controllers/StreamController.cs` (Kỹ thuật Trọng tâm: HTTP Range Proxy):**
   - Endpoint: `[HttpGet] [Route("api/v1/stream/{id}")] public async Task<HttpResponseMessage> Stream(string id)`.
   - **Xử lý HTTP Range Header:**
     - Đọc tiêu đề `Request.Headers.Range`.
     - Tạo một `HttpRequestMessage` mới gửi tới URL CDN gốc của bản nhạc, đính kèm đúng tiêu đề `Range` nhận được.
     - Đọc phản hồi từ CDN máy chủ: Nếu CDN trả về `206 Partial Content`, BFF trả về cho NAudio mã `206 Partial Content` kèm toàn bộ các header `Content-Range`, `Content-Length`, `Content-Type: audio/mpeg`.
     - Kỹ thuật này cho phép Audio Engine tua bài tức thì mà không phải nạp toàn bộ file vào bộ nhớ RAM.
6. **`BffServerHost.cs`:**
   - Đóng gói vòng đời của máy chủ OWIN:
     - `public static IDisposable Start(string baseUri = "http://localhost:5245") => WebApp.Start<Startup>(baseUri);`.

---

### BƯỚC 4: XÂY DỰNG TẦNG AUDIO ENGINE & DSP (`MusicApp.AudioEngine`)

Tầng này chịu trách nhiệm nạp dữ liệu stream, giải mã MP3 sang mẫu âm thanh PCM, điều khiển thiết bị ngõ ra âm thanh và trích xuất dữ liệu phổ tần số FFT.

#### 4.1. Cài đặt Gói NuGet cho `MusicApp.AudioEngine`:
Trong **Package Manager Console**, chọn Default project là `MusicApp.AudioEngine`:
```powershell
Install-Package NAudio -Version 1.10.0 -ProjectName MusicApp.AudioEngine
```

#### 4.2. Cấu trúc Triển khai trong `MusicApp.AudioEngine`:
1. **`Dsp/SampleAggregator.cs` (Kỹ thuật Triệt tiêu LOH Memory Leak):**
   - **Lý do thiết kế:** Trên .NET 4.6.1, việc liên tục tạo mảng `new float[1024]` 60 lần mỗi giây sẽ tạo ra hàng chục nghìn đối tượng rác đẩy vào Garbage Collector, gây hiện tượng khựng UI.
   - **Giải pháp:** Cấp phát một bộ đệm tĩnh cố định (Pre-allocated Static Ring Buffer) kích thước 1024 phần tử số thực.
   - Mỗi khi luồng âm thanh đọc được một mẫu PCM (sample), nạp mẫu vào Ring Buffer. Khi đủ 1024 mẫu, kích hoạt giải thuật tính toán FFT trên chính mảng đệm cố định này.
2. **`Dsp/FftCalculator.cs`:**
   - Sử dụng bộ giải thuật biến đổi Fourier nhanh của NAudio: `NAudio.Dsp.FastFourierTransform.FFT(true, m, complexBuffer)`. Với kích thước 1024 mẫu, bậc số mũ `m = 10` ($2^{10} = 1024$).
   - Sau khi tính xong, tính toán biên độ cường độ: $Intensity = \sqrt{Re^2 + Im^2}$.
   - Phân cụm 512 vạch phổ kết quả thành **16 bins tần số đại diện**:
     - Bins 0-3: Tần số trầm (Bass / Sub-bass: 20Hz - 250Hz).
     - Bins 4-10: Tần số trung (Midrange / Vocal: 250Hz - 4000Hz).
     - Bins 11-15: Tần số cao (Treble: 4000Hz - 20000Hz).
   - Chuẩn hóa biên độ về khoảng giá trị từ `0.0` đến `35.0` (phù hợp với chiều cao pixel của cột đồ thị trên giao diện).
3. **`Stream/BufferedHttpWaveStream.cs`:**
   - Quản lý luồng mạng từ `http://localhost:5245/api/v1/stream/{id}`.
   - Sử dụng `BufferedWaveProvider` của NAudio để thiết lập ngưỡng đệm an toàn (Pre-buffer threshold ~ 2-3 giây âm thanh) nhằm triệt tiêu hoàn toàn hiện tượng rè tiếng hoặc ngắt quãng khi mạng lag.
4. **`NAudioService.cs` (Triển khai `IAudioService`):**
   - Khởi tạo thiết bị xuất âm thanh qua `WasapiOut` (hoặc `WaveOutEvent` làm cơ chế dự phòng tương thích cao).
   - Kiểm soát vòng đời: `Play()`, `Pause()`, `Stop()`, `Seek()`.
   - **Bộ điều tiết tốc độ (Throttling Mechanism):**
     - Sử dụng `System.Diagnostics.Stopwatch`.
     - Chỉ bắn sự kiện `SpectrumDataReady` lên UI khi thời gian trôi qua giữa 2 lần cập nhật đạt tối thiểu 33 mili-giây (~30 khung hình/giây). Việc này tránh làm nghẽn hàng đợi thông điệp Dispatcher của WPF.

---

### BƯỚC 5: XÂY DỰNG TẦNG GIAO DIỆN NGƯỜI DÙNG (WPF CLIENT - `MusicApp`)

Tầng Presentation chịu trách nhiệm trình diễn đồ họa, hoàn toàn bằng XAML thuần túy kết hợp Code-Behind và MVVM.

#### 5.1. Cấu hình Tài nguyên Giao diện Động (Dynamic Theme Dictionary)
Trong dự án `MusicApp`, tạo thư mục `Resources/Themes/`:
1. **`Resources/Themes/DarkTheme.xaml`:**
   - Định nghĩa các cọ vẽ `SolidColorBrush` với khóa tài nguyên chuẩn:
     - `CardBackgroundBrush`: `#1E1E1E` (Màu nền thẻ tối).
     - `TitleForegroundBrush`: `#FFFFFF` (Màu chữ tiêu đề trắng).
     - `SubtitleForegroundBrush`: `#B3B3B3` (Màu chữ nghệ sĩ xám nhạt).
     - `AccentBrandBrush`: `#1DB954` (Màu xanh Spotify đặc trưng).
     - `CardShadowColor`: `#000000`.
2. **`Resources/Themes/LightTheme.xaml`:**
   - Định nghĩa các cọ vẽ tương ứng cho chế độ sáng:
     - `CardBackgroundBrush`: `#F8F9FA`.
     - `TitleForegroundBrush`: `#212529`.
     - `SubtitleForegroundBrush`: `#6C757D`.
     - `AccentBrandBrush`: `#1ED760`.
     - `CardShadowColor`: `#B0B0B0`.
3. Đăng ký ResourceDictionary vào `App.xaml`:
   - Sử dụng `MergedDictionaries` để nạp `DarkTheme.xaml` làm chủ đề mặc định. Khi người dùng bấm nút chuyển theme, ViewModel chỉ cần thay thế từ điển tài nguyên trong `Application.Current.Resources.MergedDictionaries` là toàn bộ cửa sổ sẽ đổi màu tức thì trong thời gian chạy.

#### 5.2. Xây dựng Khung Thẻ bài hát (`NowPlayingCardView.xaml` & `.xaml.cs`)
Tạo thư mục `Views/` và thêm một **WPF User Control (XAML)** tên `NowPlayingCardView.xaml`:
1. **Nội dung XAML:**
   - Áp dụng cấu trúc XAML chuẩn mực đã cung cấp trong TDD (Mục 2.2).
   - **Xoay đĩa Vinyl:**
     - Sử dụng `Storyboard` chứa `DoubleAnimation` tác động lên `RotateTransform.Angle` từ 0 đến 360 độ, lặp vô tận (`RepeatBehavior="Forever"`).
     - Đặt thuộc tính `RenderTransformOrigin="0.5,0.5"` để tâm xoay nằm chính giữa đĩa.
     - Cắt ảnh đĩa thành hình tròn bản địa bằng `Image.Clip` với thẻ `<EllipseGeometry Center="60,60" RadiusX="60" RadiusY="60" />`.
   - **Hiển thị Phổ Tần số (Equalizer):**
     - Sử dụng `ItemsControl` liên kết với thuộc tính `ObservableCollection<double> EqualizerBins`.
     - `ItemsPanel` là một `StackPanel Orientation="Horizontal" VerticalAlignment="Bottom"`.
     - `ItemTemplate` là một `Rectangle Width="6" Height="{Binding}" Fill="{DynamicResource AccentBrandBrush}" RadiusX="2" RadiusY="2" Margin="2,0"`.
   - **Thanh tiến độ thời gian (Timeline Slider):**
     - Ràng buộc hai chiều (`Mode=TwoWay`) với `CurrentPositionSeconds`.
     - Hiển thị nhãn thời gian qua thuộc tính chuỗi định dạng sẵn `FormattedPosition` và `FormattedDuration`.
2. **Nội dung Code-Behind (`NowPlayingCardView.xaml.cs`):**
   - Giữ code-behind ở mức tối giản, chỉ xử lý việc điều khiển Storyboard:
   - Lắng nghe sự kiện thay đổi thuộc tính `PlaybackState` của ViewModel:
     - Khi trạng thái chuyển sang `Playing`: Gọi `((Storyboard)Resources["VinylSpinStoryboard"]).Begin(this, true);` hoặc `Resume()`.
     - Khi trạng thái chuyển sang `Paused`: Gọi `Pause()`.
     - Khi trạng thái chuyển sang `Stopped`: Gọi `Stop()`.

#### 5.3. Xây dựng Cửa sổ Chính (`MainWindow.xaml` & `.xaml.cs`)
1. **Bố cục giao diện XAML:**
   - Chia lưới `Grid` thành 3 hàng:
     - **Hàng 0 (Top Bar):** Hộp tìm kiếm (`TextBox` từ khóa) kèm nút Tìm kiếm, nút đổi Theme (Sáng/Tối).
     - **Hàng 1 (Content Area):** Danh sách kết quả tìm kiếm hiển thị dưới dạng `ListBox` (sử dụng VirtualizingStackPanel để tối ưu cuộn mượt khi danh sách có hàng trăm bài hát). Mỗi dòng hiển thị ảnh thumbnail, tên bài hát, nghệ sĩ, thời lượng và nút Phát.
     - **Hàng 2 (Bottom Bar):** Nhúng UserControl `<views:NowPlayingCardView DataContext="{Binding NowPlaying}" />` để hiển thị trình phát nhạc cố định dưới đáy màn hình.
2. **Code-Behind (`MainWindow.xaml.cs`):**
   - Gán `DataContext = new MainViewModel(...)`.
   - Xử lý sự kiện đóng cửa sổ `Window_Closing` để đảm bảo gọi dọn dẹp tài nguyên âm thanh và dừng máy chủ BFF.

#### 5.4. Xây dựng các Lớp ViewModel (Tầng Logic Điều khiển)
Tạo thư mục `ViewModels/` trong `MusicApp`:
1. **`TrackItemViewModel.cs`:**
   - Bao bọc đối tượng `TrackModel`.
   - Cung cấp các lệnh: `PlayThisTrackCommand`.
2. **`NowPlayingViewModel.cs`:**
   - Kế thừa `ObservableObject`.
   - Chứa `CurrentTrack`, `PlaybackState`, `CurrentPositionSeconds`, `TrackDurationSeconds`.
   - Chứa danh sách `ObservableCollection<double> EqualizerBins` (gồm 16 phần tử).
   - Lắng nghe sự kiện `SpectrumDataReady` từ `IAudioService`:
     - Nhận mảng 16 giá trị float từ luồng DSP.
     - Cập nhật giá trị vào `EqualizerBins` trên luồng UI thông qua `Dispatcher.InvokeAsync(..., DispatcherPriority.Render)`.
   - Chứa các lệnh điều khiển: `PlayCommand`, `PauseCommand`, `StopCommand`, `SeekCommand`.
3. **`MainViewModel.cs`:**
   - Quản lý trạng thái tìm kiếm: `SearchKeyword`, `IsSearching`, `SearchResults` (`ObservableCollection<TrackItemViewModel>`).
   - Khởi tạo cơ chế **Debouncing** cho ô tìm kiếm: Sử dụng `CancellationTokenSource` chờ 400ms sau khi người dùng ngừng gõ mới phát lệnh gọi mạng, tránh làm tràn ngập request lên server.
   - Chứa instance của `NowPlayingViewModel`.

---

### BƯỚC 6: BOOTSTRAP HỆ THỐNG & TÍCH HỢP VÒNG ĐỜI TIẾN TRÌNH

Tất cả các thành phần phải được liên kết chặt chẽ trong `App.xaml.cs`:

1. **Thiết lập An ninh Mạng tại Điểm khởi đầu (Entry Point):**
   - Trong phương thức `App.OnStartup(StartupEventArgs e)`:
     ```csharp
     // Kich hoat bat buoc TLS 1.2 tren toan bo tien trinh .NET 4.6.1
     ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
     ServicePointManager.DefaultConnectionLimit = 64;
     ```
2. **Khởi động Local BFF trong Tiến trình chạy ngầm:**
   - Khởi chạy OWIN Self-Host trên một luồng nền riêng biệt hoặc qua biến toàn cục:
     ```csharp
     _bffInstance = WebApp.Start<MusicApp.Bff.Startup>("http://localhost:5245");
     ```
3. **Khởi tạo và Tiêm Phụ thuộc (Dependency Injection Bootstrap):**
   - Khởi tạo thực thể duy nhất (Singleton) của `NAudioService`.
   - Khởi tạo `MusicApiClient("http://localhost:5245/api/v1")`.
   - Khởi tạo `MainViewModel` và truyền các phụ thuộc vào.
   - Gán `MainWindow.DataContext = mainViewModel; MainWindow.Show();`.
4. **Giải phóng Tài nguyên khi Ứng dụng Thoát (`OnExit`):**
   - Bắt buộc giải phóng `_audioService.Dispose()` để ngắt handle thiết bị WASAPI.
   - Bắt buộc giải phóng `_bffInstance?.Dispose()` để đóng port 5245 của HttpListener, tránh lỗi chiếm dụng cổng khi mở lại ứng dụng.

---

## 5. KỸ THUẬT PHÒNG VỆ & XỬ LÝ RỦI RO ĐẶC THÙ TRÊN .NET 4.6.1

Theo quy chuẩn kỹ sư cao cấp, mã nguồn không được dừng lại ở trường hợp lý tưởng mà phải phòng ngừa triệt để các rủi ro hạ tầng:

### 5.1. Triệt tiêu Rò rỉ Bộ nhớ do Ảnh (WPF BitmapImage Memory Leak)
- **Vấn đề:** Khi tải ảnh bìa album từ URL qua `new BitmapImage(new Uri(url))`, WPF sẽ giữ tham chiếu ngầm và tạo các đối tượng unmanaged trong bộ nhớ mà GC không thể tự động dọn dẹp, gây phình dung lượng RAM sau vài bài hát.
- **Giải pháp:** Tải mảng byte của ảnh về trước, nạp vào `MemoryStream`, khởi tạo `BitmapImage`, và bắt buộc gọi phương thức `bitmap.Freeze()` trước khi trả về cho UI. Lệnh `Freeze()` biến đối tượng thành bất biến (Thread-Safe & Read-Only), cho phép giải phóng toàn bộ luồng đọc dữ liệu.

### 5.2. Triệt tiêu Rò rỉ LOH trong Bộ xử lý Âm thanh (DSP Allocations)
- **Vấn đề:** .NET Framework 4.6.1 không hỗ trợ `Span<T>` hay `ArrayPool<T>`.
- **Giải pháp:** Toàn bộ mảng số thực phục vụ thuật toán FFT (kích thước 1024) phải được khởi tạo một lần duy nhất dưới dạng bộ nhớ đệm thành viên (`private readonly float[] _fftBuffer = new float[1024]`). Trong suốt quá trình phát nhạc, thuật toán chỉ ghi đè lên các chỉ số của mảng này, tuyệt đối không gọi toán tử `new` trong các phương thức đọc âm thanh lặp đi lặp lại.

### 5.3. Bảo vệ Hàng đợi Thông điệp Giao diện (Dispatcher Queue Starvation)
- **Vấn đề:** Bản nhạc phát ở tốc độ 44,100 Hz, việc đọc mẫu diễn ra liên tục hàng nghìn lần mỗi giây. Nếu bắn sự kiện lên UI mỗi khi có dữ liệu, luồng Dispatcher sẽ nghẽn hoàn toàn, khiến ứng dụng bị đơ chuột.
- **Giải pháp:** Sử dụng cơ chế đo thời gian `Stopwatch` làm van tiết lưu (Throttling). Chỉ đẩy dữ liệu tần số lên ViewModel khi khoảng thời gian cách lần gửi trước tối thiểu 33ms (~30fps). Sử dụng mức độ ưu tiên `DispatcherPriority.Render` hoặc `Background` để nhường CPU cho các thao tác bấm nút của người dùng.

### 5.4. Xử lý Lỗi Bắt tay TLS/SSL trên Máy trạm Cũ
- **Vấn đề:** Một số máy tính Windows 7/8.1 chưa cập nhật bản vá bảo mật có thể bị từ chối kết nối HTTPS với máy chủ âm thanh do thiếu các bộ mã hóa (Cipher Suites).
- **Giải pháp:** Toàn bộ kết nối ra Internet công cộng được cô lập hoàn toàn bên trong tầng Local BFF. Tầng Client WPF chỉ kết nối thuần túy với Localhost qua giao thức HTTP không mã hóa (`http://localhost:5245`). Nhờ đó, nếu có sự cố về chứng chỉ số hoặc bắt tay SSL, lỗi sẽ được khoanh vùng và bắt gọn tại lớp Service của BFF mà không làm sập giao diện người dùng.

---

## 6. QUY TRÌNH KIỂM THỬ & XÁC MINH TÍNH ĐÚNG ĐẮN (VERIFICATION PROTOCOL)

Sau khi hoàn thành việc viết code theo các bước trên, hãy thực hiện quy trình kiểm định 4 giai đoạn sau:

### Giai đoạn 1: Biên dịch Hệ thống
1. Trong Visual Studio 2017, chọn cấu hình: **Debug | Any CPU**.
2. Nhấn menu **Build** -> **Rebuild Solution** (hoặc phím tắt `Ctrl + Shift + B`).
3. **Tiêu chuẩn đạt:** Đầu ra `Build: 4 succeeded, 0 failed, 0 up-to-date, 0 skipped` với 0 Warning và 0 Error.

### Giai đoạn 2: Kiểm thử Hợp đồng API Local BFF
1. Chạy ứng dụng hoặc chạy độc lập module BFF.
2. Mở trình duyệt hoặc công cụ dòng lệnh PowerShell, thực hiện kiểm tra endpoint tìm kiếm:
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5245/api/v1/search?query=electronic&limit=5" -Method Get
   ```
3. **Tiêu chuẩn đạt:** Trả về đối tượng JSON chứa danh sách bài hát hợp lệ từ Jamendo, có đầy đủ `title`, `artist`, và `streamEndpoint`.

### Giai đoạn 3: Kiểm thử Trình diễn Đồ họa & Xoay đĩa Vinyl
1. Chọn một bài hát bất kỳ từ danh sách kết quả và nhấn nút **Phát**.
2. **Quan sát:**
   - Đĩa nhạc Vinyl bo tròn bắt đầu xoay đều quanh tâm theo chiều kim đồng hồ với chu kỳ 10 giây/vòng.
   - Nhấn **Tạm dừng (Pause)**: Đĩa dừng xoay ngay tại vị trí góc hiện tại.
   - Nhấn **Tiếp tục (Play)**: Đĩa tiếp tục xoay tiếp từ góc đã dừng, không bị giật về góc 0.
   - Chuyển đổi qua lại giữa nút chế độ **Sáng / Tối**: Toàn bộ màu nền, màu chữ và màu viền bóng thay đổi mượt mà mà không làm gián đoạn bài nhạc đang phát.

### Giai đoạn 4: Kiểm thử Phân tích Tần số Thực tế (Equalizer Bins)
1. Trong lúc bài hát đang phát, quan sát 16 cột hình chữ nhật màu xanh lá:
2. **Tiêu chuẩn đạt:**
   - Các cột ở dải Bass (bên trái) nhấp nhô mạnh theo từng nhịp trống của bản nhạc.
   - Khi bài hát bước vào đoạn tĩnh (không có nhạc), các cột hạ thấp về mức 0.
   - Kéo thanh trượt vị trí bài hát (Slider) đến một đoạn bất kỳ: Luồng âm thanh nhảy mốc phát tức thì (nhờ header HTTP Range) và phổ tần số tiếp tục phản hồi chính xác.

---
**Tài liệu hướng dẫn kỹ thuật được ban hành bởi:** Senior WPF System Architect  
**Trạng thái kiểm duyệt:** Sẵn sàng cho giai đoạn lập trình và cấu trúc mã nguồn.
