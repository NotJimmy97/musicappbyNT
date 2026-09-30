# KỊCH BẢN LỜI THOẠI THUYẾT MINH BẢO VỆ ĐỒ ÁN: MUSICAPP DESKTOP
## TÀI LIỆU LỜI THOẠI TRÌNH BÀY (PRESENTER SCRIPT) DÀNH CHO SINH VIÊN
### HỆ THỐNG PHÁT NHẠC DESKTOP NATIVE WPF (.NET FRAMEWORK 4.6.1)

---

## I. HƯỚNG DẪN TỔNG QUAN DÀNH CHO NGƯỜI THUYẾT TRÌNH

- **Tổng thời lượng tiêu chuẩn:** 15 – 20 phút.
  - **Phần 1: Thuyết trình theo Slide (13 Slide):** 10 – 12 phút.
  - **Phần 2: Demo trực tiếp phần mềm (Live Demo):** 3 – 5 phút.
  - **Phần 3: Trả lời câu hỏi phản biện của Hội đồng (Q&A):** 3 – 5 phút.
- **Phong thái trình bày:**
  - Tự tin, giọng nói rõ ràng, tốc độ vừa phải (~120 – 140 từ/phút).
  - Tương tác mắt (Eye-contact) với Quý Thầy/Cô trong Hội đồng, không nhìn chăm chăm vào màn hình máy tính hay đọc nguyên văn slide.
  - Nhấn giọng dứt khoát ở các **từ khóa kỹ thuật cốt lõi** (*Native WPF*, *Local BFF OWIN*, *Zero Heap Allocation*, *Bi-quad IIR*, *FFT Hann Window*, *SQLite WAL*, *Boltzmann Sampling*).
- **Quy ước ký hiệu trong tài liệu:**
  - `[Hành động / Thao tác]`: Hướng dẫn sinh viên bấm chuyển slide, chỉ tay lên màn hình hoặc thao tác trên ứng dụng.
  - `> "Lời thoại..."`: Câu chữ chính xác để sinh viên nói trước Hội đồng (đã được tối ưu ngữ điệu báo cáo học thuật).
  - `[Chuyển ý / Transition]`: Câu nối tự nhiên để dẫn dắt mạch tư duy sang slide tiếp theo.

---

## II. LỜI THOẠI CHI TIẾT THEO TỪNG SLIDE (SLIDE 1 – SLIDE 13)

---

### SLIDE 1: TIÊU ĐỀ & GIỚI THIỆU ĐỀ TÀI
- **Thời lượng dự kiến:** 01 phút (00:00 – 01:00)
- **Hành động & Trực quan:**
  - `[Chiếu Slide 1]`
  - `[Mở sẵn ứng dụng MusicApp ở chế độ nền trên máy tính để tạo ấn tượng]`
  - `[Đứng thẳng, chào Hội đồng với thái độ trang trọng, tự tin]`

> *"Kính thưa Quý Thầy/Cô trong Hội đồng và toàn thể các bạn sinh viên,*
>
> *Hôm nay, em xin đại diện nhóm thực hiện đề tài báo cáo về đồ án tốt nghiệp chuyên ngành: **'Xây dựng ứng dụng phát nhạc Desktop Native trên nền tảng WPF (.NET Framework 4.6.1)'**.*
>
> *Trong đồ án này, mục tiêu trọng tâm của chúng em không chỉ dừng lại ở việc tạo ra một giao diện nghe nhạc đơn thuần, mà là nghiên cứu và hiện thực hóa một **kiến trúc phần mềm chuẩn mực** cho ứng dụng máy tính cá nhân. Bằng cách kết hợp mô hình MVVM thuần túy của WPF, kiến trúc trạm trung gian cục bộ Local BFF chạy trên máy chủ OWIN Self-Host, chuỗi xử lý tín hiệu âm thanh số DSP độ trễ thấp của NAudio và cơ sở dữ liệu SQLite tối ưu hóa chế độ WAL, dự án đã mang lại trải nghiệm phát nhạc mượt mà, chuyên nghiệp và hiệu năng cao tương đương các phần mềm thương mại hàng đầu hiện nay.*
>
> *Sau đây, em xin phép bắt đầu phần trình bày chi tiết về quá trình phân tích, thiết kế và cài đặt hệ thống."*

- **[Chuyển ý sang Slide 2]:**
  > *"Để hiểu rõ tại sao nhóm lại lựa chọn phát triển ứng dụng này theo hướng Desktop Native, em xin mời Quý Thầy/Cô cùng nhìn lại bức tranh thực tế về các phần mềm phát nhạc hiện nay trên thị trường."*

---

