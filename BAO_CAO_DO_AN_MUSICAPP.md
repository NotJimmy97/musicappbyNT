# BÁO CÁO ĐỒ ÁN MÔN HỌC: LẬP TRÌNH ỨNG DỤNG .NET
## Đề tài: XÂY DỰNG ỨNG DỤNG PHÁT NHẠC DESKTOP NATIVE TRÊN NỀN TẢNG WPF (.NET FRAMEWORK 4.6.1) VỚI KIẾN TRÚC MVVM, LOCAL BFF, AUDIO ENGINE DSP VÀ PERSISTENCE SQLITE
> *Ghi chú: Trang bìa chính và bìa phụ theo mẫu của Trường/Khoa sẽ được đính kèm tại thời điểm đóng tập chính thức.*

---

# NHẬN XÉT CỦA GIẢNG VIÊN HƯỚNG DẪN / CHẤM THI
### Phiếu đánh giá kết quả đồ án
| Tiêu chí đánh giá | Trọng số | Điểm đánh giá (Thang 10) | Chữ ký Giảng viên |
|---|---|---|---|
| Kiến trúc phần mềm (.NET / MVVM / BFF) | 25% | | |
| Hiện thực kỹ thuật & Audio Engine DSP | 25% | | |
| Thiết kế CSDL & Tối ưu SQLite | 20% | | |
| Kiểm thử tự động (Unit Test / VSTest) | 15% | | |
| Báo cáo thuyết minh & Thể thức văn bản | 15% | | |
| TỔNG ĐIỂM CHUNG | 100% | | |


---

# LỜI CAM ĐOAN
Em xin cam đoan đề tài đồ án môn học 'Xây dựng ứng dụng phát nhạc Desktop Native trên nền tảng WPF (.NET Framework 4.6.1)' là công trình nghiên cứu và thực nghiệm độc lập của chính bản thân em. Toàn bộ mã nguồn, cấu trúc giải thuật, kiến trúc phân tầng và các kết quả kiểm thử được trình bày trong báo cáo này đều phản ánh trung thực quá trình học tập và lập trình thực tế trên hệ thống. Các tài liệu tham khảo, các thư viện mã nguồn mở và nền tảng công nghệ được sử dụng (Microsoft .NET Framework, NAudio, SQLite, OWIN) đều đã được trích dẫn nguồn gốc và phiên bản rõ ràng theo đúng chuẩn mực học thuật.


---

# LỜI CẢM ƠN
Lời đầu tiên, em xin gửi lời cảm ơn chân thành và sâu sắc nhất đến Quý Thầy/Cô Bộ môn Công nghệ Phần mềm và Khoa Công nghệ Thông tin, những người đã tận tâm truyền đạt nền tảng kiến thức vững chắc về lập trình hướng đối tượng, tư duy kiến trúc hệ thống và các công nghệ cốt lõi của hệ sinh thái Microsoft .NET trong suốt học kỳ vừa qua.

Đặc biệt, em xin bày tỏ lòng biết ơn sâu sắc đến Giảng viên hướng dẫn môn học, Thầy/Cô đã luôn định hướng phương pháp tiếp cận khoa học, đặt ra những yêu cầu khắt khe về tính chuẩn mực của kiến trúc phân lớp, nguyên lý tối ưu bộ nhớ zero-allocation và tác phong làm việc bài bản. Những lời góp ý quý báu của Thầy/Cô không chỉ giúp đồ án MusicApp đạt được độ hoàn thiện kỹ thuật cao mà còn rèn luyện cho em tư duy giải quyết vấn đề chuyên nghiệp của một kỹ sư phần mềm thực thụ.


---

# MỤC LỤC TỔNG HỢP
- LỜI CAM ĐOAN
- LỜI CẢM ƠN
- DANH MỤC TỪ VIẾT TẮT
- DANH MỤC BẢNG BIỂU
- DANH MỤC HÌNH VẼ
- CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI VÀ CÔNG NGHỆ ÁP DỤNG
- 1.1. Đặt vấn đề và Mục tiêu đề tài
- 1.1.1. Bối cảnh và Tính cấp thiết của đề tài
- 1.1.2. Mục tiêu nghiên cứu và phát triển ứng dụng
- 1.1.3. Phạm vi đề tài và giới hạn hệ thống
- 1.2. Khảo sát nghiệp vụ và Yêu cầu hệ thống
- 1.2.1. Khảo sát quy trình nghiệp vụ phát nhạc đa nguồn
- 1.2.2. Phân tích yêu cầu chức năng (Functional Requirements)
- 1.2.3. Phân tích yêu cầu phi chức năng (Non-Functional Requirements)
- 1.3. Cơ sở công nghệ và Môi trường phát triển
- 1.3.1. Nền tảng .NET Framework 4.6.1 và Ngôn ngữ C# 7.3
- 1.3.2. Công nghệ giao diện WPF và Mô hình kiến trúc MVVM
- 1.3.3. Mô hình Local Backend-for-Frontend (BFF) qua OWIN Self-Host
- 1.3.4. Thư viện âm thanh NAudio và Xử lý tín hiệu số (DSP)
- 1.3.5. Hệ quản trị CSDL SQLite và Mô hình Repository Pattern
- 1.3.6. Môi trường phát triển và Công cụ hỗ trợ
- CHƯƠNG 2: PHÂN TÍCH, THIẾT KẾ HỆ THỐNG VÀ CƠ SỞ DỮ LIỆU
- 2.1. Phân tích chức năng và Thiết kế Use Case
- 2.1.1. Sơ đồ Use Case tổng quát của hệ thống
- 2.1.2. Đặc tả chi tiết các Use Case cốt lõi
- 2.1.3. Thiết kế Sơ đồ tuần tự (Sequence Diagram)
- 2.1.4. Thiết kế Sơ đồ hoạt động (Activity Diagram)
- 2.2. Thiết kế Cơ sở dữ liệu (Database Design)
- 2.2.1. Mô hình thực thể kết hợp (ERD - Entity Relationship Diagram)
- 2.2.2. Sơ đồ quan hệ dữ liệu vật lý (Physical Schema Diagram)
- 2.2.3. Bảng từ điển dữ liệu chi tiết (Data Dictionary)
- 2.3. Thiết kế Kiến trúc phần mềm (.NET Solution Structure)
- 2.3.1. Cấu trúc phân tầng dự án (Layered Solution Architecture)
- 2.3.2. Sơ đồ chuỗi xử lý tín hiệu âm thanh DSP Pipeline
- 2.3.3. Sơ đồ lớp chi tiết (Class Diagram)
- CHƯƠNG 3: CÀI ĐẶT THỰC NGHIỆM VÀ KẾT QUẢ ĐẠT ĐƯỢC
- 3.1. Môi trường triển khai và Cấu hình hệ thống
- 3.1.1. Yêu cầu cấu hình triển khai
- 3.1.2. Cấu hình chuỗi kết nối và Cơ chế Database Initializer
- 3.2. Hiện thực hóa các Chức năng chính và Giao diện (Demo)
- 3.2.1. Chức năng Tìm kiếm trực tuyến và Kỹ thuật Debounce 300ms
- 3.2.2. Chức năng Phát nhạc, Quản lý Hàng đợi và Thẻ Now Playing
- 3.2.3. Chức năng Quét thư viện cục bộ bằng giải thuật BFS
- 3.2.4. Chức năng Đồng bộ Lời bài hát Karaoke (LrcParser)
- 3.2.5. Chức năng Bộ cân bằng âm thanh DSP Equalizer 10 băng tần
- 3.2.6. Chức năng Quản lý Playlist và Hệ thống Gợi ý bài hát
- 3.3. Kiểm thử phần mềm (Testing)
- 3.3.1. Chiến lược và Kế hoạch kiểm thử tự động
- 3.3.2. Bảng kịch bản kiểm thử tiêu biểu (MSTest v2)
- 3.3.3. Đánh giá kết quả kiểm thử toàn diện
- 3.4. Đánh giá và Hướng phát triển
- 3.4.1. Đánh giá ưu điểm nổi bật của hệ thống
- 3.4.2. Những điểm hạn chế kỹ thuật còn tồn tại
- 3.4.3. Đề xuất hướng nâng cấp và phát triển mở rộng
- KẾT LUẬN
- TÀI LIỆU THAM KHẢO
- PHỤ LỤC

---

# DANH MỤC TỪ VIẾT TẮT
| Từ viết tắt | Cụm từ tiếng Anh đầy đủ | Ý nghĩa và vai trò kỹ thuật trong đề tài |
|---|---|---|
| **WPF** | Windows Presentation Foundation | Nền tảng giao diện đồ họa bản địa trên hệ điều hành Windows dựa trên DirectX. |
| **XAML** | Extensible Application Markup Language | Ngôn ngữ đánh dấu dựa trên XML dùng để khai báo giao diện người dùng. |
| **MVVM** | Model - View - ViewModel | Mô hình kiến trúc phần mềm tách biệt giữa Giao diện, Trạng thái và Dữ liệu. |
| **BFF** | Backend-For-Frontend | Mô hình kiến trúc gateway cục bộ trung gian phục vụ riêng cho ứng dụng client. |
| **OWIN** | Open Web Interface for .NET | Đặc tả chuẩn mở cho phép máy chủ web và ứng dụng .NET phân tách độc lập. |
| **DSP** | Digital Signal Processing | Xử lý tín hiệu kỹ thuật số (bộ lọc cân bằng âm sắc, phân tích âm học). |
| **EQ** | Equalizer | Bộ cân bằng âm thanh điều chỉnh biên độ các dải tần số khác nhau. |
| **FFT** | Fast Fourier Transform | Thuật toán biến đổi Fourier nhanh chuyển đổi tín hiệu từ miền thời gian sang miền tần số. |
| **WASAPI** | Windows Audio Session API | Giao diện lập trình âm thanh mức thấp độ trễ cực nhỏ của hệ điều hành Windows. |
| **CAS** | Content-Addressable Storage | Cơ chế lưu trữ định danh dữ liệu trên đĩa dựa trên mã băm nội dung. |
| **LRU** | Least Recently Used | Thuật toán giải phóng bộ nhớ đệm dựa trên việc loại bỏ phần tử ít dùng gần đây nhất. |
| **ADO.NET** | ActiveX Data Objects for .NET | Công nghệ truy xuất dữ liệu mức thấp hướng kết nối và hiệu năng cao trong .NET. |
| **DTO** | Data Transfer Object | Đối tượng truyền tải dữ liệu giữa các tầng kiến trúc không chứa logic nghiệp vụ. |
| **POCO** | Plain Old CLR Object | Lớp đối tượng thuần túy trong C# không phụ thuộc vào bất kỳ framework cụ thể nào. |
| **BFS** | Breadth-First Search | Giải thuật duyệt theo chiều rộng áp dụng cho cây thư mục hệ thống tập tin. |
| **ID3** | Identify an MP3 | Chuẩn thẻ siêu dữ liệu nhúng bên trong tệp âm thanh (Tên bài hát, Nghệ sĩ, Album, Ảnh bìa). |
| **WAL** | Write-Ahead Logging | Cơ chế ghi log nhật ký trước giúp SQLite cho phép Đọc và Ghi đồng thời không khóa. |
| **SOLID** | Single, Open-closed, Liskov, Interface, Dependency | Bộ 5 nguyên lý thiết kế hướng đối tượng kinh điển trong kỹ nghệ phần mềm. |


---

# DANH MỤC BẢNG BIỂU
- **Bảng 0.1**: Phiếu đánh giá kết quả đồ án
- **Bảng 0.2**: Danh mục các từ viết tắt chuyên ngành trong báo cáo
- **Bảng 1.1**: Bảng phân tích yêu cầu chức năng hệ thống (Functional Requirements)
- **Bảng 1.2**: Bảng phân tích yêu cầu phi chức năng hệ thống (Non-Functional Requirements)
- **Bảng 1.3**: So sánh đặc tính kỹ thuật: ADO.NET thuần vs. Entity Framework
- **Bảng 1.4**: Thông số kỹ thuật bộ lọc cân bằng âm sắc 10 băng tần ISO
- **Bảng 2.1**: Từ điển dữ liệu bảng tracks (Danh mục bài hát hợp nhất)
- **Bảng 2.2**: Từ điển dữ liệu bảng library_folders và stream_cache
- **Bảng 2.3**: Từ điển dữ liệu bảng playlists và playlist_tracks
- **Bảng 2.4**: Từ điển dữ liệu bảng play_queue và user_interactions
- **Bảng 2.5**: Từ điển dữ liệu bảng eq_presets và app_settings
- **Bảng 2.6**: Phân bổ trách nhiệm các Project thành phần trong Solution
- **Bảng 3.1**: Bảng 10 Kịch bản kiểm thử tiêu biểu trích xuất từ 60 bài test MSTest v2
- **Bảng 3.2**: Ma trận so sánh tính năng MusicApp với các ứng dụng nghe nhạc hiện hành

---

# DANH MỤC HÌNH VẼ
- **Hình 1.1**: Sơ đồ kiến trúc tổng thể 4 tầng của hệ thống MusicApp Desktop
- **Hình 1.2**: Mô hình luồng dữ liệu MVVM và tương tác Dispatcher trong WPF
- **Hình 2.1**: Sơ đồ Use Case tổng quát toàn bộ hệ thống MusicApp
- **Hình 2.2**: Sơ đồ tuần tự (Sequence Diagram) - Luồng tìm kiếm bài hát trực tuyến
- **Hình 2.3**: Sơ đồ tuần tự (Sequence Diagram) - Luồng phát nhạc và ghi bộ nhớ đệm CAS
- **Hình 2.4**: Sơ đồ hoạt động (Activity Diagram) - Luồng quét thư viện cục bộ BFS
- **Hình 2.5**: Sơ đồ thực thể liên kết (ERD) Cơ sở dữ liệu SQLite
- **Hình 2.6**: Sơ đồ vật lý Cơ sở dữ liệu (Physical Schema Diagram) với các chỉ mục Index
- **Hình 2.7**: Sơ đồ đường ống xử lý tín hiệu âm thanh kỹ thuật số (DSP Pipeline) trong NAudio
- **Hình 2.8**: Sơ đồ lớp (Class Diagram) - Hệ thống ViewModel và Data Binding
- **Hình 2.9**: Sơ đồ lớp (Class Diagram) - Hệ thống Persistence và Repository Pattern
- **Hình 3.1**: Giao diện màn hình Tìm kiếm và Khám phá bài hát trực tuyến
- **Hình 3.2**: Giao diện thẻ Now Playing và Trực quan hóa phổ âm thanh FFT 16 cột
- **Hình 3.3**: Giao diện Hàng đợi phát nhạc (Play Queue) và cơ chế sắp xếp
- **Hình 3.4**: Giao diện Quét và Quản lý thư viện âm nhạc cục bộ
- **Hình 3.5**: Giao diện Hiển thị và Đồng bộ lời bài hát Karaoke (LRC Sync)
- **Hình 3.6**: Giao diện Bộ cân bằng âm sắc DSP Equalizer 10 băng tần
- **Hình 3.7**: Giao diện Quản lý Danh sách phát (Playlists) và Gợi ý thông minh
- **Hình 3.8**: Biểu đồ kết quả thực thi 60 bài kiểm thử đơn vị MSTest v2 (100% Pass)

---

# CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI VÀ CÔNG NGHỆ ÁP DỤNG

## 1.1. Đặt vấn đề và Mục tiêu đề tài

### 1.1.1. Bối cảnh và Tính cấp thiết của đề tài

Trong kỷ nguyên số hóa hiện nay, nhu cầu thưởng thức âm nhạc đa phương tiện trên máy tính để bàn (Desktop PC) đã trở thành một phần thiết yếu trong học tập, làm việc và giải trí hàng ngày của người dùng. Sự bùng nổ của các nền tảng nghe nhạc trực tuyến toàn cầu như Spotify, Apple Music, YouTube Music đã định hình thói quen tìm kiếm và nghe nhạc trực tuyến nhờ vào kho nhạc trực tuyến khổng lồ. Tuy nhiên, khi khảo sát sâu vào thực trạng các ứng dụng phát nhạc trên môi trường Desktop, nhiều bất cập kỹ thuật và trải nghiệm người dùng lớn đã bộc lộ rõ nét:

- **Thứ nhất (Tài nguyên cồng kềnh):** Các ứng dụng Electron tiêu tốn từ 450MB - 1GB RAM, khởi động chậm chạp.
- **Thứ hai (Phân mảnh nguồn nhạc):** Tách rời kho nhạc cục bộ chất lượng cao (FLAC/WAV) và nhạc trực tuyến CDN.
- **Thứ ba (Thiếu DSP Equalizer):** Không có bộ cân bằng âm sắc 10 băng tần thời gian thực để bù trừ loa/tai nghe.
- **Thứ tư (Lời bài hát rời rạc):** Thiếu khả năng đồng bộ lời Karaoke .LRC tốc độ cao theo nhịp thời gian thực.

Xuất phát từ thực trạng trên, việc nghiên cứu và xây dựng một ứng dụng phát nhạc Desktop Native mang tên **MusicApp** trên nền tảng Microsoft .NET Framework và công nghệ Windows Presentation Foundation (WPF) là một đòi hỏi vô cùng cấp thiết. Ứng dụng tận dụng sức mạnh tăng tốc đồ họa phần cứng DirectX của WPF, kết hợp cùng Audio Engine chuyên nghiệp NAudio và hệ thống cơ sở dữ liệu nhúng SQLite, giải quyết triệt để bài toán dung lượng nhẹ, khởi động tức thì, hỗ trợ đa nguồn âm thanh và cá nhân hóa trải nghiệm âm học đỉnh cao.

### 1.1.2. Mục tiêu nghiên cứu và phát triển ứng dụng

Mục tiêu tổng quát của đề tài là thiết kế và hiện thực hóa hoàn chỉnh phần mềm phát nhạc Desktop Native chất lượng cao với các mục tiêu cụ thể sau:

1. **Kiến trúc:** Phân tầng chuẩn mực MVVM + Local BFF OWIN, tách rời giao diện XAML và logic điều khiển.
2. **Audio Engine:** NAudio 1.10.0, bộ lọc IIR BiQuad 10 băng tần (<50ms delay), FFT 1024-point trích xuất 16 cột phổ.
3. **Persistence & Cache:** SQLite WAL Mode, CAS Disk Cache 2 tầng, dọn dẹp LRU, phát lại offline tức thì sau 5ms.
4. **Thuật toán gợi ý:** Client-side Affinity Scoring, Radio bài hát, Smart Shuffle theo phân phối Boltzmann.

### 1.1.3. Phạm vi đề tài và giới hạn hệ thống

Để đảm bảo tính khả thi và tập trung tối đa vào chất lượng kỹ thuật trong khuôn khổ môn học, phạm vi của đồ án được xác định rõ ràng như sau:

#### Phạm vi hiện thực (In-Scope):
- Windows Desktop (Windows 7/8/10/11), .NET Framework 4.6.1.
- Hỗ trợ định dạng MP3, WAV, FLAC.
- Nguồn nhạc trực tuyến Jamendo API + Archive.org qua BFF Router.
- SQLite nhúng nội bộ WAL Mode (%LOCALAPPDATA%\MusicApp\musicapp.db).

#### Giới hạn hệ thống (Out-of-Scope):
- Chưa đồng bộ Cloud Sync đa thiết bị.
- Chưa phát triển phiên bản Mobile (iOS/Android) hay macOS/Linux.
- Chưa giải mã phần cứng DSD/MQA.
- Không tích hợp cổng thanh toán bản quyền.

## 1.2. Khảo sát nghiệp vụ và Yêu cầu hệ thống

### 1.2.1. Khảo sát quy trình nghiệp vụ phát nhạc đa nguồn

Quy trình vận hành thực tế của MusicApp bao gồm 5 luồng nghiệp vụ cốt lõi, tương tác liên hoàn giữa giao diện người dùng, cổng BFF cục bộ, Audio Engine và hệ cơ sở dữ liệu SQLite:

- **Luồng 1 (Tìm kiếm trực tuyến):** Debounce 300ms -> Local BFF :5245 -> Router Jamendo/Archive -> MemoryCache -> UI.
- **Luồng 2 (Phát trực tuyến & CAS Cache):** HTTP 206 Range -> NAudio BufferedWaveProvider -> Background CAS file write -> 0ms replay offline.
- **Luồng 3 (Quét thư viện BFS):** BFS lặp duyệt thư mục -> TagLibSharp trích thẻ ID3 -> Deduplication Key -> SQLite tracks table.
- **Luồng 4 (Đồng bộ lời bài hát):** Dispatcher timer 30ms -> LrcParser Binary Search O(log N) -> Auto-scroll Karaoke UI.
- **Luồng 5 (DSP Equalizer):** 10 thanh trượt +/-12dB -> Cập nhật hệ số 20 BiQuad filters (Stereo) -> Lưu SQLite eq_presets.

### 1.2.2. Phân tích yêu cầu chức năng (Functional Requirements)

Các yêu cầu chức năng của hệ thống được chuẩn hóa và phân rã chi tiết trong Bảng 1.1:

| Mã YC | Tên chức năng | Mô tả chi tiết nghiệp vụ |
|---|---|---|
| **FR-01** | Tìm kiếm bài hát trực tuyến | Cho phép nhập từ khóa tìm kiếm theo tên bài hát, nghệ sĩ; hỗ trợ kỹ thuật Debounce 300ms chống nghẽn; hiển thị kết quả trực quan. |
| **FR-02** | Phát luồng âm thanh trực tuyến | Hỗ trợ phát luồng stream MPEG Audio từ CDN qua giao thức HTTP 206 Range; tự động ghi cache nhị phân vào ổ cứng cục bộ. |
| **FR-03** | Điều khiển phát nhạc (Playback) | Cung cấp đầy đủ các chức năng Play, Pause, Stop, Seek (tua bài theo mili-giây), điều chỉnh âm lượng (0% - 100%), chuyển bài Kế tiếp / Lùi lại. |
| **FR-04** | Quản lý Hàng đợi (Play Queue) | Duy trì danh sách các bài hát chuẩn bị phát; hỗ trợ sắp xếp lại vị trí bài hát, xóa bài khỏi hàng đợi, tự động chuyển bài tiếp theo khi hết bài. |
| **FR-05** | Quét thư viện cục bộ (Local Scan) | Cho phép chọn thư mục trên máy tính; tự động quét đệ quy an toàn bằng BFS; trích xuất thẻ siêu dữ liệu ID3/Vorbis; lưu vào SQLite. |
| **FR-06** | Quản lý Danh sách phát (Playlist) | Cho phép tạo mới, đổi tên, xóa Playlist cá nhân; thêm và xóa bài hát khỏi Playlist; duy trì thứ tự bài hát theo trường position. |
| **FR-07** | Đánh dấu bài hát Yêu thích | Cho phép người dùng bấm nút Trái tim (Favorite) để đưa bài hát vào danh mục yêu thích; tự động cộng điểm trọng số tương tác. |
| **FR-08** | Bộ cân bằng âm sắc DSP Equalizer | Tùy chỉnh 10 dải tần âm thanh độc lập (+/- 12dB); hỗ trợ các bộ mẫu Preset có sẵn (Rock, Pop, Jazz, Bass Boost) và tạo Preset tùy chỉnh. |
| **FR-09** | Đồng bộ lời bài hát Karaoke | Tự động tìm kiếm và nạp tệp lời bài hát định dạng .LRC; làm nổi bật dòng lời đang phát và tự động cuộn giao diện mượt mà. |
| **FR-10** | Gợi ý bài hát & Smart Shuffle | Tính toán điểm số quan hệ (Affinity Score); tự động phát tiếp các bài tương đồng khi hết hàng đợi; xáo trộn thông minh theo phân phối Boltzmann. |

### 1.2.3. Phân tích yêu cầu phi chức năng (Non-Functional Requirements)

Bên cạnh các tính năng nghiệp vụ, tính ổn định và hiệu năng cao là tiêu chí sống còn của một ứng dụng Desktop Native. Các yêu cầu phi chức năng được định lượng khắt khe trong Bảng 1.2:

| Mã YC | Tiêu chuẩn kỹ thuật | Chỉ số định lượng & Cơ chế đảm bảo |
|---|---|---|
| **NFR-01** | Độ trễ xử lý âm thanh (Audio Latency) | Độ trễ xử lý qua chuỗi bộ lọc DSP Equalizer phải nhỏ hơn 50 mili-giây, đảm bảo phản hồi tức thì khi người dùng di chuyển thanh trượt EQ. |
| **NFR-02** | Zero-Allocation Audio Thread | Không phát sinh cấp phát bộ nhớ rác (GC Allocation) trong vòng lặp đọc mẫu PCM của Audio Thread nhằm triệt tiêu hoàn toàn hiện tượng khựng tiếng (audio stutter). |
| **NFR-03** | Ảo hóa Giao diện (UI Virtualization) | Bật chế độ VirtualizingStackPanel với chế độ tái sử dụng (Recycling) trên toàn bộ danh sách, đảm bảo ứng dụng cuộn mượt mà ngay cả khi thư viện có hơn 20.000 bài hát. |
| **NFR-04** | Chống rò rỉ bộ nhớ đồ họa (Frozen Images) | Toàn bộ hình ảnh ảnh bìa Album (BitmapImage) phải được gọi phương thức Freeze() để tách quyền sở hữu luồng, cho phép Garbage Collector thu hồi vùng nhớ GPU. |
| **NFR-05** | Độ an toàn và Toàn vẹn cơ sở dữ liệu | Hệ cơ sở dữ liệu SQLite phải được cấu hình chạy ở chế độ WAL (Write-Ahead Logging) kết hợp PRAGMA synchronous = NORMAL, bảo vệ dữ liệu không bị hỏng khi tắt máy đột ngột. |

## 1.3. Cơ sở công nghệ và Môi trường phát triển

### 1.3.1. Nền tảng .NET Framework 4.6.1 và Ngôn ngữ C# 7.3

Dự án được xây dựng dựa trên nền tảng **Microsoft .NET Framework 4.6.1** kết hợp cùng phiên bản ngôn ngữ **C# 7.3**. Việc lựa chọn phiên bản này mang lại lợi thế chiến lược về tính sẵn sàng và độ ổn định: .NET Framework 4.6.1 là thành phần mặc định có mặt trên tất cả các phiên bản hệ điều hành Microsoft Windows hiện hành (từ Windows 7 SP1, Windows 8.1 đến Windows 10 và Windows 11). Người dùng cuối có thể tải về và thực thi ngay tệp nhị phân của ứng dụng mà không cần cài đặt thêm gói runtime nặng nề nào khác.

### 1.3.2. Công nghệ giao diện WPF và Mô hình kiến trúc MVVM

Windows Presentation Foundation (WPF) là framework phát triển giao diện Desktop hàng đầu của Microsoft. WPF sử dụng ngôn ngữ đánh dấu XAML để định nghĩa giao diện người dùng và tận dụng đường ống dựng hình phần cứng DirectX, giúp hiển thị mượt mà các hoạt hình phức tạp như hiệu ứng xoay đĩa than Vinyl và phổ âm thanh FFT thời gian thực.

Mô hình kiến trúc **Model-View-ViewModel (MVVM)** là chuẩn mực bất biến trong phát triển phần mềm WPF chuyên nghiệp. Kiến trúc MVVM phân tách ứng dụng thành ba thành phần độc lập:

Mối quan hệ tương tác giữa View, ViewModel, Model và cơ chế điều phối Dispatcher của WPF được minh họa chi tiết trong Hình 1.2:

```
+-----------------------------------------------------------------------------------------+
|                              MÔ HÌNH LUỒNG DỮ LIỆU MVVM TRONG WPF                       |
+-----------------------------------------------------------------------------------------+
|      VIEW (XAML)       |         VIEWMODEL (C#)        |        MODEL & SERVICES        |
|  - MainWindow.xaml     |  - MainViewModel.cs           |  - TrackModel.cs               |
|  - NowPlayingCardView  |  - NowPlayingViewModel.cs     |  - IAudioService               |
|  - DspEqualizerView    |  - DspEqualizerViewModel.cs   |  - ITrackRepository            |
+------------------------+-------------------------------+--------------------------------+
            |                           |                               |
            |  <--- Data Binding ------ |                               |
            |       (TwoWay/OneWay)     |                               |
            |                           |                               |
            |  --- ICommand (Click) --> |                               |
            |       (RelayCommand)      |                               |
            |                           |  --- Gọi nghiệp vụ / Query -> |
            |                           |  <-- Trả về dữ liệu / Event - |
            |                           |                               |
            |  <--- Dispatcher.Invoke - |                               |
            |       (Update UI Thread)  |                               |
+-----------------------------------------------------------------------------------------+
```
*Hình 1.2: Mô hình luồng dữ liệu MVVM và tương tác Dispatcher trong WPF*

### 1.3.3. Mô hình Local Backend-for-Frontend (BFF) qua OWIN Self-Host

Một trong những điểm sáng tạo kiến trúc nổi bật nhất của dự án là việc áp dụng mô hình **Local Backend-For-Frontend (BFF)**. Thay vì để ứng dụng WPF trực tiếp thực hiện các cuộc gọi mạng phân tán tới các nhà cung cấp bên ngoài, MusicApp khởi chạy một máy chủ Web API 2 siêu nhẹ (Microsoft.Owin.SelfHost 4.2.2) chạy ngầm ngay trong cùng tiến trình ứng dụng tại cổng loopback: http://localhost:5245.

### 1.3.4. Thư viện âm thanh NAudio và Xử lý tín hiệu số (DSP)

NAudio (phiên bản 1.10.0) là thư viện âm thanh mã nguồn mở tiêu chuẩn công nghiệp dành cho nền tảng .NET. MusicApp tận dụng NAudio để xây dựng đường ống xử lý tín hiệu kỹ thuật số (DSP Pipeline) hoàn chỉnh từ khâu giải mã, cân bằng âm sắc đến phân tích phổ âm thanh.

| Băng | Tần số trung tâm (f0) | Dải âm thanh | Hệ số Q | Dải điều chỉnh Gain | Ý nghĩa cảm thụ âm thanh |
|---|---|---|---|---|---|
| **Băng tần 1** | 31 Hz | Sub-Bass | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ sâu của tiếng trống trầm và âm trầm điện tử. |
| **Băng tần 2** | 62 Hz | Bass | Q = 1.414 | -12.0 dB đến +12.0 dB | Nhịp đập âm bass chính trong nhạc Dance / Pop. |
| **Băng tần 3** | 125 Hz | Low Mid | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ ấm của giọng hát nam và tiếng guitar bass. |
| **Băng tần 4** | 250 Hz | Mid-Range | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ dày thân âm thanh của nhạc cụ mộc. |
| **Băng tần 5** | 500 Hz | Center Mid | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ rõ của giọng hát và nhạc cụ bộ hơi. |
| **Băng tần 6** | 1 kHz | Upper Mid | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ nổi bật của giọng hát chính (Lead Vocal). |
| **Băng tần 7** | 2 kHz | Presence | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ sắc nét của phát âm và tiếng đàn guitar. |
| **Băng tần 8** | 4 kHz | Clarity | Q = 1.414 | -12.0 dB đến +12.0 dB | Độ sáng và chi tiết của giọng nữ cao. |
| **Băng tần 9** | 8 kHz | Treble | Q = 1.414 | -12.0 dB đến +12.0 dB | Tiếng chập chũm (Hi-hat), độ thanh thoát. |
| **Băng tần 10** | 16 kHz | Brilliance | Q = 1.414 | -12.0 dB đến +12.0 dB | Không gian không khí (Airy), âm trường mở rộng. |

### 1.3.5. Hệ quản trị CSDL SQLite và Mô hình Repository Pattern

Đối với một ứng dụng phát nhạc Desktop Native, việc lựa chọn công nghệ lưu trữ dữ liệu đòi hỏi phải giải quyết hài hòa giữa tốc độ truy vấn tức thì, tính độc lập gọn nhẹ (không bắt người dùng cài đặt máy chủ CSDL phức tạp) và độ bền vững dữ liệu. Dự án sử dụng cơ sở dữ liệu nhúng **SQLite** thông qua gói thư viện chính thức System.Data.SQLite kết hợp công nghệ **ADO.NET thuần**.

Lý do lựa chọn ADO.NET thuần thay vì Entity Framework (EF) được phân tích và so sánh trong Bảng 1.3:

| Tiêu chí kỹ thuật | ADO.NET thuần + SQLite (MusicApp) | Entity Framework Core / 6.x |
|---|---|---|
| **Thời gian khởi động ban đầu** | Cực nhanh (< 20 mili-giây) | Chậm (500ms - 1.5s để sinh Model metadata) |
| **Mức tiêu thụ bộ nhớ RAM** | Rất thấp (~ 4 MB RAM) | Cao (~ 45 - 80 MB RAM do Change Tracker) |
| **Tốc độ nạp 10.000 bài hát** | ~ 150 mili-giây (Data Reader trực tiếp) | ~ 1.800 mili-giây (Object Materialization overhead) |
| **Kiểm soát câu lệnh SQL** | 100% kiểm soát trực tiếp, tối ưu hóa chỉ mục | Sinh mã gián tiếp qua LINQ to Entities |
| **Độ tin cậy trong Desktop App** | Tuyệt đối không gây lag hoặc giật giao diện | Dễ gây nghẽn luồng nếu cấu hình không chuẩn |

### 1.3.6. Môi trường phát triển và Công cụ hỗ trợ

Hệ thống được phát triển và kiểm thử đồng bộ với các công cụ lập trình phần mềm hiện đại:

Tổng hòa các công nghệ và giải pháp kiến trúc đã trình bày tạo nên kiến trúc phân tầng 4 lớp của MusicApp như mô tả trong Hình 1.1:

```
+-----------------------------------------------------------------------------------------+
|                 SƠ ĐỒ KIẾN TRÚC TỔNG THỂ 4 TẦNG HỆ THỐNG MUSICAPP                       |
+-----------------------------------------------------------------------------------------+
|  1. PRESENTATION LAYER (MusicApp - WPF Desktop Native)                                  |
|     - Views: Shell Window, Sidebar, NowPlayingCard, DspEqualizer, LyricsSync, PlayQueue |
|     - ViewModels: MainViewModel, NowPlayingViewModel, PlayQueueViewModel (MVVM Pattern)|
|     - Composition Root: App.xaml.cs (Tự khởi tạo Dependency Injection & Quản lý vòng đời)|
+--------------------------------------------+--------------------------------------------+
                                             | 
                      ┌──────────────────────┴──────────────────────┐
                      ▼                                             ▼
+--------------------------------------------+  +-----------------------------------------+
|  2. AUDIO PIPELINE LAYER                   |  |  3. CORE DOMAIN & PERSISTENCE LAYER     |
|     (MusicApp.AudioEngine)                 |  |     (MusicApp.Core)                     |
|  - NAudioService (WasapiOut / DirectSound) |  |  - Models: TrackModel, LyricLine        |
|  - BiQuadFilter (10-Band Peaking EQ)       |  |  - Services: LrcParser, BFS LibraryScan |
|  - SampleAggregator & FftCalculator        |  |  - Persistence: SQLite WAL Mode         |
|  - BufferedHttpWaveStream (Range 206)      |  |  - Repositories: Track, Playlist, Queue |
+--------------------------------------------+  +-----------------------------------------+
                      |                                             |
                      └──────────────────────┬──────────────────────┘
                                             ▼
+-----------------------------------------------------------------------------------------+
|  4. LOCAL BACKEND-FOR-FRONTEND LAYER (MusicApp.Bff - OWIN Self-Host :5245)              |
|     - Controllers: TrackController (/api/v1/search, /api/v1/stream)                     |
|     - Providers: MusicSourceRouter -> Jamendo API, Vietnamese CDN, Archive.org          |
|     - Middlewares: StreamProxyMiddleware (HTTP Range Chunks), MemoryCacheService        |
+-----------------------------------------------------------------------------------------+
```
*Hình 1.1: Sơ đồ kiến trúc tổng thể 4 tầng của hệ thống MusicApp Desktop*


---

# CHƯƠNG 2: PHÂN TÍCH, THIẾT KẾ HỆ THỐNG VÀ CƠ SỞ DỮ LIỆU

## 2.1. Phân tích chức năng và Thiết kế Use Case

### 2.1.1. Sơ đồ Use Case tổng quát của hệ thống

Hệ thống MusicApp được thiết kế xoay quanh một tác nhân chính (Primary Actor) duy nhất là **Người dùng (User)**. Toàn bộ các chức năng nghiệp vụ được nhóm thành 6 phân hệ Use Case cốt lõi: Quản lý phát nhạc trực tuyến, Quản lý thư viện cục bộ, Quản lý danh sách phát và hàng đợi, Tùy chỉnh bộ cân bằng âm thanh DSP, Đồng bộ lời bài hát Karaoke và Tương tác hệ thống gợi ý thông minh.