### SLIDE 2: BỐI CẢNH, LÝ DO CHỌN ĐỀ TÀI & THÁCH THỨC KỸ THUẬT
- **Thời lượng dự kiến:** 01 phút 15 giây (01:00 – 02:15)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 2]`
  - `[Chỉ tay vào bảng so sánh giữa Electron và Native WPF]`
  - `[Nhấn mạnh con số 1GB RAM so với 100MB RAM]`

> *"Thưa Thầy/Cô, khi quan sát thói quen sử dụng máy tính hiện nay, chúng ta thấy hầu hết người dùng đều vừa làm việc, lập trình hoặc chơi game vừa nghe nhạc. Tuy nhiên, các phần mềm phát nhạc phổ biến hiện nay như Spotify hay Discord hầu như đều được đóng gói bằng công nghệ Electron — bản chất là nhúng cả một trình duyệt web Chromium vào hệ điều hành.*
>
> *Hệ quả là gì ạ? Một ứng dụng chỉ để phát âm thanh lại chiếm dụng từ **500MB đến hơn 1GB RAM**, làm giảm hiệu năng chung của máy tính. Đồng thời, tầng trừu tượng Web Audio không cho phép can thiệp sâu vào xử lý tín hiệu số cấp thấp, và ứng dụng gần như tê liệt nếu mất kết nối Internet.*
>
> *Từ thực trạng đó, đề tài của chúng em đặt ra câu hỏi kỹ thuật cốt lõi: **Làm thế nào để xây dựng một ứng dụng phát nhạc Desktop Native thực thụ, vừa sở hữu giao diện hiện đại, vừa can thiệp sâu vào xử lý tín hiệu âm thanh DSP với độ trễ thấp, mà chỉ chiếm chưa đến 120MB RAM?***
>
> *Để giải quyết trọn vẹn bài toán này, nhóm đã nghiên cứu nền tảng WPF trên .NET Framework và đặt ra các tiêu chuẩn kỹ thuật khắt khe về quản trị bộ nhớ cũng như kiến trúc phân tầng."*

- **[Chuyển ý sang Slide 3]:**
  > *"Xuất phát từ bài toán kỹ thuật đó, nhóm đã tiến hành khảo sát nghiệp vụ và xây dựng ma trận yêu cầu chức năng cho toàn bộ hệ thống."*

---

### SLIDE 3: KHẢO SÁT NGHIỆP VỤ & MA TRẬN YÊU CẦU HỆ THỐNG
- **Thời lượng dự kiến:** 01 phút (02:15 – 03:15)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 3]`
  - `[Chỉ vào danh mục 10 phân hệ chức năng và 3 yêu cầu phi chức năng]`

> *"Thưa Thầy/Cô, để ứng dụng đáp ứng tốt nhu cầu thực tế của người dùng, nhóm đã phân rã hệ thống thành **10 phân hệ nghiệp vụ hoàn chỉnh** như trên màn hình.*
>
> *Bên cạnh các tính năng phát nhạc cơ bản, điểm khác biệt lớn của MusicApp là sự xuất hiện của các phân hệ kỹ thuật chuyên sâu: bộ cân bằng âm thanh 10 băng tần DSP, phân tích phổ FFT hiển thị cột sóng thời gian thực, đồng bộ lời bài hát Karaoke chuẩn xác đến từng mili-giây, và đặc biệt là hệ thống đề xuất bài hát Smart Shuffle dựa trên lịch sử nghe.*
>
> *Đồng thời, nhóm đặt ra 3 yêu cầu phi chức năng bất biến:
> - Thứ nhất: **Zero UI Freezing** — giao diện tuyệt đối không được giật lag, duy trì ổn định 60 khung hình/giây.
> - Thứ hai: **Zero Heap Allocation** — chuỗi âm thanh không được cấp phát bộ nhớ động liên tục để tránh Garbage Collector ngắt quãng tiếng.
> - Và thứ ba: CSDL SQLite phải vận hành theo chuẩn ACID với tốc độ đọc ghi tức thời."*

- **[Chuyển ý sang Slide 4]:**
  > *"Để hiện thực hóa trọn vẹn 10 phân hệ chức năng cùng các ràng buộc phi chức năng khắt khe này, nhóm đã thiết kế kiến trúc hệ thống theo mô hình Modular Monolith."*

---