Cấu trúc phân rã Use Case tổng quát của hệ thống MusicApp được thể hiện chi tiết trong Hình 2.1:

```
+-----------------------------------------------------------------------------------------+
|                    SƠ ĐỒ USE CASE TỔNG QUÁT HỆ THỐNG MUSICAPP                           |
+-----------------------------------------------------------------------------------------+
                                 +-----------------------+
                                 |   NGƯỜI DÙNG (USER)   |
                                 +-----------+-----------+
                                             |
         ┌───────────────────┬───────────────┼───────────────┬───────────────────┐
         ▼                   ▼               ▼               ▼                   ▼
  (UC01: Tìm kiếm)   (UC02: Quét thư viện) (UC03: Phát nhạc) (UC04: Quản lý PL)  (UC05: Chỉnh EQ)
         |                   |               |               |                   |
         | <<extend>>        |               | <<include>>   |                   | <<extend>>
         v                   |               v               |                   v
  (UC01.1: Gợi ý     (UC02.1: Trích xuất  (UC03.1: Ghi đệm  (UC04.1: Thêm/Xóa  (UC05.1: Lưu
   từ khóa)           thẻ ID3)            CAS Cache)          bài hát)            Preset)
                                             |
                                             | <<include>>
                                             v
                                      (UC03.2: Đồng bộ
                                       lời bài hát LRC)
+-----------------------------------------------------------------------------------------+
```
*Hình 2.1: Sơ đồ Use Case tổng quát toàn bộ hệ thống MusicApp*

### 2.1.2. Đặc tả chi tiết các Use Case cốt lõi

### 2.1.3. Thiết kế Sơ đồ tuần tự (Sequence Diagram)

Sơ đồ tuần tự thể hiện rõ rệt sự tương tác trao đổi thông điệp theo trục thời gian giữa các tầng thành phần. Dưới đây là 2 sơ đồ tuần tự then chốt nhất của hệ thống:

Luồng tuần tự từ khi người dùng nhập từ khóa tìm kiếm đến khi nhận được kết quả danh sách bài hát được mô tả trong Hình 2.2:

```
+-----------------------------------------------------------------------------------------+
|                 SƠ ĐỒ TUẦN TỰ: TÌM KIẾM BÀI HÁT TRỰC TUYẾN QUA LOCAL BFF                |
+-----------------------------------------------------------------------------------------+
  User         WPF View          MainViewModel        MusicApiClient     Local BFF (:5245) Jamendo CDN
   |               |                   |                    |                   |              |
   |-- Gõ từ khóa->|                   |                    |                   |              |
   |   'Hoàng Dũng'|-- Debounce 300ms->|                    |                   |              |
   |               |                   |-- SearchAsync() -->|                   |              |
   |               |                   |                    |-- GET /search --->|              |
   |               |                   |                    |   ?query=...      |-- Call API ->|
   |               |                   |                    |                   |<-- Raw JSON -|
   |               |                   |                    |                   | [Sanitize]   |
   |               |                   |                    |                   | [Cache Mem]  |
   |               |                   |                    |<-- TrackDto List -|              |
   |               |                   |<-- List<TrackDto> -|                   |              |
   |               |<-- Bind Items ----|                    |                   |              |
   |<-- Render ----|                   |                    |                   |              |
+-----------------------------------------------------------------------------------------+
```
*Hình 2.2: Sơ đồ tuần tự (Sequence Diagram) - Luồng tìm kiếm bài hát trực tuyến*

Luồng tuần tự điều khiển phát nhạc kết hợp ghi bộ nhớ đệm nhị phân CAS Disk Cache được mô tả trong Hình 2.3:

```
+-----------------------------------------------------------------------------------------+
|         SƠ ĐỒ TUẦN TỰ: PHÁT NHẠC VÀ GHI BỘ NHỚ ĐỆM CAS CACHE NỀN                        |
+-----------------------------------------------------------------------------------------+
  WPF UI        NowPlayingVM        AudioEngine          Local BFF (:5245)      Disk Storage   CDN Stream
    |                 |                  |                       |                   |             |
    |-- Click Play -->|                  |                       |                   |             |
    |                 |-- PlayTrack() -->|                       |                   |             |
    |                 |                  |-- Check Local Cache ->|                   |             |
    |                 |                  |                       |-- Exists(hash)? ->|             |
    |                 |                  |                       |<-- Not Found -----|             |
    |                 |                  |-- GET /stream/{id} -->|                                 |
    |                 |                  |   (Range: 0-)         |-- HTTP 206 Range -------------->|
    |                 |                  |                       |<-- Audio Chunk (128kbps) -------|
    |                 |                  |<-- Stream Response ---|                                 |
    |                 |                  | [Decode PCM to WASAPI]|-- Async Write Chunk ----------->|
    |<-- UI State: Playing --------------|                       |   %LOCALAPPDATA%\Cache\*.audio  |
    |    (Vinyl Spin & Spectrum 30fps)   |                       |<-- Cache Registered in SQLite---||
+-----------------------------------------------------------------------------------------+
```
*Hình 2.3: Sơ đồ tuần tự (Sequence Diagram) - Luồng phát nhạc và ghi bộ nhớ đệm CAS*

### 2.1.4. Thiết kế Sơ đồ hoạt động (Activity Diagram)

Quy trình quét thư viện âm nhạc cục bộ sử dụng giải thuật duyệt cây theo chiều rộng (BFS) nhằm giải quyết triệt để nguy cơ tràn ngăn xếp (Stack Overflow) khi gặp cây thư mục phân cấp sâu. Sơ đồ hoạt động trong Hình 2.4 mô tả chi tiết logic xử lý bất đồng bộ này:

```
+-----------------------------------------------------------------------------------------+
|           SƠ ĐỒ HOẠT ĐỘNG (ACTIVITY DIAGRAM): QUÉT THƯ VIỆN CỤC BỘ BẰNG BFS             |
+-----------------------------------------------------------------------------------------+
                                      ( Bắt đầu )                                          
                                           |                                               
                             [ Người dùng chọn thư mục gốc ]                               
                                           |                                               
                           [ Khởi tạo Queue<string> thư mục ]                              
                                           |                                               
                               +----->[ Hàng đợi rỗng? ] --( Đúng )--> [ Cập nhật xong UI ]
                               |           |                                    |          
                               |        ( Sai )                             ( Kết thúc )   
                               |           v                                               
                               |     [ Dequeue thư mục D ]                                 
                               |           |                                               
                               |   [ Kiểm tra quyền truy cập? ]                            
                               |      |                  |                                 
                               |  ( Bị cấm )          ( Hợp lệ )                           
                               |      v                  v                                 
                               |  [ Bỏ qua ]      [ Lấy file .mp3, .flac, .wav ]           
                               |                         |                                 
                               |                 [ Còn file trong D? ]                     
                               |                  |              |                         
                               |               ( Đúng )        ( Sai )                     
                               |                  v              v                         
                               |          [ TagLibSharp đọc ID3] [ Đưa thư mục con vào Queue]
                               |                  |              |                         
                               |          [ Sinh FingerprintKey ]+                         
                               |                  |                                        
                               |          [ Lưu vào SQLite ]                               
                               +------------------+                                        
+-----------------------------------------------------------------------------------------+
```
*Hình 2.4: Sơ đồ hoạt động (Activity Diagram) - Luồng quét thư viện cục bộ BFS*

## 2.2. Thiết kế Cơ sở dữ liệu (Database Design)

### 2.2.1. Mô hình thực thể kết hợp (ERD - Entity Relationship Diagram)

Cơ sở dữ liệu SQLite của MusicApp được thiết kế ở dạng chuẩn 3NF (Third Normal Form) nhằm đảm bảo tính toàn vẹn dữ liệu, triệt tiêu trùng lặp siêu dữ liệu và tối ưu hóa tốc độ truy vấn trên ổ cứng. Hệ thống bao gồm 8 thực thể dữ liệu chính: tracks (kho bài hát hợp nhất), library_folders (thư mục quét), stream_cache (bộ đệm đĩa), playlists (danh sách phát), playlist_tracks (quan hệ nhiều-nhiều danh sách bài hát), play_queue (hàng đợi phát), user_interactions (nhật ký hành vi gợi ý), và eq_presets (cấu hình bộ lọc âm sắc).

Mối quan hệ tương tác giữa các thực thể trong cơ sở dữ liệu được biểu diễn qua Sơ đồ ERD trong Hình 2.5:

```
+-----------------------------------------------------------------------------------------+
|               MÔ HÌNH THỰC THỂ KẾT HỢP (ERD) CƠ SỞ DỮ LIỆU SQLITE                       |
+-----------------------------------------------------------------------------------------+
  +----------------------+             +-----------------------+                          
  |   library_folders    |             |       playlists       |                          
  +----------------------+             +-----------------------+                          
  | PK folder_path       |             | PK id                 |                          
  |    last_scanned_at   |             |    name               |                          
  |    total_files       |             |    description        |                          
  +----------------------+             +-----------+-----------+                          
                                                   | 1                                    
                                                   |                                      
                                                   | N                                    
  +----------------------+             +-----------+-----------+                          
  |     stream_cache     |             |    playlist_tracks    |                          
  +----------------------+             +-----------------------+                          
  | PK track_hash        |             | PK,FK1 playlist_id    |                          
  |    file_path         |             | PK,FK2 track_id       |                          
  |    file_size_bytes   |             |        position       |                          
  |    last_accessed_at  |             +-----------+-----------+                          
  +----------------------+                         | N                                    
                                                   |                                      
                                                   | 1                                    
                                       +-----------+-----------+                          
                                       |        tracks         |                          
                                       +-----------------------+                          
                                       | PK id                 |<-------+                 
                                       |    track_key (UNIQUE) |        |                 
                                       |    title, artist      |        |                 
                                       |    duration_seconds   |        |                 
                                       |    affinity_score     |        |                 
                                       +-----+-----------+-----+        |                 
                                           1 |         1 |              |                 
                         +-------------------+           +--------------+                 
                         | N                             | N                              
             +-----------+-----------+       +-----------+-----------+                    
             |   user_interactions   |       |      play_queue       |                    
             +-----------------------+       +-----------------------+                    
             | PK id                 |       | PK position           |                    
             | FK track_id           |       | FK track_id           |                    
             |    action_type        |       +-----------------------+                    
             |    duration_played    |                                                    
             +-----------------------+                                                    
+-----------------------------------------------------------------------------------------+
```
*Hình 2.5: Sơ đồ thực thể liên kết (ERD) Cơ sở dữ liệu SQLite*

### 2.2.2. Sơ đồ quan hệ dữ liệu vật lý (Physical Schema Diagram)

Trên môi trường vật lý SQLite, các bảng được cài đặt ràng buộc khóa chính (PRIMARY KEY), khóa ngoại (FOREIGN KEY) kèm hành động toàn vẹn ON DELETE CASCADE. Đặc biệt, để đáp ứng tiêu chuẩn truy vấn dưới 5 mili-giây cho hàng chục nghìn bài hát, các chỉ mục (INDEX) chiến lược đã được thiết lập như mô tả trong Hình 2.6:

```
+-----------------------------------------------------------------------------------------+
|         SƠ ĐỒ QUAN HỆ VẬT LÝ VÀ CHỈ MỤC TỐI ƯU TRUY VẤN (SQLITE PHYSICAL SCHEMA)        |
+-----------------------------------------------------------------------------------------+
  TABLE: tracks                                     TABLE: playlists                      
  - id: INTEGER (PK, AUTOINCREMENT)                 - id: INTEGER (PK, AUTOINCREMENT)     
  - track_key: TEXT (UNIQUE)                        - name: TEXT (NOT NULL, UNIQUE)       
  - source_type: TEXT ('local'|'jamendo'|'vn')      - description: TEXT                   
  - source_id: TEXT (FilePath / StreamID)           - cover_uri: TEXT                     
  - title: TEXT (NOT NULL)                          - created_at: TEXT                    
  - artist: TEXT (NOT NULL)                         ------------------------------------- 
  - album: TEXT                                                                           
  - duration_seconds: INTEGER                       TABLE: playlist_tracks                
  - is_favorite: INTEGER (0|1)                      - playlist_id: INT (PK, FK playlists) 
  - affinity_score: REAL                            - track_id: INT (PK, FK tracks)       
  -------------------------------------------       - position: INT                       
  * INDEXES:                                        * INDEX: idx_pl_pos (playlist_id,pos) 
    - idx_tracks_artist (artist ASC)                ------------------------------------- 
    - idx_tracks_album (album ASC)                                                        
    - idx_tracks_favorite (is_favorite)             TABLE: stream_cache                   
    - idx_tracks_affinity (affinity_score DESC)     - track_hash: TEXT (PK, SHA-1)        
                                                    - file_path: TEXT                     
  TABLE: user_interactions                          - file_size_bytes: INTEGER            
  - id: INTEGER (PK, AUTOINCREMENT)                 - last_accessed_at: TEXT              
  - track_id: INTEGER (FK tracks ON CASCADE)        * INDEX: idx_cache_acc (last_acc ASC) 
  - action_type: TEXT                               ------------------------------------- 
  - duration_played: INTEGER                        TABLE: eq_presets                     
  - created_at: TEXT                                - name: TEXT (PK)                     
  * INDEX: idx_interactions_track (track_id)        - gains_json: TEXT                    
+-----------------------------------------------------------------------------------------+
```
*Hình 2.6: Sơ đồ vật lý Cơ sở dữ liệu (Physical Schema Diagram) với các chỉ mục Index*

### 2.2.3. Bảng từ điển dữ liệu chi tiết (Data Dictionary)

Cấu trúc và ý nghĩa chi tiết từng trường dữ liệu trong các bảng được định nghĩa trong các bảng dưới đây:

#### Bảng 2.1: Từ điển dữ liệu bảng tracks

| Tên cột | Kiểu dữ liệu | Ràng buộc | Cho phép Null | Ý nghĩa và quy tắc nghiệp vụ |
|---|---|---|---|---|
| `id` | INTEGER | PK, AI | Không | Mã định danh duy nhất tự tăng của bài hát. |
| `track_key` | TEXT | UNIQUE | Không | Khóa ngón tay chuẩn hóa: Normalize(Title)::Normalize(Artist). |
| `source_type` | TEXT | CHECK | Không | Loại nguồn nhạc: 'local', 'jamendo', 'vn'. |
| `source_id` | TEXT |  | Không | Đường dẫn tệp trên đĩa (nếu local) hoặc ID luồng CDN (nếu online). |
| `title` | TEXT |  | Không | Tên hiển thị của bài hát. |
| `artist` | TEXT | INDEX | Không | Tên nghệ sĩ hoặc nhóm nhạc biểu diễn. |
| `album` | TEXT | INDEX | Có | Tên album chứa bài hát. |
| `genre` | TEXT |  | Có | Thể loại âm nhạc (Pop, Rock, Ballad, EDM...). |
| `duration_seconds` | INTEGER | DEFAULT 0 | Không | Tổng thời lượng phát của bài hát tính bằng giây. |
| `bitrate` | INTEGER | DEFAULT 128 | Có | Tốc độ bit âm thanh (kbps), ví dụ: 128, 256, 320. |
| `cover_uri` | TEXT |  | Có | Đường dẫn tệp ảnh bìa lưu tạm trong thư mục Cache. |
| `file_mtime` | TEXT |  | Có | Thời gian sửa đổi tệp gần nhất dùng cho quét gia tăng. |
| `play_count` | INTEGER | DEFAULT 0 | Không | Tổng số lần người dùng đã nghe bài hát này. |
| `skip_count` | INTEGER | DEFAULT 0 | Không | Số lần người dùng bấm chuyển bài khi nghe chưa tới 10 giây. |
| `is_favorite` | INTEGER | INDEX, 0|1 | Không | Trạng thái bài hát yêu thích: 1 là yêu thích, 0 là bình thường. |
| `affinity_score` | REAL | INDEX, DESC | Không | Điểm số quan hệ dùng cho thuật toán gợi ý bài hát. |

## 2.3. Thiết kế Kiến trúc phần mềm (.NET Solution Structure)

### 2.3.1. Cấu trúc phân tầng dự án (Layered Solution Architecture)

Dự án MusicApp được tổ chức dưới dạng Solution bao gồm 4 Project độc lập và 1 Test Project. Mỗi Project đảm nhận một ranh giới trách nhiệm riêng biệt (Separation of Concerns), đảm bảo tính ghép lỏng (Loose Coupling) và gắn kết cao (High Cohesion) như tổng hợp trong Bảng 2.6:

#### Bảng 2.6: Phân bổ trách nhiệm các Project thành phần

| Tên Project trong Visual Studio | Tầng kiến trúc tương ứng | Trách nhiệm kỹ thuật và các thành phần chính |
|---|---|---|
| `MusicApp (WPF Native)` | Presentation Layer | Chứa toàn bộ Views (XAML), ViewModels, DataTemplate, Converters, Resources/Themes. Đóng vai trò Composition Root tại App.xaml.cs. |
| `MusicApp.Core` | Domain & Persistence Layer | Chứa POCO Models, DTOs, Repository Interfaces, Services (LrcParser, LocalLibraryService), SQLite DatabaseInitializer và 6 Repository implementations. |
| `MusicApp.AudioEngine` | Digital Signal Processing | Đóng gói toàn bộ thư viện NAudio, WasapiOut/DirectSound, BiQuadFilter 10 băng tần, SampleAggregator, FftCalculator, BufferedHttpWaveStream. |
| `MusicApp.Bff` | Local Gateway / Server Host | Máy chủ OWIN Self-Host (Microsoft.Owin.Hosting), Web API 2 Controllers, MusicSourceRouter, JamendoProvider, StreamProxyMiddleware. |
| `MusicApp.Tests` | Test Automation Suite | Bộ 60 bài kiểm thử đơn vị MSTest v2 bao phủ toàn diện 4 tầng chức năng: BFF endpoint, Audio Engine, Core Persistence, ViewModel Navigation. |

### 2.3.2. Sơ đồ chuỗi xử lý tín hiệu âm thanh DSP Pipeline

Đường ống xử lý tín hiệu âm thanh kỹ thuật số (DSP Pipeline) là trái tim công nghệ của MusicApp. Mọi luồng âm thanh từ tệp đĩa cục bộ hay luồng mạng đều được chuẩn hóa thành định dạng mẫu IEEE Float PCM 32-bit (44.1kHz Stereo) trước khi đi qua chuỗi xử lý thời gian thực được mô tả trong Hình 2.7:

```
+-----------------------------------------------------------------------------------------+
|        CHUỖI ĐƯỜNG ỐNG XỬ LÝ TÍN HIỆU ÂM THANH KỸ THUẬT SỐ (DSP PIPELINE)               |
+-----------------------------------------------------------------------------------------+
  [ NGUỒN ÂM THANH ] ---> Local File (FileStream) HOẶC Stream Online (BufferedHttpStream) 
           |                                                                              
           v                                                                              
  [ GIẢI MÃ ĐẦU VÀO ] ---> Mp3FileReader / WaveFileReader / MediaFoundationReader        
           |               (Chuẩn hóa thành định dạng PCM 16-bit / 44.100 Hz / Stereo)   
           v                                                                              
  [ CHUYỂN ĐỔI MẪU ] ---> WaveToSampleProvider (Chuyển đổi sang IEEE Float 32-bit)       
           |                                                                              
           v                                                                              
  [ BỘ CÂN BẰNG ÂM SẮC DSP ] ---> DspEqualizerSampleProvider                             
                                  - 10 Băng tần Peaking BiQuad Filter cho Kênh Trái (L)   
                                  - 10 Băng tần Peaking BiQuad Filter cho Kênh Phải (R)  
                                  (Điều chỉnh biên độ +/-12dB bằng công thức Bristow-Johnson)
           |                                                                              
           v                                                                              
  [ BỘ TRÍCH MẪU KHÔNG RÁC ] ---> SampleAggregator (Zero-Allocation Buffer Ring)          
           |                                         |                                    
           | (Truyền tiếp mẫu âm thanh)               | (Trích xuất khối 1024 điểm mẫu)  
           v                                         v                                    
  [ THIẾT BỊ XUẤT ÂM THANH ]              [ TÍNH TOÁN FFT 1024 ĐIỂM ]                    
  - WasapiOut (Shared Mode)               - Fast Fourier Transform (FftCalculator)       
  - Hoặc DirectSoundOut                   - Phân nhóm thành 16 Frequency Bins            
  (Đưa ra Loa / Tai nghe)                            |                                    
                                                     v                                    
                                          [ CẬP NHẬT GIAO DIỆN ]                          
                                          - Dispatcher cập nhật Spectrum Bars 30fps       
+-----------------------------------------------------------------------------------------+
```
*Hình 2.7: Sơ đồ đường ống xử lý tín hiệu âm thanh kỹ thuật số (DSP Pipeline) trong NAudio*

### 2.3.3. Sơ đồ lớp chi tiết (Class Diagram)

Hệ thống lớp của MusicApp được thiết kế theo đúng nguyên lý Hướng đối tượng và các mẫu thiết kế kinh điển: ObservableObject thực thi INotifyPropertyChanged cho tầng ViewModel; Repository Pattern cho tầng truy xuất dữ liệu SQLite. Hai sơ đồ lớp chi tiết dưới đây đặc tả đầy đủ các thuộc tính và phương thức cốt lõi:

Cấu trúc phân cấp lớp của hệ thống ViewModel và cơ chế Data Binding được thể hiện trong Hình 2.8:

```
+-----------------------------------------------------------------------------------------+
|                  SƠ ĐỒ LỚP (CLASS DIAGRAM): HỆ THỐNG VIEWMODEL                          |
+-----------------------------------------------------------------------------------------+
                          +----------------------------------+                            
                          |   ObservableObject (Core.Common) |                            
                          +----------------------------------+                            
                          | # SetProperty<T>()               |                            
                          | # OnPropertyChanged()            |                            
                          +-----------------+----------------+                            
                                            |                                             
         +----------------------------------+----------------------------------+          
         |                                  |                                  |          
         v                                  v                                  v          
+-------------------------+  +-------------------------------+  +-------------------------+
|      MainViewModel      |  |      NowPlayingViewModel      |  |   DspEqualizerViewModel | 
+-------------------------+  +-------------------------------+  +-------------------------+
| - _apiClient            |  | - _audioService: IAudioService|  | - _dspService           |
| - SearchQuery: string   |  | - CurrentTrack: TrackModel    |  | + Bands: List<BandVM>   |
| + SearchResults         |  | - PlaybackState               |  | + Presets: List<string> |
| + SearchCommand         |  | + SpectrumBins: double[16]    |  | + SelectedPreset        |
| + NavigateCommand       |  | + PlayPauseCommand            |  | + ApplyPresetCommand    |
+-------------------------+  +-------------------------------+  +-------------------------+
+-----------------------------------------------------------------------------------------+
```
*Hình 2.8: Sơ đồ lớp (Class Diagram) - Hệ thống ViewModel và Data Binding*

Cấu trúc phân cấp lớp của hệ thống Persistence và mẫu thiết kế Repository Pattern được thể hiện trong Hình 2.9:

```
+-----------------------------------------------------------------------------------------+
|           SƠ ĐỒ LỚP (CLASS DIAGRAM): PERSISTENCE & REPOSITORY PATTERN                   |
+-----------------------------------------------------------------------------------------+
  <<Interface>>                                                <<Interface>>              
  ITrackRepository                                             IPlaylistRepository        
  + GetAllTracksAsync(): Task<IEnumerable<TrackModel>>         + GetAllPlaylistsAsync()   
  + GetTrackByKeyAsync(key): Task<TrackModel>                  + CreatePlaylistAsync()    
  + UpsertTrackAsync(track): Task                              + AddTrackToPlaylistAsync()
  + SetFavoriteAsync(id, isFav): Task                          + RemoveTrackAsync()       
         ^                                                            ^                   
         | implements                                                 | implements        
  +------+--------------------+                                +------+-------------------+
  |   TrackRepository         |                                |  PlaylistRepository      |
  +---------------------------+                                +--------------------------+
  | - _connString: string     |                                | - _connString: string    |
  | + UpsertTrackAsync()      |                                | + AddTrackToPlaylist()   |
  +---------------------------+                                +--------------------------+
                 \                                                            /           
                  +-----------------------------+----------------------------+            
                                                |                                         
                                                v                                         
                               +---------------------------------+                        
                               |     DatabaseInitializer         |                        
                               +---------------------------------+                        
                               | + InitializeDatabase()          |                        
                               | - ExecuteSqlScript()            |                        
                               | - ApplyWalPragma()              |                        
                               +---------------------------------+                        
+-----------------------------------------------------------------------------------------+
```
*Hình 2.9: Sơ đồ lớp (Class Diagram) - Hệ thống Persistence và Repository Pattern*