### SLIDE 4: KIẾN TRÚC TỔNG THỂ HỆ THỐNG: MODULAR MONOLITH & MVVM
- **Thời lượng dự kiến:** 01 phút 30 giây (03:15 – 04:45)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 4]`
  - `[Chỉ vào sơ đồ kiến trúc 5 project, nhấn mạnh mũi tên phụ thuộc một chiều]`

> *"Kính thưa Hội đồng, đây là sơ đồ kiến trúc tổng thể của hệ thống MusicApp.*
>
> *Thay vì gộp chung tất cả mã nguồn vào một dự án WPF duy nhất như cách làm thông thường, nhóm đã tổ chức solution theo mô hình **Modular Monolith với 5 dự án độc lập**, tuân thủ nguyên lý thiết kế Clean Architecture và các nguyên tắc SOLID.*
>
> *Cụ thể:
> - Tầng nhân là `MusicApp.Core` chứa toàn bộ thực thể nghiệp vụ, giao diện kết nối, bộ giải mã lời bài hát LRC và các Repository dữ liệu. Tầng này độc lập hoàn toàn và không tham chiếu ngược ra ngoài.
> - Tầng `MusicApp.AudioEngine` chịu trách nhiệm độc quyền về điều khiển thiết bị phần cứng âm thanh và thuật toán DSP.
> - Tầng `MusicApp.Bff` đóng vai trò là một máy chủ Web API siêu nhỏ chạy ngầm trong ứng dụng. Giữa AudioEngine và BFF **tuyệt đối không có phụ thuộc vòng**, chỉ giao tiếp qua cổng mạng Loopback nội bộ.
> - Và trên cùng là tầng giao diện `MusicApp` áp dụng mô hình MVVM thuần túy: View kết nối với ViewModel qua Data Binding và RelayCommand, loại bỏ hoàn toàn mã logic nghiệp vụ khỏi tệp code-behind.*
>
> *Nhờ sự phân tầng rõ ràng này, mã nguồn có tính kết dính cao, giảm thiểu phụ thuộc và hỗ trợ kiểm thử tự động vô cùng thuận tiện."*

- **[Chuyển ý sang Slide 5]:**
  > *"Sau đây, em xin đi sâu vào phân hệ đóng vai trò 'trạm điều phối dữ liệu' của ứng dụng: đó chính là Local BFF OWIN."*

---

### SLIDE 5: TRẠM DỊCH VỤ CỤC BỘ LOCAL BFF OWIN & HTTP 206 STREAMING PROXY
- **Thời lượng dự kiến:** 01 phút 15 giây (04:45 – 06:00)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 5]`
  - `[Chỉ vào sequence diagram mô tả luồng HTTP 206 Range Request]`

> *"Thưa Thầy/Cô, một trong những điểm kiến trúc sáng tạo nhất của đồ án chính là việc triển khai **Local Backend-for-Frontend (BFF)**.*
>
> *Thay vì để giao diện WPF gọi trực tiếp các API bên ngoài, nhóm đã nhúng một máy chủ Web API OWIN Self-Host chạy trên cổng nội bộ 5245. Điều này mang lại 3 giá trị kỹ thuật cốt lõi:*
>
> *Thứ nhất, nó giải quyết bài toán tìm kiếm tiếng Việt. Nhờ giải thuật chuẩn hóa Unicode FormD trên tầng BFF, người dùng gõ từ khóa không dấu vẫn tìm thấy chính xác bài hát có dấu với tốc độ phản hồi chỉ vài mili-giây.*
>
> *Thứ hai, nó đóng vai trò là một **HTTP 206 Streaming Proxy**. Khi người dùng tua một bài hát trực tuyến, thay vì phải tải toàn bộ tệp MP3 về bộ nhớ, hệ thống sẽ gửi các Range Request phân đoạn từng gói byte nhị phân. Điều này giúp độ trễ khi tua nhạc giảm xuống dưới 80 mili-giây mà hoàn toàn không gây tràn bộ nhớ RAM.*
>
> *Và thứ ba, hệ thống tích hợp sẵn MemoryCache với thời gian lưu đệm 30 phút, giúp giảm tới 70% số lượng request trùng lặp ra ngoài Internet."*

- **[Chuyển ý sang Slide 6]:**
  > *"Nếu như Local BFF là trạm cung cấp dữ liệu, thì AudioEngine chính là trái tim kỹ thuật vận hành âm thanh của MusicApp."*

---