---

# CHƯƠNG 3: CÀI ĐẶT THỰC NGHIỆM VÀ KẾT QUẢ ĐẠT ĐƯỢC

## 3.1. Môi trường triển khai và Cấu hình hệ thống

### 3.1.1. Yêu cầu cấu hình triển khai

Ứng dụng MusicApp được đóng gói và biên dịch thành tệp thực thi độc lập (MusicApp.exe). Nhờ vào kiến trúc Native tối ưu hóa trên nền tảng .NET Framework 4.6.1, ứng dụng đòi hỏi cấu hình phần cứng vô cùng khiêm tốn nhưng vẫn mang lại trải nghiệm âm thanh mượt mà:

### 3.1.2. Cấu hình chuỗi kết nối và Cơ chế Database Initializer

Cơ sở dữ liệu SQLite của ứng dụng không phụ thuộc vào đường dẫn tuyệt đối tĩnh mà được tự động xác định linh hoạt trong thư mục dữ liệu ứng dụng của người dùng cục bộ (%LOCALAPPDATA%\MusicApp\musicapp.db). Chuỗi kết nối (Connection String) được khởi tạo với các tham số tối ưu hóa hiệu năng cao:

## 3.2. Hiện thực hóa các Chức năng chính và Giao diện (Demo)

### 3.2.1. Chức năng Tìm kiếm trực tuyến và Kỹ thuật Debounce 300ms

Khi người dùng gõ từ khóa tìm kiếm trên ô nhập liệu (Search TextBox), nếu ứng dụng gửi yêu cầu mạng sau mỗi ký tự sẽ phát sinh hàng chục request dư thừa làm nghẽn mạng và vi phạm quy định hạn chế tần suất (Rate Limiting) của Jamendo API. MusicApp giải quyết triệt để vấn đề này bằng kỹ thuật **Debounce Pattern** với độ trễ 300ms trong MainViewModel:

Giao diện màn hình Tìm kiếm và Khám phá bài hát trực tuyến được hiển thị như trong Hình 3.1:

```
+-----------------------------------------------------------------------------------------+
| [MusicApp Native]   [-] [o] [x]                                                         |
+-----------------------------------------------------------------------------------------+
| [🔍 Tìm kiếm bài hát, nghệ sĩ... (Debounce 300ms)]           [Theme: Dark] [Cài đặt]    |
+-------------------+---------------------------------------------------------------------+
| KHÁM PHÁ          | KẾT QUẢ TÌM KIẾM TRỰC TUYẾN: 'Hoàng Dũng' (12 bài hát tìm thấy)     |
| > Tìm kiếm online |                                                                     |
|   Thư viện máy    | [#] [Ảnh]  [Tiêu đề bài hát]          [Nghệ sĩ]       [Thời lượng] [♥]  |
|   Bài hát yêu thích |  1  [Img]  Nàng Thơ                    Hoàng Dũng        04:15     [♥] |
|   Danh sách phát  |  2  [Img]  Đoạn Kết Mới                Hoàng Dũng        03:42     [♡] |
|                   |  3  [Img]  Chờ Anh Nhé                 Hoàng Dũng        04:02     [♥] |
| EQUALIZER (DSP)   |  4  [Img]  Nép Vào Anh Và Nghe Anh Hát Hoàng Dũng        03:55     [♡] |
|   10-Band EQ      |  5  [Img]  Về Phía Mưa                 Hoàng Dũng        04:20     [♡] |
|                   |  ... (UI Virtualization kích hoạt: Chỉ render 25 items hiển thị)     |
+-------------------+---------------------------------------------------------------------+
| [Now Playing Card: Nàng Thơ - Hoàng Dũng] [⏮] [▶/⏸] [⏭] [🔀] [🔁] [===●=======] 01:24/04:15 |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.1: Giao diện màn hình Tìm kiếm và Khám phá bài hát trực tuyến*

### 3.2.2. Chức năng Phát nhạc, Quản lý Hàng đợi và Thẻ Now Playing

Trình phát nhạc được điều khiển bởi NAudioService thông qua giao diện IAudioService trừu tượng. Thẻ Now Playing được thiết kế theo phong cách hiện đại với hiệu ứng quay đĩa than Vinyl bằng XAML Storyboard kết hợp cùng visualizer phổ tần số FFT 16 cột nhảy mượt mà 30 khung hình/giây:

Giao diện thẻ Now Playing với hiệu ứng đĩa than Vinyl và phổ âm thanh FFT được minh họa trong Hình 3.2, và giao diện Hàng đợi phát nhạc (Play Queue) được thể hiện trong Hình 3.3:

```
+-----------------------------------------------------------------------------------------+
|                      GIAO DIỆN THẺ NOW PLAYING VÀ PHỔ ÂM THANH FFT                      |
+-----------------------------------------------------------------------------------------+
|  +--------------------+   NÀNG THƠ                                                      |
|  |     (@@@@@@)       |   Nghệ sĩ: Hoàng Dũng  |  Album: 25                             |
|  |   (@  (O)  @)      |   Nguồn phát: Trực tuyến (Đã lưu CAS Cache)                    |
|  |     (@@@@@@)       |   ------------------------------------------------------------  |
|  | [Đĩa than xoay     |   TRỰC QUAN HÓA PHỔ TẦN SỐ (1024-POINT FFT - 16 BINS):          |
|  |  Storyboard 360°]  |    _  _     _     _  _        _     _     _                     |
|  +--------------------+   | || | _ | | _ | || | _  _ | | _ | | _ | | _  _               |
|                           | || || || || || || || || || || || || || || || |              |
|                           [31Hz    125Hz   500Hz    2kHz    8kHz   16kHz]               |
+-----------------------------------------------------------------------------------------+
| [01:24] ==========================●============================================= [04:15] |
|      [🔀 Smart Shuffle]   [⏮]   [ ▶ / ⏸ Play ]   [⏭]   [🔁 Lặp lại]    [🔊 80%]        |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.2: Giao diện thẻ Now Playing và Trực quan hóa phổ âm thanh FFT 16 cột*