### SLIDE 6: AUDIO ENGINE & 10-BAND GRAPHIC EQUALIZER DSP PIPELINE
- **Thời lượng dự kiến:** 01 phút 30 giây (06:00 – 07:30)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 6]`
  - `[Chỉ vào Audio Graph Pipeline, nhấn mạnh khối DspEqualizerSampleProvider]`
  - `[Nhấn giọng ở khái niệm Zero Heap Allocation]`

> *"Kính thưa Thầy/Cô, trái tim của ứng dụng MusicApp nằm ở **Engine Xử lý Tín hiệu Âm thanh DSP**.*
>
> *Để mang lại chất lượng âm thanh đẳng cấp, nhóm đã hiện thực hóa một bộ cân bằng đồ họa 10 băng tần chuẩn ISO từ 32Hz đến 16kHz bằng thuật toán lọc đáp ứng xung vô hạn Bi-quad IIR Peaking Filter.*
>
> *Tại đây, nhóm đã giải quyết một thách thức kỹ thuật lớn trong lập trình âm thanh: Trong luồng đọc dữ liệu của NAudio, hàm `Read()` được gọi liên tục hàng trăm lần mỗi giây. Nếu chúng ta tạo mới đối tượng bộ lọc mỗi khi người dùng kéo cần gạt, bộ thu gom rác (Garbage Collector) sẽ kích hoạt và gây ra hiện tượng khựng tiếng (audio stuttering).*
>
> *Nhóm đã áp dụng kỹ thuật **Zero Heap Allocation**: toàn bộ các hệ số toán học của bộ lọc được cập nhật tại chỗ thông qua phương thức `SetPeakingEq`. Đồng thời, hệ thống duy trì 20 bộ lọc tách biệt cho 2 kênh âm thanh Stereo và tích hợp bộ Soft Limiter chống vỡ tiếng khi đẩy dải trầm lên mức tối đa."*

- **[Chuyển ý sang Slide 7]:**
  > *"Song song với việc xử lý âm thanh, làm thế nào để người dùng 'nhìn thấy' được giai điệu? Đó chính là nhiệm vụ của phân hệ phân tích phổ FFT."*

---

### SLIDE 7: PHÂN TÍCH PHỔ ÂM THANH FFT & HIỆU ỨNG TRỰC QUAN ĐĨA THAN
- **Thời lượng dự kiến:** 01 phút (07:30 – 08:30)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 7]`
  - `[Mô tả công thức cửa sổ Hanning và cơ chế Event Throttling 33ms]`

> *"Thưa Thầy/Cô, bên cạnh việc nghe nhạc, trải nghiệm thị giác là yếu tố then chốt tạo nên sự lôi cuốn của MusicApp.*
>
> *Hệ thống đã triển khai thuật toán biến đổi phổ Fourier nhanh FFT 1024 điểm kết hợp cửa sổ Hanning để trích xuất năng lượng âm thanh của 16 dải tần số logarit. Các cột sóng này nhảy múa đồng bộ theo điệu nhạc với màu xanh đặc trưng của Spotify.*
>
> *Tuy nhiên, một bài toán tối ưu quan trọng ở đây là: Nếu cứ mỗi lần tính xong FFT ta lại bắn sự kiện lên giao diện, luồng WPF Dispatcher sẽ bị nghẽn thông điệp và đơ ứng dụng ngay lập tức. Nhóm đã xử lý triệt để vấn đề này bằng cơ chế **Event Throttling 33 mili-giây**, giới hạn tốc độ dựng hình ở mức 30 khung hình/giây. Điều này giúp hiệu ứng cột sóng và đĩa than xoay tròn chuyển động vô cùng mượt mà mà mức chiếm dụng CPU của toàn bộ ứng dụng chỉ ở mức dưới 2.5%."*

- **[Chuyển ý sang Slide 8]:**
  > *"Tiếp theo, em xin trình bày về cách hệ thống bảo toàn dữ liệu bài hát và danh sách phát của người dùng thông qua tầng lưu trữ SQLite."*

---

### SLIDE 8: TẦNG LƯU TRỮ DỮ LIỆU BỀN VỮNG: CSDL SQLITE CHẾ ĐỘ WAL
- **Thời lượng dự kiến:** 01 phút (08:30 – 09:30)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 8]`
  - `[Chỉ vào sơ đồ thực thể liên kết 9 bảng]`
  - `[Nhấn mạnh từ khóa PRAGMA journal_mode = WAL]`

> *"Kính thưa Hội đồng, để người dùng không bị mất danh sách bài hát yêu thích, lịch sử hàng đợi và vị trí bài đang nghe dở sau mỗi lần tắt ứng dụng, nhóm đã thiết kế một tầng lưu trữ dữ liệu bền vững chuẩn mực.*
>
> *Cơ sở dữ liệu SQLite được thiết kế chuẩn hóa gồm 9 bảng quan hệ, bao quát từ danh mục bài hát, danh sách phát cá nhân, hàng đợi nghe nhạc cho đến bộ nhớ đệm luồng âm thanh ngoại tuyến.*
>
> *Điểm mấu chốt ở đây là cấu hình **Write-Ahead Logging (WAL mode)** kết hợp vùng nhớ đệm 64MB RAM. Trong các ứng dụng đa luồng, khi luồng ngầm vừa ghi nhận điểm tương tác người dùng, mà giao diện lại vừa đọc dữ liệu bài hát, nếu dùng SQLite mặc định sẽ rất dễ bị lỗi khóa cơ sở dữ liệu. Nhờ cơ chế WAL, các tác vụ đọc và ghi hoàn toàn không chặn lẫn nhau, đảm bảo tính toàn vẹn dữ liệu ACID và tốc độ truy vấn tức thời."*

- **[Chuyển ý sang Slide 9]:**
  > *"Dựa trên dữ liệu tương tác bền vững này, MusicApp đã xây dựng một tính năng vô cùng thông minh: Động cơ gợi ý cá nhân hóa Smart Shuffle."*

---

### SLIDE 9: ĐỘNG CƠ GỢI Ý CÁ NHÂN HÓA & THUẬT TOÁN SMART SHUFFLE
- **Thời lượng dự kiến:** 01 phút 15 giây (09:30 – 10:45)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 9]`
  - `[Chỉ vào bảng điểm Affinity Score và công thức phân phối Boltzmann]`

> *"Thưa Thầy/Cô, một trong những tính năng cao cấp mà nhóm học hỏi từ các nền tảng streaming hiện đại như Spotify chính là **Hệ thống gợi ý bài hát cá nhân hóa và chế độ Smart Shuffle**.*
>
> *Thay vì trộn bài ngẫu nhiên 50-50 bằng hàm `Random()` máy móc, hệ thống của chúng em học thói quen người dùng theo thời gian thực. Mỗi khi người dùng nghe hết một bài, hệ thống cộng 5 điểm; bấm yêu thích được cộng 10 điểm; nhưng nếu vừa bật lên mà bấm Skip qua ngay thì bị trừ 4 điểm.*
>
> *Khi kích hoạt chế độ Smart Shuffle, thuật toán **phân phối xác suất Boltzmann** sẽ biến đổi các điểm số này thành xác suất lựa chọn bài hát tiếp theo. Các bài hát hợp gu người dùng sẽ có cơ hội được phát cao hơn, nhưng các bài hát mới vẫn có một tỷ lệ xác suất nhất định để xuất hiện. Điều này tạo nên sự cân bằng hoàn hảo giữa việc thưởng thức bài hát quen thuộc và khám phá những giai điệu mới."*

- **[Chuyển ý sang Slide 10]:**
  > *"Bên cạnh tính năng trực tuyến và gợi ý thông minh, phân hệ ngoại tuyến của MusicApp cũng được trang bị các giải thuật tối ưu mạnh mẽ."*

---

### SLIDE 10: PHÂN HỆ NGOẠI TUYẾN: QUÉT THƯ VIỆN BFS, HÀNG ĐỢI KÉO THẢ & KARAOKE LYRICS
- **Thời lượng dự kiến:** 01 phút 15 giây (10:45 – 12:00)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 10]`
  - `[Nhấn mạnh vào 3 điểm: BFS Scan, Freeze Image và Binary Search Lyrics]`

> *"Kính thưa Hội đồng, phân hệ ngoại tuyến của MusicApp được chăm chút rất tỉ mỉ để người dùng có được trải nghiệm tuyệt vời nhất với kho nhạc có sẵn trong máy tính.*
>
> *Đầu tiên là dịch vụ quét thư viện cục bộ: Nhóm sử dụng giải thuật hàng đợi BFS thay vì đệ quy hàm để đảm bảo an toàn tuyệt đối trước các thư mục cây sâu hàng trăm cấp của Windows. Đặc biệt, toàn bộ ảnh bìa album được xử lý bằng lệnh `.Freeze()`, giúp dữ liệu hình ảnh trở thành bất biến và triệt tiêu hoàn toàn nguy cơ rò rỉ bộ nhớ.*
>
> *Kế đến là hàng đợi Play Queue hỗ trợ kéo thả trực quan để sắp xếp lại thứ tự bài hát. Cuối cùng là tính năng đồng bộ lời bài hát Karaoke từ tệp .LRC: Nhờ áp dụng giải thuật tìm kiếm nhị phân với độ phức tạp chỉ $O(\log N)$ và kỹ thuật Event Gating chống gửi thông điệp dư thừa, lời bài hát được highlight và cuộn tự động vào giữa màn hình vô cùng chính xác theo đúng từng câu hát của ca sĩ."*

- **[Chuyển ý sang Slide 11]:**
  > *"Để hoàn thiện một khối lượng công việc đồ sộ như vậy, nhóm đã trải qua một tiến trình phát triển bài bản và vượt qua nhiều bài toán kỹ thuật thực tế."*

---

### SLIDE 11: QUÁ TRÌNH XÂY DỰNG, TIẾN ĐỘ THỰC HIỆN & QUẢN TRỊ RỦI RO
- **Thời lượng dự kiến:** 01 phút (12:00 – 13:00)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 11]`
  - `[Chỉ vào bảng 4 bài toán kỹ thuật lớn và giải pháp tương ứng]`