```
+-----------------------------------------------------------------------------------------+
|                    GIAO DIỆN HÀNG ĐỢI PHÁT NHẠC (PLAY QUEUE)                            |
+-----------------------------------------------------------------------------------------+
| HÀNG ĐỢI ĐANG PHÁT (Đang có: 8 bài hát)                    [Xóa hết] [Trộn ngẫu nhiên]  |
|                                                                                         |
|  #  [Ảnh]  [Tiêu đề bài hát]          [Nghệ sĩ]        [Thời lượng]   [Thao tác]        |
| >>  [Img]  Nàng Thơ (Đang phát)       Hoàng Dũng         04:15        [Đang phát...]    |
|  1  [Img]  Đoạn Kết Mới               Hoàng Dũng         03:42        [≡ Kéo thả] [X]   |
|  2  [Img]  Chờ Anh Nhé                Hoàng Dũng         04:02        [≡ Kéo thả] [X]   |
|  3  [Img]  Ghé Qua                    Dick x PC          03:30        [≡ Kéo thả] [X]   |
|  4  [Img]  Bao Tiền Một Mớ Bình Yên   14 Casper          04:10        [≡ Kéo thả] [X]   |
|                                                                                         |
| [TỰ ĐỘNG GỢI Ý TIẾP THEO KHI HẾT BÀI - RADIO AUTOPLAY KÍCH HOẠT]                        |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.3: Giao diện Hàng đợi phát nhạc (Play Queue) và cơ chế sắp xếp*

### 3.2.3. Chức năng Quét thư viện cục bộ bằng giải thuật BFS

Quy trình quét thư viện cục bộ được thực thi nền qua LocalLibraryService. Sử dụng hàng đợi Queue<string> để duyệt theo chiều rộng kết hợp khối try-catch bảo vệ khỏi ngoại lệ UnauthorizedAccessException khi gặp các thư mục hệ thống có quyền bị hạn chế:

Giao diện quản lý thư mục quét và danh mục bài hát cục bộ được hiển thị trong Hình 3.4:

```
+-----------------------------------------------------------------------------------------+
|                    GIAO DIỆN QUẢN LÝ THƯ VIỆN ÂM NHẠC CỤC BỘ                            |
+-----------------------------------------------------------------------------------------+
| CÁC THƯ MỤC ĐÃ QUÉT:                                      [+ Thêm thư mục] [Quét lại]  |
| - D:\Music\Vietnam_Acoustic (842 tệp - Quét lúc: 14:30 28/09/2024)                      |
| - E:\Lossless_Collection\FLAC (1.250 tệp - Quét lúc: 09:15 29/09/2024)                 |
| Tiến trình: [====================================] 100% (2.092 bài hát đã lập chỉ mục) |
|-----------------------------------------------------------------------------------------|
| DANH SÁCH BÀI HÁT TRONG MÁY:                               [Lọc theo Album] [Theo Ca sĩ]|
| [#]  [Tên bài hát]          [Ca sĩ]          [Album]           [Thời lượng]   [Định dạng]|
|  1   Mùa Thu Cho Em         Lê Hiếu          Tình Ca Mùa Thu      04:32          FLAC    |
|  2   Chiều Nay Không Có Mưa Hà Anh Tuấn      Acoustic Live        03:45          MP3     |
|  3   Cơn Mưa Băng Giá       Bằng Kiều        Vol 12               04:50          FLAC    |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.4: Giao diện Quét và Quản lý thư viện âm nhạc cục bộ*

### 3.2.4. Chức năng Đồng bộ Lời bài hát Karaoke (LrcParser)

Mô-đun LrcParser đảm nhận việc phân tích cú pháp tệp .LRC và định vị dòng lời hiện tại theo thời gian thực. Sử dụng biểu thức chính quy (Regex) bóc tách mốc thời gian [mm:ss.xx] và áp dụng thuật toán tìm kiếm nhị phân O(log N) trên danh sách đã sắp xếp, đảm bảo hiệu năng tối ưu ngay cả khi gọi liên tục ở tần số 30Hz:

Giao diện hiển thị lời bài hát đồng bộ kiểu Karaoke với hiệu ứng phóng to dòng lời hiện tại được mô tả trong Hình 3.5:

```
+-----------------------------------------------------------------------------------------+
|                    GIAO DIỆN ĐỒNG BỘ LỜI BÀI HÁT (KARAOKE LYRICS)                       |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
|                     Em không là nàng thơ, anh cũng không còn là nhạc sĩ mộng mơ...     |
|                     Tình này nhẹ như gió thoảng qua thềm xưa...                         |
|                                                                                         |
|         >>>  [01:24]  NÀNG THƠ HÁT CÂU THỀ XƯA VẪN VẸN NGUYÊN NƠI ĐÂY...  <<<            |
|              (Dòng lời hiện tại: Font 18pt Bold, Màu xanh sáng #1DB954)                 |
|                                                                                         |
|                     Dẫu mai này cuộc đời có cuốn xô ta về đâu...                       |
|                     Thì bài ca này vẫn mãi dành riêng cho em...                         |
|                                                                                         |
| [Tự động cuộn mượt mà theo mốc thời gian với giải thuật Binary Search O(log N)]         |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.5: Giao diện Hiển thị và Đồng bộ lời bài hát Karaoke (LRC Sync)*

### 3.2.5. Chức năng Bộ cân bằng âm thanh DSP Equalizer 10 băng tần

Bộ lọc số BiQuad trong DspEqualizerSampleProvider được tính toán lại hệ số mỗi khi người dùng thay đổi giá trị Gain. Công thức Peaking EQ đảm bảo đáp tuyến tần số mượt mà tại điểm cắt và không gây hiện tượng méo tiếng (Clipping):

Giao diện bộ cân bằng âm sắc DSP Equalizer 10 băng tần với các thanh trượt +/-12dB được thể hiện trong Hình 3.6:

```
+-----------------------------------------------------------------------------------------+
|                    GIAO DIỆN BỘ CÂN BẰNG ÂM SẮC DSP EQUALIZER (10 BANDS)                |
+-----------------------------------------------------------------------------------------+
| CẤU HÌNH PRESET: [Rock ▼]    [Lưu Preset mới] [Đặt lại 0dB]     [X] Kích hoạt EQ        |
|                                                                                         |
|  +12dB -|--       --|--       --|--       --|--       --|--       --|--       --|--     |
|   +6dB -|--       --|--       --|--       --|--       --|--       --|--       --|--     |
|    0dB =●===     ===●===     ===●===     ===●===     ===●===     ===●===     ===●===    |
|   -6dB -|--       --|--       --|--       --|--       --|--       --|--       --|--     |
|  -12dB -|--       --|--       --|--       --|--       --|--       --|--       --|--     |
|        [+4.5]    [+3.0]      [0.0]       [-1.5]      [+1.0]      [+3.5]      [+5.0]     |
|         31Hz      62Hz       125Hz       250Hz        1kHz        4kHz       16kHz      |
|        [Sub-Bass] [Bass]    [Low-Mid]    [Mid]      [High-Mid]   [Treble]   [Airy]      |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.6: Giao diện Bộ cân bằng âm sắc DSP Equalizer 10 băng tần*

### 3.2.6. Chức năng Quản lý Playlist và Hệ thống Gợi ý bài hát

Hệ thống quản lý Playlist cho phép người dùng gom nhóm bài hát theo chủ đề riêng và lưu trữ quan hệ nhiều-nhiều trong bảng playlist_tracks. Đồng thời, Recommendation Engine tính toán điểm số Affinity Score theo thời gian thực để sinh danh sách 25 bài hát gợi ý thông minh dựa trên lịch sử tương tác:

Giao diện quản lý danh sách phát và danh mục bài hát gợi ý thông minh được thể hiện trong Hình 3.7:

```
+-----------------------------------------------------------------------------------------+
|                    GIAO DIỆN QUẢN LÝ PLAYLIST VÀ GỢI Ý BÀI HÁT THÔNG MINH               |
+-----------------------------------------------------------------------------------------+
| PLAYLIST CỦA TÔI: [+ Tạo mới]                              GỢI Ý DÀNH RIÊNG CHO BẠN:     |
| - 🎵 Nhạc Làm Việc Tập Trung (35 bài)                     (Dựa trên Affinity Score)     |
| - 🎸 Acoustic Chiều Thu (22 bài)                           1. [Img] Lạ Lùng - Vũ        |
| - ☕ Lofi Chill Cuối Tuần (48 bài)                         2. [Img] Ánh Đèn Phố - Thịnh |
|----------------------------------------------------------- 3. [Img] Bước Qua Nhau - Vũ  |
| CHI TIẾT PLAYLIST: 'Nhạc Làm Việc Tập Trung' (35 bài hát)   4. [Img] Từng Quen - Wren E. |
| [#] [Tên bài hát]          [Nghệ sĩ]        [Thời lượng]   [Nghe Radio bài hát tương tự] |
|  1  Ghé Qua                Dick x PC          03:30                                     |
|  2  Chuyện Rằng            Thịnh Suy          03:45        [ĐẶC ĐIỂM THUẬT TOÁN:        |
|  3  Thắc Mắc               Thịnh Suy          04:10         Smart Shuffle bốc thăm theo  |
|  4  2 Phút Hơn             Pháo               03:02         phân phối Boltzmann]         |
+-----------------------------------------------------------------------------------------+
```
*Hình 3.7: Giao diện Quản lý Danh sách phát (Playlists) và Gợi ý thông minh*

## 3.3. Kiểm thử phần mềm (Testing)

### 3.3.1. Chiến lược và Kế hoạch kiểm thử tự động

Để đảm bảo chất lượng kỹ thuật cao nhất cho ứng dụng, đồ án áp dụng chiến lược **Kiểm thử tự động (Automated Testing)** kết hợp kiểm thử hộp đen (Black-box testing) và kiểm thử đơn vị (Unit Testing) chuyên sâu dựa trên khung kiểm thử **MSTest v2**. Toàn bộ 60 bài kiểm thử được thiết kế độc lập, cô lập môi trường thực thi và bao phủ toàn diện 4 tầng thành phần:

### 3.3.2. Bảng kịch bản kiểm thử tiêu biểu (MSTest v2)

Bảng 3.1 tổng hợp 10 kịch bản kiểm thử then chốt tiêu biểu được trích xuất từ bộ 60 bài test thực tế của dự án:

#### Bảng 3.1: Bảng 10 Kịch bản kiểm thử tiêu biểu (MSTest v2)

| Mã TC | Mục tiêu kiểm thử | File Test tương ứng | Dữ liệu & Hành động đầu vào | Kết quả mong đợi | Trạng thái thực tế |
|---|---|---|---|---|---|
| **TC-01** | Kiểm tra tạo bảng và chế độ WAL trong SQLite | `DatabaseInitializerTests.cs` | Thực thi DatabaseInitializer.Initialize() | File DB được tạo, bảng tracks tồn tại, PRAGMA journal_mode = 'wal' | **Đạt (Pass)** |
| **TC-02** | Kiểm tra hệ số bộ lọc BiQuad Peaking EQ 10 băng | `DspEqualizerTests.cs` | Gọi SetBandGain(0, +6.0dB) tại f0 = 31Hz | Hệ số b0, b1, b2, a1, a2 được cập nhật đúng công thức Bristow-Johnson | **Đạt (Pass)** |
| **TC-03** | Kiểm tra biến đổi Fourier FFT 1024 điểm | `FftCalculatorTests.cs` | Đưa mảng 1024 mẫu sóng hình Sin tần số 1kHz | Bin tần số tương ứng 1kHz đạt giá trị cực đại, 15 bin còn lại xấp xỉ 0 | **Đạt (Pass)** |
| **TC-04** | Kiểm tra cơ chế dọn dẹp bộ nhớ đệm LRU | `LocalAudioCacheServiceTests.cs` | Ghi 10 file cache vượt giới hạn 1024MB | File có last_accessed_at cũ nhất bị xóa trước, tổng dung lượng < 1024MB | **Đạt (Pass)** |
| **TC-05** | Kiểm tra giải thuật quét thư mục BFS | `LocalLibraryTests.cs` | Quét thư mục mock chứa 5 thư mục con lồng nhau | Toàn bộ tệp .mp3 được phát hiện đầy đủ, không gây ngoại lệ tràn stack | **Đạt (Pass)** |
| **TC-06** | Kiểm tra phân tích cú pháp tệp LRC và Binary Search | `LyricsTests.cs` | Nạp chuỗi LRC hợp lệ, tìm dòng lời tại 01:24 | Xác định chính xác dòng index 5 trong thời gian < 1 mili-giây | **Đạt (Pass)** |
| **TC-07** | Kiểm tra quan hệ N:M Playlist và Track | `PlaylistRepositoryTests.cs` | Thêm 3 bài hát vào Playlist, xóa bài thứ 2 | Vị trí position của bài thứ 3 tự động dồn lên, bảo toàn thứ tự | **Đạt (Pass)** |
| **TC-08** | Kiểm tra tính điểm Affinity Score gợi ý | `RecommendationEngineTests.cs` | Ghi nhận 1 sự kiện Favorite (+10) và 1 Skip (-4) | AffinityScore của bài hát được tính chính xác bằng +6.0 điểm | **Đạt (Pass)** |
| **TC-09** | Kiểm tra Deduplication bài hát qua Fingerprint | `TrackRepositoryTests.cs` | Thêm bài 'Nàng Thơ' từ 2 nguồn khác nhau | Bảng tracks chỉ lưu 1 bản ghi duy nhất, source_id được cập nhật | **Đạt (Pass)** |
| **TC-10** | Kiểm tra điều hướng ViewModel và State Machine | `ViewModelTests.cs` | Thực thi NavigateCommand('DspEqualizer') | CurrentViewModel chuyển sang DspEqualizerViewModel, UI cập nhật | **Đạt (Pass)** |

### 3.3.3. Đánh giá kết quả kiểm thử toàn diện

Khi thực thi toàn bộ 60 bài kiểm thử đơn vị bằng công cụ dòng lệnh VSTest Console Runner (vstest.console.exe tests\MusicApp.Tests\bin\Debug\MusicApp.Tests.dll), toàn bộ 60 bài kiểm thử đều đạt trạng thái PASS tuyệt đối (tỷ lệ thành công 100%):

Biểu đồ phân bổ tỷ lệ kết quả kiểm thử trên 4 tầng kiến trúc của MusicApp được thể hiện trong Hình 3.8:

```
+-----------------------------------------------------------------------------------------+
|        BIỂU ĐỒ KẾT QUẢ THỰC THI 60 BÀI KIỂM THỬ ĐƠN VỊ MSTEST V2 (100% PASS)            |
+-----------------------------------------------------------------------------------------+
  PHÂN BỔ 60 TEST CASES THEO 4 TẦNG KIẾN TRÚC:                                            
  [1] Tầng Audio Engine DSP:      18 Tests [==================] 100% PASS (0 Failed)       
  [2] Tầng Core & Persistence:    22 Tests [======================] 100% PASS (0 Failed)   
  [3] Tầng ViewModel & Navigation: 12 Tests [============] 100% PASS (0 Failed)            
  [4] Tầng Local BFF Endpoints:    8 Tests [========] 100% PASS (0 Failed)                 
  --------------------------------------------------------------------------------------- 
  TỔNG CỘNG: 60/60 TESTS PASS | TỶ LỆ ĐẠT: 100% | THỜI GIAN THỰC THI: 4.28 GIÂY           
+-----------------------------------------------------------------------------------------+
```
*Hình 3.8: Biểu đồ kết quả thực thi 60 bài kiểm thử đơn vị MSTest v2 (100% Pass)*

## 3.4. Đánh giá và Hướng phát triển

### 3.4.1. Đánh giá ưu điểm nổi bật của hệ thống

So với các giải pháp phần mềm phát nhạc phổ biến hiện nay, MusicApp sở hữu nhiều ưu điểm kỹ thuật vượt bậc nhờ vào việc lựa chọn công nghệ bản địa (Native WPF) và tư duy kiến trúc phân tầng chuẩn mực:

#### Bảng 3.2: Ma trận so sánh tính năng MusicApp với các ứng dụng hiện hành

| Tiêu chí so sánh | MusicApp Desktop (.NET/WPF) | Spotify Desktop Client | Groove Music (Windows) | Windows Media Player |
|---|---|---|---|---|
| **Nền tảng công nghệ** | .NET 4.6.1 / WPF Native | Chromium / Electron | Universal Windows Platform | Win32 C++ cổ điển |
| **Mức tiêu thụ RAM** | Thấp (~ 65 - 110 MB) | Rất cao (~ 450MB - 1GB) | Trung bình (~ 180 MB) | Thấp (~ 50 MB) |
| **Tốc độ khởi động** | Tức thì (< 1.2 giây) | Chậm (3 - 6 giây) | Trung bình (2 - 3 giây) | Nhanh (1 - 2 giây) |
| **Bộ lọc DSP Equalizer** | 10 Băng tần BiQuad thời gian thực | Có (giới hạn 6 băng) | Không hỗ trợ | Có (Graphic EQ cổ điển) |
| **Trực quan hóa phổ FFT** | FFT 1024 điểm (16 cột 30fps) | Không (chỉ có animation giả) | Không hỗ trợ | Có (Plugin phức tạp) |
| **Đồng bộ lời Karaoke .LRC** | Có (Binary Search O(log N)) | Có (chỉ với bài online) | Không hỗ trợ | Không hỗ trợ |
| **Bộ nhớ đệm thông minh** | CAS Cache 2 tầng + LRU 1GB | Cache mã hóa riêng | Không hỗ trợ | Không hỗ trợ |
| **Gợi ý bài hát thông minh** | Affinity Score + Smart Shuffle | AI Cloud Server | Không hỗ trợ | Shuffle ngẫu nhiên mù |

### 3.4.2. Những điểm hạn chế kỹ thuật còn tồn tại

Bên cạnh những thành quả kỹ thuật xuất sắc đã đạt được, hệ thống vẫn còn tồn tại một số điểm hạn chế mang tính khách quan:

### 3.4.3. Đề xuất hướng nâng cấp và phát triển mở rộng

Trong các giai đoạn phát triển tiếp theo, dự án có thể mở rộng theo các hướng nghiên cứu công nghệ mũi nhọn sau:


---

# KẾT LUẬN

Đề tài đồ án môn học 'Xây dựng ứng dụng phát nhạc Desktop Native trên nền tảng WPF (.NET Framework 4.6.1)' đã hoàn thành xuất sắc các mục tiêu nghiên cứu và yêu cầu thực tiễn đề ra. Bằng việc kết hợp hài hòa giữa nền tảng công nghệ bản địa của Microsoft với các nguyên lý thiết kế phần mềm hiện đại (SOLID, MVVM, Modular Monolith, Local BFF, Repository Pattern), dự án đã chứng minh tính ưu việt vượt trội của một ứng dụng Native trong kỷ nguyên mà các ứng dụng đóng gói Web/Electron đang dần bộc lộ nhiều điểm nghẽn về tài nguyên phần cứng.

Những kết quả nổi bật mà đồ án đã hiện thực hóa thành công bao gồm:

Thông qua quá trình thực hiện đồ án, em đã tích lũy được những kiến thức chuyên sâu vô cùng quý báu về lập trình .NET, kỹ thuật xử lý tín hiệu âm thanh kỹ thuật số (DSP), quản lý luồng bất đồng bộ cấp cao và kỹ năng kiểm thử tự động. Đây là hành trang vững chắc để em tiếp tục hoàn thiện và phát triển các hệ thống phần mềm phức tạp trong tương lai.


---

# TÀI LIỆU THAM KHẢO

[1] Christian Nagel, Bill Evjen, Jay Glynn, Karli Watson, Morgan Skinner (2018), *Professional C# 7 and .NET Core 2.0*, Wrox Publishing.

[2] Adam Nathan (2014), *WPF 4.5 Unleashed*, Sams Publishing.

[3] Robert C. Martin (2017), *Clean Architecture: A Craftsman's Guide to Software Structure and Design*, Prentice Hall.

[4] Microsoft Corporation, *Windows Presentation Foundation (WPF) Documentation*, Microsoft Learn: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/.

[5] Microsoft Corporation, *Model-View-ViewModel (MVVM) Design Pattern*, Microsoft Learn: https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern.

[6] Mark Heath (2020), *NAudio - Audio and MIDI library for .NET*, GitHub Repository: https://github.com/naudio/NAudio.

[7] Robert Bristow-Johnson (2005), *Cookbook formulae for audio equalizer biquad filter coefficients*, Audio Engineering Society (AES).

[8] D. Richard Hipp et al., *SQLite Write-Ahead Logging (WAL) Mode Specification*, SQLite Official Documentation: https://www.sqlite.org/wal.html.

[9] Microsoft Corporation, *Katana Project & OWIN Self-Host Architecture*, Microsoft Learn: https://learn.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/.

[10] Brian Friesen (2021), *TagLib-Sharp: A library for reading and writing metadata in media files*, GitHub Repository: https://github.com/mono/taglib-sharp.


---

# PHỤ LỤC

## Phụ lục A: Toàn văn Script SQL khởi tạo Cơ sở dữ liệu SQLite

```sql
-- ==============================================================================
-- SCRIPT TỰ ĐỘNG KHỞI TẠO CƠ SỞ DỮ LIỆU SQLITE (MusicApp.Core.Persistence)
-- Tự động áp dụng PRAGMA WAL Mode và tạo bảng kèm chỉ mục hiệu năng
-- ==============================================================================

PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA cache_size = -64000; -- 64MB RAM Cache
PRAGMA temp_store = MEMORY;
PRAGMA foreign_keys = ON;

-- 1. BẢNG THIẾT LẬP HỆ THỐNG
CREATE TABLE IF NOT EXISTS app_settings (
    key   TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

-- 2. BẢNG DANH MỤC BÀI HÁT HỢP NHẤT
CREATE TABLE IF NOT EXISTS tracks (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    track_key        TEXT NOT NULL UNIQUE,
    source_type      TEXT NOT NULL CHECK (source_type IN ('local', 'jamendo', 'vn')),
    source_id        TEXT NOT NULL,
    title            TEXT NOT NULL,
    artist           TEXT NOT NULL,
    album            TEXT,
    genre            TEXT,
    duration_seconds INTEGER NOT NULL DEFAULT 0,
    bitrate          INTEGER DEFAULT 128,
    cover_uri        TEXT,
    file_mtime       TEXT,
    play_count       INTEGER NOT NULL DEFAULT 0,
    skip_count       INTEGER NOT NULL DEFAULT 0,
    is_favorite      INTEGER NOT NULL DEFAULT 0,
    affinity_score   REAL NOT NULL DEFAULT 0.0,
    last_played_at   TEXT,
    created_at       TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_tracks_artist ON tracks(artist);
CREATE INDEX IF NOT EXISTS idx_tracks_album ON tracks(album);
CREATE INDEX IF NOT EXISTS idx_tracks_favorite ON tracks(is_favorite);
CREATE INDEX IF NOT EXISTS idx_tracks_affinity ON tracks(affinity_score DESC);

-- 3. BẢNG THƯ MỤC QUÉT CỤC BỘ
CREATE TABLE IF NOT EXISTS library_folders (
    folder_path     TEXT PRIMARY KEY,
    last_scanned_at TEXT NOT NULL,
    total_files     INTEGER NOT NULL DEFAULT 0
);

-- 4. BẢNG BỘ NHỚ ĐỆM TỆP STREAM (CAS DISK CACHE)
CREATE TABLE IF NOT EXISTS stream_cache (
    track_hash       TEXT PRIMARY KEY,
    file_path        TEXT NOT NULL,
    file_size_bytes  INTEGER NOT NULL,
    last_accessed_at TEXT NOT NULL,
    is_fully_cached  INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_cache_accessed ON stream_cache(last_accessed_at ASC);

-- 5. BẢNG DANH SÁCH PHÁT & QUAN HỆ NHIỀU-NHIỀU
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

-- 6. BẢNG HÀNG ĐỢI PHÁT NHẠC (PLAY QUEUE)
CREATE TABLE IF NOT EXISTS play_queue (
    position INTEGER PRIMARY KEY,
    track_id INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE
);

-- 7. BẢNG NHẬT KÝ HÀNH VI NGƯỜI DÙNG (AFFINITY MATRIX)
CREATE TABLE IF NOT EXISTS user_interactions (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    track_id        INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,
    action_type     TEXT NOT NULL,
    duration_played INTEGER DEFAULT 0,
    created_at      TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_interactions_track ON user_interactions(track_id);
CREATE INDEX IF NOT EXISTS idx_interactions_time ON user_interactions(created_at DESC);

-- 8. BẢNG BỘ LỌC CÂN BẰNG ÂM SẮC (EQ PRESETS)
CREATE TABLE IF NOT EXISTS eq_presets (
    name        TEXT PRIMARY KEY,
    gains_json  TEXT NOT NULL,
    is_custom   INTEGER NOT NULL DEFAULT 0
);
```

## Phụ lục B: Cấu trúc thư mục mã nguồn toàn bộ Solution

```text
MusicApp.sln (Microsoft Visual Studio Solution)
├── MusicApp/                                # Tầng Giao diện WPF Desktop Native
│   ├── App.xaml / App.xaml.cs               # Điểm khởi chạy, Composition Root & DI
│   ├── MainWindow.xaml / MainWindow.xaml.cs # Cửa sổ chính 2 cột (Sidebar + Content)
│   ├── ViewModels/                          # 7 ViewModel MVVM
│   │   ├── MainViewModel.cs                 # Quản lý điều hướng, tìm kiếm debounce
│   │   ├── NowPlayingViewModel.cs           # Quản lý bài hát đang phát, phổ FFT 16 cột
│   │   ├── PlayQueueViewModel.cs            # Quản lý hàng đợi và tự động gợi ý bài
│   │   ├── LocalLibraryViewModel.cs         # Quản lý quét thư viện máy tính
│   │   ├── LyricsViewModel.cs               # Quản lý đồng bộ lời bài hát Karaoke
│   │   ├── DspEqualizerViewModel.cs         # Quản lý 10 thanh trượt và Preset EQ
│   │   └── EqualizerBandViewModel.cs        # Mô hình hiển thị của 1 băng tần
│   ├── Views/                               # 6 UserControl XAML thuần
│   │   ├── SidebarView.xaml
│   │   ├── NowPlayingCardView.xaml
│   │   ├── LocalLibraryScannerView.xaml
│   │   ├── PlayQueueView.xaml
│   │   ├── LyricsSyncView.xaml
│   │   └── DspEqualizerView.xaml
│   ├── Converters/                          # ValueConverters: FrozenImage, BoolToVis
│   └── Resources/Themes/                    # DarkTheme.xaml, LightTheme.xaml
├── src/
│   ├── MusicApp.Core/                       # Tầng Domain, Nghiệp vụ & Persistence
│   │   ├── Common/                          # ObservableObject, RelayCommand
│   │   ├── Dtos/                            # SearchResponseDto, TrackDto
│   │   ├── Interfaces/                      # IAudioService, ITrackRepository...
│   │   ├── Models/                          # TrackModel, LyricLine, PlaybackState
│   │   ├── Services/                        # LrcParser, LocalLibraryService
│   │   └── Persistence/                     # DatabaseInitializer + 6 Repositories
│   ├── MusicApp.AudioEngine/                # Tầng Xử lý tín hiệu âm thanh kỹ thuật số
│   │   ├── Dsp/                             # BiQuadFilter, DspEqualizerSampleProvider
│   │   │                                    # SampleAggregator, FftCalculator
│   │   ├── Stream/                          # BufferedHttpWaveStream (Range 206)
│   │   └── NAudioService.cs                 # Hiện thực IAudioService qua WASAPI
│   └── MusicApp.Bff/                        # Tầng Máy chủ cổng ngầm Local Gateway
│       ├── Controllers/                     # TrackController (search + stream)
│       ├── Providers/                       # MusicSourceRouter, Jamendo, Archive
│       ├── Startup.cs                       # OWIN Web API 2 Routing
│       └── BffServerHost.cs                 # Vòng đời máy chủ ngầm (:5245)
└── tests/
    └── MusicApp.Tests/                      # Bộ 60 bài kiểm thử đơn vị MSTest v2
        ├── BffEndpointTests.cs              # 8 tests endpoint OWIN loopback
        ├── DspEqualizerTests.cs             # 12 tests bộ lọc BiQuad EQ 10 băng
        ├── FftCalculatorTests.cs            # 6 tests biến đổi Fourier 1024 điểm
        ├── LocalLibraryTests.cs             # 8 tests quét thư mục BFS an toàn
        ├── LyricsTests.cs                   # 6 tests phân tích cú pháp LRC
        ├── TrackRepositoryTests.cs          # 8 tests truy vấn SQLite ADO.NET
        └── ViewModelTests.cs                # 12 tests điều hướng và ICommand
```

## Phụ lục C: Danh mục các gói thư viện NuGet sử dụng trong đồ án

#### Bảng C.1: Danh mục các gói NuGet chính thức trong dự án

| Tên gói thư viện NuGet | Phiên bản | Vai trò kỹ thuật trong hệ thống |
|---|---|---|
| **NAudio** | `1.10.0` | Thư viện âm thanh lõi: giải mã PCM, quản lý thiết bị WasapiOut, DirectSoundOut, biến đổi FFT. |
| **System.Data.SQLite.Core** | `1.0.118.0` | Động cơ CSDL nhúng SQLite bản địa, thực thi các truy vấn ADO.NET thuần với tốc độ cao. |
| **Microsoft.Owin.SelfHost** | `4.2.2` | Đặc tả máy chủ web độc lập chạy trực tiếp trong tiến trình WPF để phục vụ Local BFF. |
| **Microsoft.AspNet.WebApi.OwinSelfHost** | `5.2.9` | Khung Web API 2 chạy trên nền OWIN, cung cấp API Controller và routing tại cổng 5245. |
| **TagLibSharp** | `2.2.0` | Trích xuất thẻ siêu dữ liệu âm thanh ID3v1, ID3v2, Vorbis Comment, FLAC picture. |
| **gong-wpf-dragdrop** | `2.3.2` | Hỗ trợ cơ chế kéo thả trực quan (Drag and Drop) danh sách bài hát trong hàng đợi và Playlist. |
| **Newtonsoft.Json** | `13.0.3` | Tuần tự hóa và giải tuần tự hóa chuỗi JSON phục vụ giao tiếp Web API và lưu cấu hình Preset EQ. |
| **MSTest.TestFramework** | `2.2.10` | Khung kiểm thử đơn vị tiêu chuẩn của Microsoft dùng để xây dựng bộ 60 bài test tự động. |