> *"Thưa Thầy/Cô, quá trình xây dựng dự án MusicApp là một hành trình kỹ thuật thực sự với 9 giai đoạn phát triển bài bản.*
>
> *Trong quá trình này, nhóm đã đối mặt và giải quyết thành công 4 rủi ro kỹ thuật lớn:
> - Thứ nhất là hiện tượng giao diện bị đơ do luồng âm thanh phát dữ liệu phổ quá dồn dập — được giải quyết bằng kỹ thuật Event Throttling 33ms.
> - Thứ hai là hiện tượng rò rỉ RAM nghiêm trọng khi quét ảnh bìa album — được giải quyết bằng phương thức đóng băng `BitmapImage.Freeze()`.
> - Thứ ba là hiện tượng méo tiếng Stereo trên bộ lọc EQ — được giải quyết bằng việc cách ly 20 bộ lọc nhị thức riêng biệt cho hai kênh Trái và Phải.
> - Và thứ tư là lỗi khóa database trong môi trường đa luồng — được giải quyết bằng chế độ ghi trước SQLite WAL.
>
> *Chính việc giải quyết triệt để các bài toán hóc búa này đã giúp đồ án đạt được độ ổn định và hoàn thiện kỹ thuật rất cao."*

- **[Chuyển ý sang Slide 12]:**
  > *"Chất lượng kỹ thuật của ứng dụng không chỉ dừng lại ở mặt lý thuyết, mà đã được kiểm chứng nghiêm ngặt thông qua hệ thống kiểm thử tự động và đo lường thực tế."*

---

### SLIDE 12: KIỂM THỬ PHẦN MỀM TỰ ĐỘNG & ĐÁNH GIÁ KẾT QUẢ THỰC NGHIỆM
- **Thời lượng dự kiến:** 01 phút (13:00 – 14:00)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 12]`
  - `[Chỉ vào con số 75/75 Tests Pass 100% và bảng Benchmark thực tế]`

> *"Kính thưa Quý Thầy/Cô, để khẳng định chất lượng kỹ thuật của đề tài, nhóm không chỉ đánh giá bằng mắt thường mà xây dựng một bộ kiểm thử tự động toàn diện.*
>
> *Dự án kiểm thử `MusicApp.Tests` gồm **75 ca kiểm thử tự động** bao phủ tất cả các phân hệ cốt lõi từ thuật toán DSP, tính toán phổ FFT, giao dịch cơ sở dữ liệu SQLite cho đến logic điều phối hàng đợi và bộ đề xuất bài hát. Toàn bộ 75 bài kiểm thử đều vượt qua 100% trên luồng CI/CD của GitHub Actions.*
>
> *Khi chạy thực nghiệm trên môi trường Windows thực tế, các chỉ số đo đạc được vô cùng ấn tượng: Thời gian khởi động ứng dụng chỉ mất 1.2 giây; dung lượng RAM chiếm dụng ổn định quanh mức 90MB; CPU chỉ tiêu thụ khoảng 2%; và tốc độ quét thư viện cục bộ đạt xấp xỉ 1,500 bài hát mỗi giây. Đây là những minh chứng số học rõ ràng cho tính tối ưu của kiến trúc hệ thống."*

- **[Chuyển ý sang Slide 13]:**
  > *"Cuối cùng, em xin tóm lược những kết quả đạt được và định hướng mở rộng của đồ án."*

---

### SLIDE 13: TỔNG KẾT ĐỀ TÀI, HƯỚNG PHÁT TRIỂN & LỜI KẾT
- **Thời lượng dự kiến:** 01 phút (14:00 – 15:00)
- **Hành động & Trực quan:**
  - `[Chuyển sang Slide 13]`
  - `[Nhìn thẳng vào Hội đồng, hạ giọng chân thành, trân trọng]`

> *"Kính thưa Thầy/Cô và các bạn,*
>
> *Qua quá trình nghiên cứu và thực hiện đề tài MusicApp, nhóm đã thu nhận được những trải nghiệm vô cùng quý giá về tư duy kiến trúc phần mềm, nguyên lý tối ưu hiệu năng và phương pháp làm việc chuẩn chỉ của một kỹ sư phát triển phần mềm.*
>
> *Đồ án đã chứng minh rằng: Với nền tảng .NET và WPF bản địa, chúng ta hoàn toàn có thể xây dựng nên những ứng dụng máy tính cá nhân mạnh mẽ, giao diện đẹp mắt, độ trễ âm thanh bằng không và tiêu thụ tài nguyên siêu nhẹ, vượt trội hơn hẳn các giải pháp web wrapper cồng kềnh hiện nay.*
>
> *Nhóm xin chân thành cảm ơn sự lắng nghe và đồng hành của Quý Thầy/Cô. Sau đây, nhóm xin kính mời Quý Thầy/Cô theo dõi phần demo trực tiếp trên phần mềm và rất mong nhận được những lời nhận xét, góp ý quý báu từ Hội đồng! Em xin trân trọng cảm ơn!"*

---

## III. KỊCH BẢN TRẢ LỜI CÂU HỎI PHẢN BIỆN CỦA HỘI ĐỒNG (DEFENSE Q&A SCRIPT)

Dưới đây là kịch bản trả lời chuẩn chỉ dành cho sinh viên khi Hội đồng đặt câu hỏi phản biện:

---

### CÂU HỎI 0: *"Tại sao nhóm lại chọn .NET Framework 4.6.1 thay vì .NET 6/7/8 mới hơn?"*
- **Tâm thế:** Điềm tĩnh, phân tích rõ tính tương thích và yêu cầu học thuật.
- **Lời thoại trả lời:**
  > *"Dạ thưa Thầy/Cô, .NET Framework 4.6.1 là nền tảng được tích hợp sẵn mặc định trong hệ điều hành Windows từ Windows 10, giúp người dùng cuối có thể chạy ngay tệp thực thi mà không cần cài thêm .NET Runtime bên ngoài.*
  >
  > *Đồng thời, đây là nền tảng chuẩn mực được quy định trong đề cương môn học nhằm đánh giá sâu sắc kiến trúc gốc của Windows Presentation Foundation (WPF), WCF/OWIN và các thư viện xử lý âm thanh bản địa như NAudio. Toàn bộ mã nguồn của nhóm đều được viết theo chuẩn Clean Code, sẵn sàng để port sang .NET 8/9 trong tương lai khi có yêu cầu."*

---

### CÂU HỎI 1: *"Tại sao phải nhúng một máy chủ Local BFF (OWIN) ngay trong app desktop mà không gọi trực tiếp các API đám mây từ ViewModel?"*
- **Tâm thế:** Nhấn mạnh vào 3 lợi thế kiến trúc: Streaming Proxy, DTO Abstraction và Cache/Search tập trung.
- **Lời thoại trả lời:**
  > *"Dạ thưa Thầy/Cô, việc nhúng Local BFF mang lại 3 lợi thế kiến trúc lớn:
  > - **Thứ nhất:** Nó đóng vai trò là một Streaming Proxy hỗ trợ chuẩn HTTP 206 Range Requests, giúp chia nhỏ luồng âm thanh để người dùng tua bài tức thời mà không cần nạp cả tệp MP3 lớn vào RAM.
  > - **Thứ hai:** Nó giúp trừu tượng hóa các nguồn nhạc khác nhau (Jamendo, Nhạc Việt, sau này là SoundCloud/YouTube) về một schema DTO đồng nhất, giúp ViewModel không bị phụ thuộc vào API của bên thứ ba.
  > - **Thứ ba:** Nó cho phép áp dụng bộ nhớ đệm MemoryCache nội bộ và giải thuật chuẩn hóa tìm kiếm tiếng Việt không dấu một cách tập trung, giúp giảm tải mạng và tăng tốc độ phản hồi."*

---

### CÂU HỎI 2: *"Làm thế nào để đảm bảo luồng giao diện WPF không bị giật lag (freeze) khi phát nhạc và hiển thị visualizer liên tục?"*
- **Tâm thế:** Trình bày giải pháp phân tách 5 luồng độc lập và kỹ thuật Event Throttling.
- **Lời thoại trả lời:**
  > *"Dạ thưa Thầy/Cô, nhóm đã cô lập triệt để 5 luồng hoạt động riêng biệt:
  > - Luồng âm thanh phần cứng chạy độc lập trong `WaveOutEvent` của NAudio;
  > - Luồng I/O mạng chạy trên Web API Thread Pool của OWIN;
  > - Luồng CSDL chạy trên các background task với SQLite WAL;
  > - Và luồng giao diện Dispatcher chỉ nhận dữ liệu đã được tiết lưu (throttled).
  >
  > *Cụ thể với visualizer FFT, thay vì cập nhật liên tục hàng trăm lần/giây, nhóm khống chế tần suất phát sự kiện ở mức đúng **33ms (tương đương 30 FPS)**. Nhờ đó, luồng giao diện luôn duy trì mượt mà ở mức 60 FPS mà mức chiếm dụng CPU chỉ dao động quanh 2%."*

---

### CÂU HỎI 3: *"Tại sao lại dùng phân phối xác suất Boltzmann trong Smart Shuffle mà không dùng giải thuật xáo trộn thông thường như Fisher-Yates?"*
- **Tâm thế:** Phân tích sự khác biệt giữa xáo trộn ngẫu nhiên đều và đề xuất cá nhân hóa.
- **Lời thoại trả lời:**
  > *"Dạ thưa Thầy/Cô, giải thuật Fisher-Yates Shuffle giả định tất cả các bài hát đều có xác suất xuất hiện ngang nhau (phân phối đều $1/N$). Tuy nhiên trong thực tế người dùng nghe nhạc, có những bài họ rất thích nghe và có những bài họ luôn bấm bỏ qua.*
  >
  > *Thuật toán Boltzmann/Softmax cho phép chuyển đổi điểm yêu thích thực tế (Affinity Score) thành trọng số xác suất. Nhờ đó, các bài hát người dùng hay nghe trọn vẹn hoặc thả tim sẽ có xác suất được chọn cao hơn, đồng thời tham số nhiệt độ $T$ vẫn mở ra cơ hội để khám phá các bài hát mới, tạo nên trải nghiệm cá nhân hóa thông minh."*

---

### CÂU HỎI 4: *"Tại sao nhóm lại dùng SQLite WAL mà không dùng LocalDB của SQL Server hoặc tệp JSON?"*
- **Tâm thế:** So sánh tính gọn nhẹ, tính độc lập và khả năng chịu tải đa luồng.
- **Lời thoại trả lời:**
  > *"Dạ thưa Thầy/Cô:
  > - Nếu dùng tệp JSON, mỗi khi thêm một bài hát vào playlist hay cập nhật điểm tương tác ta lại phải đọc/ghi toàn bộ tệp, rất chậm và dễ bị mất dữ liệu khi ứng dụng tắt đột ngột.
  > - Nếu dùng SQL Server LocalDB thì người dùng máy tính bắt buộc phải cài dịch vụ SQL Server rất nặng nề và phức tạp.
  >
  > *SQLite là giải pháp serverless tự đóng gói nhỏ gọn chỉ vài Megabytes, và đặc biệt chế độ **WAL (Write-Ahead Logging)** cho phép các luồng đọc và luồng ghi hoạt động đồng thời mà không bao giờ bị xung đột khóa dữ liệu ('database is locked')."*

---

### CÂU HỎI 5: *"Kỹ thuật Zero Heap Allocation trong Audio Engine hoạt động ra sao và đem lại lợi ích gì?"*
- **Tâm thế:** Trình bày nguyên lý bộ nhớ Heap/GC thế hệ 0 và phương thức `SetPeakingEq`.
- **Lời thoại trả lời:**
  > *"Dạ thưa Thầy/Cô, trong luồng xử lý âm thanh, phương thức `Read(buffer, offset, count)` được gọi liên tục mỗi vài phần nghìn giây. Nếu mỗi lần người dùng kéo thanh trượt Equalizer ta lại khởi tạo một đối tượng bộ lọc mới bằng từ khóa `new`, các đối tượng cũ sẽ chất đống trên bộ nhớ Heap thế hệ 0 (Gen 0).*
  >
  > *Khi bộ thu gom rác Garbage Collector của .NET kích hoạt dọn dẹp, nó sẽ tạm dừng luồng tiến trình (Stop-the-world), gây ra hiện tượng khựng âm thanh (audio click/pop).*
  >
  > *Kỹ thuật **Zero Heap Allocation** của nhóm là chỉ thay đổi các biến hệ số số thực $a_0, a_1, b_1...$ trực tiếp trên vùng nhớ của thể hiện bộ lọc hiện hữu thông qua phương thức `SetPeakingEq()`, hoàn toàn không cấp phát ô nhớ mới, đảm bảo luồng âm thanh chạy liên tục và êm ái."*

---

## IV. PHỤ LỤC: KỊCH BẢN THAO TÁC LIVE DEMO TRONG 3 PHÚT

Khi chuyển sang phần demo thực tế trên máy tính, sinh viên nên thực hiện tuần tự 5 bước sau:

1. **Bước 1 (30s) - Khởi động & Phát nhạc Online:**
   - Mở ứng dụng `MusicApp.exe` (chỉ vào thời gian khởi động tức thì ~1.2s).
   - Tìm kiếm bài hát tiếng Việt không dấu: Gõ `"con mua ngang qua"` -> Chọn phát bài -> Chỉ vào đĩa than bắt đầu xoay và các cột sóng FFT nhảy theo nhạc.
2. **Bước 2 (30s) - Thao tác 10-Band EQ & Presets:**
   - Mở giao diện Equalizer -> Chọn Preset `Bass Boost` -> Chỉ vào âm trầm tăng rõ rệt mà âm thanh không hề bị giật cục hay méo tiếng (nhờ Zero Heap Allocation & Soft Limiter).
3. **Bước 3 (45s) - Tua nhạc HTTP 206 & Đồng bộ Karaoke:**
   - Kéo Seekbar tua bài hát tới phút thứ 2 -> Chỉ vào âm thanh phát tiếp tức thời (< 80ms, không lag).
   - Mở tab Lyrics -> Cho Hội đồng thấy lời bài hát Karaoke cuộn mượt và highlight đúng nhịp hát của ca sĩ.
4. **Bước 4 (30s) - Quét thư viện Offline & Kéo thả hàng đợi:**
   - Bấm quét thư mục nhạc trên máy tính -> Chỉ vào tốc độ nạp bài cực nhanh và ảnh bìa hiển thị sắc nét.
   - Mở tab Queue -> Kéo thả đổi vị trí bài hát trong danh sách Up Next.
5. **Bước 5 (15s) - Smart Shuffle & Đổi Theme:**
   - Bật nút Smart Shuffle -> Thả tim bài hát để thấy điểm Affinity Score tăng -> Đổi sang Light Theme rồi quay về Dark Theme.
