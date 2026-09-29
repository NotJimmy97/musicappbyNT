# -*- coding: utf-8 -*-
"""
chapter1.py
Sinh nội dung CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI VÀ CÔNG NGHỆ ÁP DỤNG
- 1.1. Đặt vấn đề và Mục tiêu đề tài
- 1.2. Khảo sát nghiệp vụ và Yêu cầu hệ thống
- 1.3. Cơ sở công nghệ và Môi trường phát triển
Gồm Bảng 1.1, 1.2, 1.3, 1.4 và Hình 1.1, 1.2.
"""

def render_chapter_1(builder, md_lines):
    builder.add_heading_1("CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI VÀ CÔNG NGHỆ ÁP DỤNG", page_break=True)
    md_lines.append("\n---\n\n# CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI VÀ CÔNG NGHỆ ÁP DỤNG\n\n")

    # =========================================================================
    # 1.1. ĐẶT VẤN ĐỀ VÀ MỤC TIÊU ĐỀ TÀI
    # =========================================================================
    builder.add_heading_2("1.1. Đặt vấn đề và Mục tiêu đề tài")
    md_lines.append("## 1.1. Đặt vấn đề và Mục tiêu đề tài\n\n")

    builder.add_heading_3("1.1.1. Bối cảnh và Tính cấp thiết của đề tài")
    md_lines.append("### 1.1.1. Bối cảnh và Tính cấp thiết của đề tài\n\n")

    p1 = (
        "Trong kỷ nguyên số hóa hiện nay, nhu cầu thưởng thức âm nhạc đa phương tiện trên máy tính để bàn (Desktop PC) "
        "đã trở thành một phần thiết yếu trong học tập, làm việc và giải trí hàng ngày của người dùng. Sự bùng nổ của các nền tảng "
        "nghe nhạc trực tuyến toàn cầu như Spotify, Apple Music, YouTube Music đã định hình thói quen tìm kiếm và nghe nhạc trực tuyến "
        "nhờ vào kho nhạc trực tuyến khổng lồ. Tuy nhiên, khi khảo sát sâu vào thực trạng các ứng dụng phát nhạc trên môi trường Desktop, "
        "nhiều bất cập kỹ thuật và trải nghiệm người dùng lớn đã bộc lộ rõ nét:"
    )
    builder.add_paragraph(p1)
    md_lines.append(p1 + "\n\n")

    builder.add_bullet_point(
        "Sự cồng kềnh và ngốn tài nguyên của ứng dụng nền Web/Electron: Hầu hết các ứng dụng Desktop hiện đại (điển hình như Spotify Desktop) "
        "thực chất là một phiên bản Chromium rút gọn chạy trên nền Electron. Điều này dẫn đến mức tiêu thụ bộ nhớ RAM rất lớn "
        "(thường xuyên chiếm từ 450 MB đến hơn 1 GB RAM ngay cả khi chạy nền), thời gian khởi động lâu và khả năng tương tác trực tiếp "
        "với hệ thống âm thanh phần cứng Windows bị hạn chế đáng kể.",
        bold_prefix="Thứ nhất: "
    )
    builder.add_bullet_point(
        "Sự phân mảnh nghiêm trọng giữa nguồn nhạc Offline và Online: Người yêu âm nhạc thường sở hữu một bộ sưu tập nhạc cá nhân chất lượng cao "
        "(FLAC, WAV, MP3 320kbps) lưu trữ trên ổ cứng để nghe ngoại tuyến với độ trễ bằng không. Trong khi đó, các trình phát nhạc truyền thống "
        "(Windows Media Player, Groove Music) chỉ quản lý tệp cục bộ mà thiếu khả năng tìm kiếm và phát trực tuyến; ngược lại, các ứng dụng stream "
        "lại gây khó khăn khi tích hợp và lập chỉ mục kho nhạc ngoại tuyến sẵn có.",
        bold_prefix="Thứ hai: "
    )
    builder.add_bullet_point(
        "Thiếu vắng công cụ xử lý âm thanh kỹ thuật số chuyên sâu (DSP Equalizer): Phần lớn các phần mềm phát nhạc phổ thông chỉ phát tín hiệu âm thanh "
        "ở mức phẳng (Flat Response), không cung cấp bộ cân bằng âm sắc nhiều băng tần thời gian thực (Real-time Parametric/Graphic Equalizer). "
        "Điều này khiến người nghe không thể bù trừ đáp tuyến tần số cho các thiết bị ngoại vi (tai nghe, loa kiểm âm) hoặc điều chỉnh âm sắc theo gu thưởng thức riêng (Bass Boost, Vocal, Rock, Jazz).",
        bold_prefix="Thứ ba: "
    )
    builder.add_bullet_point(
        "Đồng bộ lời bài hát (Karaoke Lyrics) rời rạc: Trải nghiệm nghe nhạc hiện đại đòi hỏi khả năng hiển thị lời bài hát đồng bộ theo từng mili-giây "
        "dưới dạng Karaoke (tệp định dạng .LRC). Đa số trình phát ngoại tuyến không hỗ trợ tính năng này hoặc phân tích cú pháp rất chậm khi tệp lời dài.",
        bold_prefix="Thứ tư: "
    )

    md_lines.append("- **Thứ nhất (Tài nguyên cồng kềnh):** Các ứng dụng Electron tiêu tốn từ 450MB - 1GB RAM, khởi động chậm chạp.\n")
    md_lines.append("- **Thứ hai (Phân mảnh nguồn nhạc):** Tách rời kho nhạc cục bộ chất lượng cao (FLAC/WAV) và nhạc trực tuyến CDN.\n")
    md_lines.append("- **Thứ ba (Thiếu DSP Equalizer):** Không có bộ cân bằng âm sắc 10 băng tần thời gian thực để bù trừ loa/tai nghe.\n")
    md_lines.append("- **Thứ tư (Lời bài hát rời rạc):** Thiếu khả năng đồng bộ lời Karaoke .LRC tốc độ cao theo nhịp thời gian thực.\n\n")

    p2 = (
        "Xuất phát từ thực trạng trên, việc nghiên cứu và xây dựng một ứng dụng phát nhạc Desktop Native mang tên **MusicApp** trên nền tảng "
        "Microsoft .NET Framework và công nghệ Windows Presentation Foundation (WPF) là một đòi hỏi vô cùng cấp thiết. Ứng dụng tận dụng sức mạnh "
        "tăng tốc đồ họa phần cứng DirectX của WPF, kết hợp cùng Audio Engine chuyên nghiệp NAudio và hệ thống cơ sở dữ liệu nhúng SQLite, "
        "giải quyết triệt để bài toán dung lượng nhẹ, khởi động tức thì, hỗ trợ đa nguồn âm thanh và cá nhân hóa trải nghiệm âm học đỉnh cao."
    )
    builder.add_paragraph(p2)
    md_lines.append(p2 + "\n\n")

    builder.add_heading_3("1.1.2. Mục tiêu nghiên cứu và phát triển ứng dụng")
    md_lines.append("### 1.1.2. Mục tiêu nghiên cứu và phát triển ứng dụng\n\n")

    p3 = "Mục tiêu tổng quát của đề tài là thiết kế và hiện thực hóa hoàn chỉnh phần mềm phát nhạc Desktop Native chất lượng cao với các mục tiêu cụ thể sau:"
    builder.add_paragraph(p3)
    md_lines.append(p3 + "\n\n")

    builder.add_bullet_point("Về mặt kiến trúc phần mềm: Xây dựng giải pháp theo mô hình Modular Monolith kết hợp kiến trúc phân tầng chuẩn mực (Presentation, Core Domain, Audio Engine, Local BFF). Áp dụng triệt để mẫu thiết kế MVVM (Model-View-ViewModel) tách rời tuyệt đối giao diện XAML khỏi logic điều khiển; tích hợp máy chủ cổng ngầm Backend-For-Frontend (BFF) chạy trên OWIN Self-Host nhằm chuẩn hóa luồng stream và bảo mật danh tính nguồn cấp.", bold_prefix="1. Kiến trúc: ")
    builder.add_bullet_point("Về mặt xử lý âm thanh kỹ thuật số: Xây dựng Audio Engine dựa trên thư viện NAudio 1.10.0, hiện thực bộ lọc số IIR BiQuad Peaking EQ 10 băng tần chuẩn ISO (31Hz đến 16kHz) với độ trễ cực thấp (< 50ms), đồng thời tích hợp thuật toán biến đổi Fourier nhanh (1024-point FFT) trích xuất phổ tần số 16 cột phục vụ hiệu ứng trực quan hóa âm thanh sống động.", bold_prefix="2. Audio Engine: ")
    builder.add_bullet_point("Về mặt lưu trữ và bộ nhớ đệm: Triển khai mô hình lưu trữ đa nguồn học hỏi từ Spotify với cơ chế định danh hợp nhất (Unified Track Identity), bộ nhớ đệm tệp nhị phân Content-Addressable Storage (CAS) kết hợp giải thuật dọn dẹp LRU (Least Recently Used), cho phép phát lại tức thì trong 5ms từ ổ cứng ngoại tuyến.", bold_prefix="3. Persistence & Cache: ")
    builder.add_bullet_point("Về mặt thuật toán thông minh: Xây dựng hệ thống gợi ý bài hát cục bộ (Client-Side Recommendation Engine) dựa trên ma trận phản hồi ngầm định (Implicit Feedback Matrix) tính toán điểm số Affinity Score, hỗ trợ tính năng Radio bài hát và thuật toán xáo trộn thông minh (Smart Shuffle theo phân phối Boltzmann).", bold_prefix="4. Trí thông minh cục bộ: ")

    md_lines.append("1. **Kiến trúc:** Phân tầng chuẩn mực MVVM + Local BFF OWIN, tách rời giao diện XAML và logic điều khiển.\n")
    md_lines.append("2. **Audio Engine:** NAudio 1.10.0, bộ lọc IIR BiQuad 10 băng tần (<50ms delay), FFT 1024-point trích xuất 16 cột phổ.\n")
    md_lines.append("3. **Persistence & Cache:** SQLite WAL Mode, CAS Disk Cache 2 tầng, dọn dẹp LRU, phát lại offline tức thì sau 5ms.\n")
    md_lines.append("4. **Thuật toán gợi ý:** Client-side Affinity Scoring, Radio bài hát, Smart Shuffle theo phân phối Boltzmann.\n\n")

    builder.add_heading_3("1.1.3. Phạm vi đề tài và giới hạn hệ thống")
    md_lines.append("### 1.1.3. Phạm vi đề tài và giới hạn hệ thống\n\n")

    p_scope = (
        "Để đảm bảo tính khả thi và tập trung tối đa vào chất lượng kỹ thuật trong khuôn khổ môn học, phạm vi của đồ án được xác định rõ ràng như sau:"
    )
    builder.add_paragraph(p_scope)
    md_lines.append(p_scope + "\n\n")

    builder.add_paragraph("Các nội dung và chức năng thuộc phạm vi đồ án:", bold_prefix="Phạm vi hiện thực (In-Scope): ")
    builder.add_bullet_point("Môi trường thực thi: Hệ điều hành Microsoft Windows (Windows 7 SP1, Windows 8.1, Windows 10, Windows 11) trên nền .NET Framework 4.6.1.")
    builder.add_bullet_point("Định dạng âm thanh hỗ trợ: Phát giải mã hoàn chỉnh các tệp âm thanh định dạng MPEG Audio Layer III (.mp3), Waveform (.wav), và Free Lossless Audio Codec (.flac).")
    builder.add_bullet_point("Nguồn cấp dữ liệu trực tuyến: Tích hợp nguồn nhạc mở Creative Commons qua Jamendo API và kho siêu dữ liệu Archive.org thông qua bộ định tuyến MusicSourceRouter.")
    builder.add_bullet_point("Lưu trữ dữ liệu: Cơ sở dữ liệu SQLite cục bộ được nhúng trực tiếp (%LOCALAPPDATA%\\MusicApp\\musicapp.db) với cơ chế WAL Mode và Repository Pattern.")

    builder.add_paragraph("Các nội dung nằm ngoài phạm vi đồ án (sẽ nghiên cứu trong tương lai):", bold_prefix="Giới hạn hệ thống (Out-of-Scope): ")
    builder.add_bullet_point("Chưa hỗ trợ đồng bộ dữ liệu đám mây (Cloud Sync) giữa nhiều máy tính của cùng một người dùng.")
    builder.add_bullet_point("Chưa xây dựng phiên bản di động (Mobile App iOS / Android) hoặc phiên bản đa nền tảng (macOS / Linux).")
    builder.add_bullet_point("Chưa hỗ trợ phát trực tiếp các định dạng âm thanh độc quyền đòi hỏi giải mã phần cứng phức tạp như DSD (Direct Stream Digital) hay MQA.")
    builder.add_bullet_point("Không tích hợp cổng thanh toán thương mại điện tử mua bản quyền bài hát trực tuyến.")

    md_lines.append("#### Phạm vi hiện thực (In-Scope):\n")
    md_lines.append("- Windows Desktop (Windows 7/8/10/11), .NET Framework 4.6.1.\n")
    md_lines.append("- Hỗ trợ định dạng MP3, WAV, FLAC.\n")
    md_lines.append("- Nguồn nhạc trực tuyến Jamendo API + Archive.org qua BFF Router.\n")
    md_lines.append("- SQLite nhúng nội bộ WAL Mode (%LOCALAPPDATA%\\MusicApp\\musicapp.db).\n\n")
    md_lines.append("#### Giới hạn hệ thống (Out-of-Scope):\n")
    md_lines.append("- Chưa đồng bộ Cloud Sync đa thiết bị.\n")
    md_lines.append("- Chưa phát triển phiên bản Mobile (iOS/Android) hay macOS/Linux.\n")
    md_lines.append("- Chưa giải mã phần cứng DSD/MQA.\n")
    md_lines.append("- Không tích hợp cổng thanh toán bản quyền.\n\n")

    # =========================================================================
    # 1.2. KHẢO SÁT NGHIỆP VỤ VÀ YÊU CẦU HỆ THỐNG
    # =========================================================================
    builder.add_heading_2("1.2. Khảo sát nghiệp vụ và Yêu cầu hệ thống")
    md_lines.append("## 1.2. Khảo sát nghiệp vụ và Yêu cầu hệ thống\n\n")

    builder.add_heading_3("1.2.1. Khảo sát quy trình nghiệp vụ phát nhạc đa nguồn")
    md_lines.append("### 1.2.1. Khảo sát quy trình nghiệp vụ phát nhạc đa nguồn\n\n")

    p_flow = (
        "Quy trình vận hành thực tế của MusicApp bao gồm 5 luồng nghiệp vụ cốt lõi, tương tác liên hoàn giữa giao diện người dùng, "
        "cổng BFF cục bộ, Audio Engine và hệ cơ sở dữ liệu SQLite:"
    )
    builder.add_paragraph(p_flow)
    md_lines.append(p_flow + "\n\n")

    builder.add_bullet_point(
        "Người dùng nhập từ khóa tìm kiếm trên thanh điều hướng. Hệ thống áp dụng cơ chế Debounce 300ms nhằm ngăn ngừa bão hòa request. "
        "Yêu cầu được gửi tới Local BFF tại cổng loopback :5245. BFF định tuyến tới Jamendo/Archive.org, chuẩn hóa siêu dữ liệu thành SearchResultDto, "
        "ghi nhớ vào MemoryCache và trả về giao diện để người dùng chọn bài hát.",
        bold_prefix="Luồng 1 - Tìm kiếm bài hát trực tuyến: "
    )
    builder.add_bullet_point(
        "Khi người dùng kích hoạt một bài hát trực tuyến, Audio Engine khởi tạo kết nối stream qua endpoint proxy của BFF. "
        "BFF sử dụng HTTP Range Request (mã phản hồi 206 Partial Content) để lấy từng khối byte dữ liệu âm thanh từ CDN. "
        "Song song với việc đẩy dữ liệu vào bộ đệm BufferedWaveProvider của NAudio để giải mã PCM, một tác vụ nền tự động ghi các khối byte "
        "vào tệp nhị phân trên đĩa (%LOCALAPPDATA%\\MusicApp\\Cache\\{hash}.audio). Lần nghe sau, ứng dụng phát thẳng từ đĩa mà không tốn mạng.",
        bold_prefix="Luồng 2 - Phát nhạc trực tuyến kết hợp ghi đệm CAS: "
    )
    builder.add_bullet_point(
        "Người dùng chọn một hoặc nhiều thư mục trên ổ cứng. Ứng dụng kích hoạt dịch vụ quét sử dụng giải thuật duyệt cây theo chiều rộng (BFS). "
        "Hệ thống lần lượt mở từng tệp, sử dụng thư viện TagLibSharp trích xuất thẻ ID3 (Tiêu đề, Nghệ sĩ, Album, Năm phát hành, Ảnh bìa, Thời lượng). "
        "Một chuỗi mã băm Fingerprint Key (Normalized Title + '::' + Normalized Artist) được sinh ra để chống trùng lặp dữ liệu trước khi lưu vào SQLite.",
        bold_prefix="Luồng 3 - Quét và lập chỉ mục thư viện cục bộ (Local Scan): "
    )
    builder.add_bullet_point(
        "Trong suốt quá trình phát nhạc, Audio Engine định kỳ phát tín hiệu thời gian phát hiện tại (mỗi 30 mili-giây). "
        "Module LrcParser phân tích tệp .LRC thành danh sách các mốc thời gian kèm câu lời. Giải thuật tìm kiếm nhị phân O(log N) "
        "xác định chính xác dòng lời hiện tại và tự động cuộn giao diện hiển thị, tạo hiệu ứng hát Karaoke mượt mà.",
        bold_prefix="Luồng 4 - Đồng bộ lời bài hát Karaoke (Lyrics Synchronization): "
    )
    builder.add_bullet_point(
        "Người dùng tùy ý điều chỉnh thanh trượt của 10 băng tần Equalizer (+/- 12dB). Tín hiệu điều khiển được chuyển thành hệ số lọc số "
        "cho 20 bộ lọc IIR BiQuad (10 băng tần cho mỗi kênh Stereo Trái/Phải). Người dùng có thể lưu cấu hình âm thanh ưa thích thành các Preset "
        "trong cơ sở dữ liệu SQLite để tái sử dụng.",
        bold_prefix="Luồng 5 - Cân bằng âm sắc kỹ thuật số (DSP Equalizer): "
    )

    md_lines.append("- **Luồng 1 (Tìm kiếm trực tuyến):** Debounce 300ms -> Local BFF :5245 -> Router Jamendo/Archive -> MemoryCache -> UI.\n")
    md_lines.append("- **Luồng 2 (Phát trực tuyến & CAS Cache):** HTTP 206 Range -> NAudio BufferedWaveProvider -> Background CAS file write -> 0ms replay offline.\n")
    md_lines.append("- **Luồng 3 (Quét thư viện BFS):** BFS lặp duyệt thư mục -> TagLibSharp trích thẻ ID3 -> Deduplication Key -> SQLite tracks table.\n")
    md_lines.append("- **Luồng 4 (Đồng bộ lời bài hát):** Dispatcher timer 30ms -> LrcParser Binary Search O(log N) -> Auto-scroll Karaoke UI.\n")
    md_lines.append("- **Luồng 5 (DSP Equalizer):** 10 thanh trượt +/-12dB -> Cập nhật hệ số 20 BiQuad filters (Stereo) -> Lưu SQLite eq_presets.\n\n")

    builder.add_heading_3("1.2.2. Phân tích yêu cầu chức năng (Functional Requirements)")
    md_lines.append("### 1.2.2. Phân tích yêu cầu chức năng (Functional Requirements)\n\n")

    p_fr_intro = "Các yêu cầu chức năng của hệ thống được chuẩn hóa và phân rã chi tiết trong Bảng 1.1:"
    builder.add_paragraph(p_fr_intro)
    md_lines.append(p_fr_intro + "\n\n")

    fr_data = [
        ["FR-01", "Tìm kiếm bài hát trực tuyến", "Cho phép nhập từ khóa tìm kiếm theo tên bài hát, nghệ sĩ; hỗ trợ kỹ thuật Debounce 300ms chống nghẽn; hiển thị kết quả trực quan."],
        ["FR-02", "Phát luồng âm thanh trực tuyến", "Hỗ trợ phát luồng stream MPEG Audio từ CDN qua giao thức HTTP 206 Range; tự động ghi cache nhị phân vào ổ cứng cục bộ."],
        ["FR-03", "Điều khiển phát nhạc (Playback)", "Cung cấp đầy đủ các chức năng Play, Pause, Stop, Seek (tua bài theo mili-giây), điều chỉnh âm lượng (0% - 100%), chuyển bài Kế tiếp / Lùi lại."],
        ["FR-04", "Quản lý Hàng đợi (Play Queue)", "Duy trì danh sách các bài hát chuẩn bị phát; hỗ trợ sắp xếp lại vị trí bài hát, xóa bài khỏi hàng đợi, tự động chuyển bài tiếp theo khi hết bài."],
        ["FR-05", "Quét thư viện cục bộ (Local Scan)", "Cho phép chọn thư mục trên máy tính; tự động quét đệ quy an toàn bằng BFS; trích xuất thẻ siêu dữ liệu ID3/Vorbis; lưu vào SQLite."],
        ["FR-06", "Quản lý Danh sách phát (Playlist)", "Cho phép tạo mới, đổi tên, xóa Playlist cá nhân; thêm và xóa bài hát khỏi Playlist; duy trì thứ tự bài hát theo trường position."],
        ["FR-07", "Đánh dấu bài hát Yêu thích", "Cho phép người dùng bấm nút Trái tim (Favorite) để đưa bài hát vào danh mục yêu thích; tự động cộng điểm trọng số tương tác."],
        ["FR-08", "Bộ cân bằng âm sắc DSP Equalizer", "Tùy chỉnh 10 dải tần âm thanh độc lập (+/- 12dB); hỗ trợ các bộ mẫu Preset có sẵn (Rock, Pop, Jazz, Bass Boost) và tạo Preset tùy chỉnh."],
        ["FR-09", "Đồng bộ lời bài hát Karaoke", "Tự động tìm kiếm và nạp tệp lời bài hát định dạng .LRC; làm nổi bật dòng lời đang phát và tự động cuộn giao diện mượt mà."],
        ["FR-10", "Gợi ý bài hát & Smart Shuffle", "Tính toán điểm số quan hệ (Affinity Score); tự động phát tiếp các bài tương đồng khi hết hàng đợi; xáo trộn thông minh theo phân phối Boltzmann."]
    ]

    builder.add_table(
        "Bảng 1.1: Bảng phân tích yêu cầu chức năng hệ thống (Functional Requirements)",
        ["Mã YC", "Tên chức năng", "Mô tả chi tiết nghiệp vụ"],
        fr_data
    )

    md_lines.append("| Mã YC | Tên chức năng | Mô tả chi tiết nghiệp vụ |\n|---|---|---|\n")
    for code, name, desc in fr_data:
        md_lines.append(f"| **{code}** | {name} | {desc} |\n")
    md_lines.append("\n")

    builder.add_heading_3("1.2.3. Phân tích yêu cầu phi chức năng (Non-Functional Requirements)")
    md_lines.append("### 1.2.3. Phân tích yêu cầu phi chức năng (Non-Functional Requirements)\n\n")

    p_nfr_intro = (
        "Bên cạnh các tính năng nghiệp vụ, tính ổn định và hiệu năng cao là tiêu chí sống còn của một ứng dụng Desktop Native. "
        "Các yêu cầu phi chức năng được định lượng khắt khe trong Bảng 1.2:"
    )
    builder.add_paragraph(p_nfr_intro)
    md_lines.append(p_nfr_intro + "\n\n")

    nfr_data = [
        ["NFR-01", "Độ trễ xử lý âm thanh (Audio Latency)", "Độ trễ xử lý qua chuỗi bộ lọc DSP Equalizer phải nhỏ hơn 50 mili-giây, đảm bảo phản hồi tức thì khi người dùng di chuyển thanh trượt EQ."],
        ["NFR-02", "Zero-Allocation Audio Thread", "Không phát sinh cấp phát bộ nhớ rác (GC Allocation) trong vòng lặp đọc mẫu PCM của Audio Thread nhằm triệt tiêu hoàn toàn hiện tượng khựng tiếng (audio stutter)."],
        ["NFR-03", "Ảo hóa Giao diện (UI Virtualization)", "Bật chế độ VirtualizingStackPanel với chế độ tái sử dụng (Recycling) trên toàn bộ danh sách, đảm bảo ứng dụng cuộn mượt mà ngay cả khi thư viện có hơn 20.000 bài hát."],
        ["NFR-04", "Chống rò rỉ bộ nhớ đồ họa (Frozen Images)", "Toàn bộ hình ảnh ảnh bìa Album (BitmapImage) phải được gọi phương thức Freeze() để tách quyền sở hữu luồng, cho phép Garbage Collector thu hồi vùng nhớ GPU."],
        ["NFR-05", "Độ an toàn và Toàn vẹn cơ sở dữ liệu", "Hệ cơ sở dữ liệu SQLite phải được cấu hình chạy ở chế độ WAL (Write-Ahead Logging) kết hợp PRAGMA synchronous = NORMAL, bảo vệ dữ liệu không bị hỏng khi tắt máy đột ngột."]
    ]

    builder.add_table(
        "Bảng 1.2: Bảng phân tích yêu cầu phi chức năng hệ thống (Non-Functional Requirements)",
        ["Mã YC", "Tiêu chuẩn kỹ thuật", "Chỉ số định lượng & Cơ chế đảm bảo"],
        nfr_data
    )

    md_lines.append("| Mã YC | Tiêu chuẩn kỹ thuật | Chỉ số định lượng & Cơ chế đảm bảo |\n|---|---|---|\n")
    for code, name, desc in nfr_data:
        md_lines.append(f"| **{code}** | {name} | {desc} |\n")
    md_lines.append("\n")

    # =========================================================================
    # 1.3. CƠ SỞ CÔNG NGHỆ VÀ MÔI TRƯỜNG PHÁT TRIỂN
    # =========================================================================
    builder.add_heading_2("1.3. Cơ sở công nghệ và Môi trường phát triển")
    md_lines.append("## 1.3. Cơ sở công nghệ và Môi trường phát triển\n\n")

    builder.add_heading_3("1.3.1. Nền tảng .NET Framework 4.6.1 và Ngôn ngữ C# 7.3")
    md_lines.append("### 1.3.1. Nền tảng .NET Framework 4.6.1 và Ngôn ngữ C# 7.3\n\n")

    p_dotnet = (
        "Dự án được xây dựng dựa trên nền tảng **Microsoft .NET Framework 4.6.1** kết hợp cùng phiên bản ngôn ngữ **C# 7.3**. "
        "Việc lựa chọn phiên bản này mang lại lợi thế chiến lược về tính sẵn sàng và độ ổn định: .NET Framework 4.6.1 là thành phần mặc định "
        "có mặt trên tất cả các phiên bản hệ điều hành Microsoft Windows hiện hành (từ Windows 7 SP1, Windows 8.1 đến Windows 10 và Windows 11). "
        "Người dùng cuối có thể tải về và thực thi ngay tệp nhị phân của ứng dụng mà không cần cài đặt thêm gói runtime nặng nề nào khác."
    )
    builder.add_paragraph(p_dotnet)
    md_lines.append(p_dotnet + "\n\n")

    builder.add_paragraph("Các tính năng C# 7.3 cốt lõi được ứng dụng triệt để trong dự án bao gồm:")
    builder.add_bullet_point("Async / Await và Task-based Asynchronous Pattern (TAP): Giải phóng luồng giao diện (UI Thread) khỏi các tác vụ I/O đĩa và mạng, kết hợp cờ ConfigureAwait(false) ở tầng Core/BFF nhằm ngăn ngừa tình trạng Deadlock luồng.", bold_prefix="1. Bất đồng bộ hiện đại: ")
    builder.add_bullet_point("Pattern Matching & Tuples: Cho phép phân tích kiểu dữ liệu âm thanh và trạng thái phát nhạc một cách cô đọng, loại bỏ các chuỗi if-else dài dòng.", bold_prefix="2. Khớp mẫu cú pháp: ")
    builder.add_bullet_point("LINQ to Objects: Tối ưu hóa các thao tác lọc, tìm kiếm và sắp xếp danh sách bài hát trong bộ nhớ với cú pháp khai báo trong sáng.", bold_prefix="3. Truy vấn LINQ: ")
    builder.add_bullet_point("Expression-Bodied Members: Giảm thiểu độ dài mã nguồn cho các thuộc tính ViewModel và phương thức một dòng.", bold_prefix="4. Cú pháp rút gọn: ")

    builder.add_heading_3("1.3.2. Công nghệ giao diện WPF và Mô hình kiến trúc MVVM")
    md_lines.append("### 1.3.2. Công nghệ giao diện WPF và Mô hình kiến trúc MVVM\n\n")

    p_wpf = (
        "Windows Presentation Foundation (WPF) là framework phát triển giao diện Desktop hàng đầu của Microsoft. "
        "WPF sử dụng ngôn ngữ đánh dấu XAML để định nghĩa giao diện người dùng và tận dụng đường ống dựng hình phần cứng DirectX, "
        "giúp hiển thị mượt mà các hoạt hình phức tạp như hiệu ứng xoay đĩa than Vinyl và phổ âm thanh FFT thời gian thực."
    )
    builder.add_paragraph(p_wpf)
    md_lines.append(p_wpf + "\n\n")

    p_mvvm = (
        "Mô hình kiến trúc **Model-View-ViewModel (MVVM)** là chuẩn mực bất biến trong phát triển phần mềm WPF chuyên nghiệp. "
        "Kiến trúc MVVM phân tách ứng dụng thành ba thành phần độc lập:"
    )
    builder.add_paragraph(p_mvvm)
    md_lines.append(p_mvvm + "\n\n")

    builder.add_bullet_point("Model: Đại diện cho dữ liệu thực thể nghiệp vụ (TrackModel, PlaylistModel, LyricLine, PlaybackState). Model hoàn toàn không biết gì về giao diện người dùng.", bold_prefix="1. Model (Mô hình dữ liệu): ")
    builder.add_bullet_point("View: Giao diện hiển thị trực quan được khai báo bằng ngôn ngữ XAML. View liên kết với ViewModel thông qua cơ chế Data Binding và các lệnh Command, không chứa mã xử lý nghiệp vụ trong Code-Behind.", bold_prefix="2. View (Giao diện hiển thị): ")
    builder.add_bullet_point("ViewModel: Thành phần trung gian lưu trữ trạng thái hiển thị và chuyển giao dữ liệu từ Model tới View. ViewModel kế thừa ObservableObject (thực thi giao tiếp INotifyPropertyChanged) và đóng gói các hành động của người dùng qua RelayCommand / AsyncRelayCommand (thực thi giao tiếp ICommand).", bold_prefix="3. ViewModel (Trạng thái và Điều khiển): ")

    # Dẫn dắt Hình 1.2
    p_fig12_lead = "Mối quan hệ tương tác giữa View, ViewModel, Model và cơ chế điều phối Dispatcher của WPF được minh họa chi tiết trong Hình 1.2:"
    builder.add_paragraph(p_fig12_lead)
    md_lines.append(p_fig12_lead + "\n\n")

    schematic_mvvm = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|                              MÔ HÌNH LUỒNG DỮ LIỆU MVVM TRONG WPF                       |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "|      VIEW (XAML)       |         VIEWMODEL (C#)        |        MODEL & SERVICES        |\n"
        "|  - MainWindow.xaml     |  - MainViewModel.cs           |  - TrackModel.cs               |\n"
        "|  - NowPlayingCardView  |  - NowPlayingViewModel.cs     |  - IAudioService               |\n"
        "|  - DspEqualizerView    |  - DspEqualizerViewModel.cs   |  - ITrackRepository            |\n"
        "+------------------------+-------------------------------+--------------------------------+\n"
        "            |                           |                               |\n"
        "            |  <--- Data Binding ------ |                               |\n"
        "            |       (TwoWay/OneWay)     |                               |\n"
        "            |                           |                               |\n"
        "            |  --- ICommand (Click) --> |                               |\n"
        "            |       (RelayCommand)      |                               |\n"
        "            |                           |  --- Gọi nghiệp vụ / Query -> |\n"
        "            |                           |  <-- Trả về dữ liệu / Event - |\n"
        "            |                           |                               |\n"
        "            |  <--- Dispatcher.Invoke - |                               |\n"
        "            |       (Update UI Thread)  |                               |\n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 1.2: Mô hình luồng dữ liệu MVVM và tương tác Dispatcher trong WPF",
        schematic_mvvm,
        note="Sơ đồ tương tác Data Binding, ICommand và Dispatcher luồng giao diện"
    )
    md_lines.append("```\n" + schematic_mvvm + "\n```\n")
    md_lines.append("*Hình 1.2: Mô hình luồng dữ liệu MVVM và tương tác Dispatcher trong WPF*\n\n")

    builder.add_heading_3("1.3.3. Mô hình Local Backend-for-Frontend (BFF) qua OWIN Self-Host")
    md_lines.append("### 1.3.3. Mô hình Local Backend-for-Frontend (BFF) qua OWIN Self-Host\n\n")

    p_bff = (
        "Một trong những điểm sáng tạo kiến trúc nổi bật nhất của dự án là việc áp dụng mô hình **Local Backend-For-Frontend (BFF)**. "
        "Thay vì để ứng dụng WPF trực tiếp thực hiện các cuộc gọi mạng phân tán tới các nhà cung cấp bên ngoài, MusicApp khởi chạy "
        "một máy chủ Web API 2 siêu nhẹ (Microsoft.Owin.SelfHost 4.2.2) chạy ngầm ngay trong cùng tiến trình ứng dụng tại cổng loopback: "
        "http://localhost:5245."
    )
    builder.add_paragraph(p_bff)
    md_lines.append(p_bff + "\n\n")

    builder.add_paragraph("Lợi ích kỹ thuật vượt trội của mô hình Local BFF:")
    builder.add_bullet_point("Bảo vệ thông tin định danh (Credentials Isolation): Toàn bộ API Key, Client ID của các dịch vụ âm nhạc trực tuyến được lưu giữ và quản lý tại máy chủ cục bộ, không bị lộ ra ngoài giao diện người dùng.", bold_prefix="1. An toàn bảo mật: ")
    builder.add_bullet_point("Proxy chuyển tiếp HTTP Range (Audio Stream Forwarding): Khi phát nhạc trực tuyến, BFF đóng vai trò proxy hỗ trợ tiêu đề 'Range: bytes=start-end', cho phép Audio Engine tua bài hát tùy ý trên một luồng stream trực tuyến mà không bị đứt kết nối.", bold_prefix="2. Tua bài mượt mà: ")
    builder.add_bullet_point("Bộ đệm siêu dữ liệu (In-Memory Caching): Tích hợp System.Runtime.Caching.MemoryCache giúp giảm thiểu 80% số lượng request lặp lại đối với các truy vấn tìm kiếm phổ biến.", bold_prefix="3. Giảm tải mạng: ")
    builder.add_bullet_point("Trừu tượng hóa nhà cung cấp (Provider Agnostic): WPF Client chỉ giao tiếp với một API duy nhất (/api/v1/search, /api/v1/stream). Tầng BFF tự do hoán đổi hoặc mở rộng thêm nguồn nhạc mới mà không làm thay đổi bất kỳ dòng mã nào ở tầng Presentation.", bold_prefix="4. Mở rộng linh hoạt: ")

    builder.add_heading_3("1.3.4. Thư viện âm thanh NAudio và Xử lý tín hiệu số (DSP)")
    md_lines.append("### 1.3.4. Thư viện âm thanh NAudio và Xử lý tín hiệu số (DSP)\n\n")

    p_naudio = (
        "NAudio (phiên bản 1.10.0) là thư viện âm thanh mã nguồn mở tiêu chuẩn công nghiệp dành cho nền tảng .NET. "
        "MusicApp tận dụng NAudio để xây dựng đường ống xử lý tín hiệu kỹ thuật số (DSP Pipeline) hoàn chỉnh từ khâu giải mã, "
        "cân bằng âm sắc đến phân tích phổ âm thanh."
    )
    builder.add_paragraph(p_naudio)
    md_lines.append(p_naudio + "\n\n")

    builder.add_bullet_point("Bộ lọc số IIR BiQuad Peaking EQ: Hiện thực hóa theo công thức lọc số kinh điển của Robert Bristow-Johnson. Mỗi bộ lọc bậc 2 xử lý một băng tần trung tâm với hệ số phẩm chất Q = 1.414, cho phép tăng hoặc giảm biên độ mượt mà từ -12dB đến +12dB.", bold_prefix="1. Bộ cân bằng âm sắc 10 băng tần: ")
    builder.add_bullet_point("Phân tích phổ biến đổi Fourier nhanh (1024-point FFT): Mẫu âm thanh PCM được chuyển qua bộ tích lũy SampleAggregator (zero-allocation) để tính toán FFT 1024 điểm, phân chia thành 16 phổ tần số (Spectrum Bins) cung cấp dữ liệu cho giao diện trực quan hóa nhịp điệu.", bold_prefix="2. Trực quan hóa âm thanh FFT: ")
    builder.add_bullet_point("Đầu ra âm thanh WASAPI / DirectSound: Kết nối trực tiếp với hệ điều hành Windows ở chế độ chia sẻ (Shared Mode), mang lại chất lượng âm thanh nguyên bản không bị suy hao.", bold_prefix="3. Xuất âm thanh chất lượng cao: ")

    eq_spec_data = [
        ["Băng tần 1", "31 Hz", "Sub-Bass", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ sâu của tiếng trống trầm và âm trầm điện tử."],
        ["Băng tần 2", "62 Hz", "Bass", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Nhịp đập âm bass chính trong nhạc Dance / Pop."],
        ["Băng tần 3", "125 Hz", "Low Mid", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ ấm của giọng hát nam và tiếng guitar bass."],
        ["Băng tần 4", "250 Hz", "Mid-Range", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ dày thân âm thanh của nhạc cụ mộc."],
        ["Băng tần 5", "500 Hz", "Center Mid", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ rõ của giọng hát và nhạc cụ bộ hơi."],
        ["Băng tần 6", "1 kHz", "Upper Mid", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ nổi bật của giọng hát chính (Lead Vocal)."],
        ["Băng tần 7", "2 kHz", "Presence", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ sắc nét của phát âm và tiếng đàn guitar."],
        ["Băng tần 8", "4 kHz", "Clarity", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Độ sáng và chi tiết của giọng nữ cao."],
        ["Băng tần 9", "8 kHz", "Treble", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Tiếng chập chũm (Hi-hat), độ thanh thoát."],
        ["Băng tần 10", "16 kHz", "Brilliance", "Q = 1.414", "-12.0 dB đến +12.0 dB", "Không gian không khí (Airy), âm trường mở rộng."]
    ]

    builder.add_table(
        "Bảng 1.4: Thông số kỹ thuật bộ lọc cân bằng âm sắc 10 băng tần ISO",
        ["Băng", "Tần số trung tâm (f0)", "Dải âm thanh", "Hệ số Q", "Dải điều chỉnh Gain", "Ý nghĩa cảm thụ âm thanh"],
        eq_spec_data
    )

    md_lines.append("| Băng | Tần số trung tâm (f0) | Dải âm thanh | Hệ số Q | Dải điều chỉnh Gain | Ý nghĩa cảm thụ âm thanh |\n|---|---|---|---|---|---|\n")
    for b_idx, f0, band_name, q, gain, desc in eq_spec_data:
        md_lines.append(f"| **{b_idx}** | {f0} | {band_name} | {q} | {gain} | {desc} |\n")
    md_lines.append("\n")

    builder.add_heading_3("1.3.5. Hệ quản trị CSDL SQLite và Mô hình Repository Pattern")
    md_lines.append("### 1.3.5. Hệ quản trị CSDL SQLite và Mô hình Repository Pattern\n\n")

    p_sqlite = (
        "Đối với một ứng dụng phát nhạc Desktop Native, việc lựa chọn công nghệ lưu trữ dữ liệu đòi hỏi phải giải quyết hài hòa "
        "giữa tốc độ truy vấn tức thì, tính độc lập gọn nhẹ (không bắt người dùng cài đặt máy chủ CSDL phức tạp) và độ bền vững dữ liệu. "
        "Dự án sử dụng cơ sở dữ liệu nhúng **SQLite** thông qua gói thư viện chính thức System.Data.SQLite kết hợp công nghệ **ADO.NET thuần**."
    )
    builder.add_paragraph(p_sqlite)
    md_lines.append(p_sqlite + "\n\n")

    p_compare_lead = "Lý do lựa chọn ADO.NET thuần thay vì Entity Framework (EF) được phân tích và so sánh trong Bảng 1.3:"
    builder.add_paragraph(p_compare_lead)
    md_lines.append(p_compare_lead + "\n\n")

    db_compare_data = [
        ["Thời gian khởi động ban đầu", "Cực nhanh (< 20 mili-giây)", "Chậm (500ms - 1.5s để sinh Model metadata)"],
        ["Mức tiêu thụ bộ nhớ RAM", "Rất thấp (~ 4 MB RAM)", "Cao (~ 45 - 80 MB RAM do Change Tracker)"],
        ["Tốc độ nạp 10.000 bài hát", "~ 150 mili-giây (Data Reader trực tiếp)", "~ 1.800 mili-giây (Object Materialization overhead)"],
        ["Kiểm soát câu lệnh SQL", "100% kiểm soát trực tiếp, tối ưu hóa chỉ mục", "Sinh mã gián tiếp qua LINQ to Entities"],
        ["Độ tin cậy trong Desktop App", "Tuyệt đối không gây lag hoặc giật giao diện", "Dễ gây nghẽn luồng nếu cấu hình không chuẩn"]
    ]

    builder.add_table(
        "Bảng 1.3: So sánh đặc tính kỹ thuật: ADO.NET thuần vs. Entity Framework",
        ["Tiêu chí kỹ thuật", "ADO.NET thuần + SQLite (MusicApp)", "Entity Framework Core / 6.x"],
        db_compare_data
    )

    md_lines.append("| Tiêu chí kỹ thuật | ADO.NET thuần + SQLite (MusicApp) | Entity Framework Core / 6.x |\n|---|---|---|\n")
    for crit, raw_ado, ef in db_compare_data:
        md_lines.append(f"| **{crit}** | {raw_ado} | {ef} |\n")
    md_lines.append("\n")

    builder.add_heading_3("1.3.6. Môi trường phát triển và Công cụ hỗ trợ")
    md_lines.append("### 1.3.6. Môi trường phát triển và Công cụ hỗ trợ\n\n")

    p_tools = (
        "Hệ thống được phát triển và kiểm thử đồng bộ với các công cụ lập trình phần mềm hiện đại:"
    )
    builder.add_paragraph(p_tools)
    md_lines.append(p_tools + "\n\n")

    builder.add_bullet_point("Môi trường phát triển tích hợp (IDE): Microsoft Visual Studio 2017 / 2019 / 2022 Community & Professional.", bold_prefix="- ")
    builder.add_bullet_point("Trình biên dịch tự động: MSBuild phiên bản 15.0+ đi kèm công cụ dòng lệnh .NET Framework Developer Pack 4.6.1.", bold_prefix="- ")
    builder.add_bullet_point("Hệ quản trị CSDL cục bộ: DB Browser for SQLite và SQLite Command-Line Shell để theo dõi trực quan cấu trúc bảng và thực thi truy vấn.", bold_prefix="- ")
    builder.add_bullet_point("Khung kiểm thử đơn vị: MSTest v2 (Microsoft.VisualStudio.TestTools.UnitTesting) kết hợp công cụ thực thi VSTest Runner.", bold_prefix="- ")
    builder.add_bullet_point("Hệ thống quản lý phiên bản: Git và GitHub hỗ trợ quy trình phân nhánh và kiểm soát mã nguồn chặt chẽ.", bold_prefix="- ")

    # Dẫn dắt Hình 1.1
    p_fig11_lead = "Tổng hòa các công nghệ và giải pháp kiến trúc đã trình bày tạo nên kiến trúc phân tầng 4 lớp của MusicApp như mô tả trong Hình 1.1:"
    builder.add_paragraph(p_fig11_lead)
    md_lines.append(p_fig11_lead + "\n\n")

    schematic_arch = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|                 SƠ ĐỒ KIẾN TRÚC TỔNG THỂ 4 TẦNG HỆ THỐNG MUSICAPP                       |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "|  1. PRESENTATION LAYER (MusicApp - WPF Desktop Native)                                  |\n"
        "|     - Views: Shell Window, Sidebar, NowPlayingCard, DspEqualizer, LyricsSync, PlayQueue |\n"
        "|     - ViewModels: MainViewModel, NowPlayingViewModel, PlayQueueViewModel (MVVM Pattern)|\n"
        "|     - Composition Root: App.xaml.cs (Tự khởi tạo Dependency Injection & Quản lý vòng đời)|\n"
        "+--------------------------------------------+--------------------------------------------+\n"
        "                                             | \n"
        "                      ┌──────────────────────┴──────────────────────┐\n"
        "                      ▼                                             ▼\n"
        "+--------------------------------------------+  +-----------------------------------------+\n"
        "|  2. AUDIO PIPELINE LAYER                   |  |  3. CORE DOMAIN & PERSISTENCE LAYER     |\n"
        "|     (MusicApp.AudioEngine)                 |  |     (MusicApp.Core)                     |\n"
        "|  - NAudioService (WasapiOut / DirectSound) |  |  - Models: TrackModel, LyricLine        |\n"
        "|  - BiQuadFilter (10-Band Peaking EQ)       |  |  - Services: LrcParser, BFS LibraryScan |\n"
        "|  - SampleAggregator & FftCalculator        |  |  - Persistence: SQLite WAL Mode         |\n"
        "|  - BufferedHttpWaveStream (Range 206)      |  |  - Repositories: Track, Playlist, Queue |\n"
        "+--------------------------------------------+  +-----------------------------------------+\n"
        "                      |                                             |\n"
        "                      └──────────────────────┬──────────────────────┘\n"
        "                                             ▼\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "|  4. LOCAL BACKEND-FOR-FRONTEND LAYER (MusicApp.Bff - OWIN Self-Host :5245)              |\n"
        "|     - Controllers: TrackController (/api/v1/search, /api/v1/stream)                     |\n"
        "|     - Providers: MusicSourceRouter -> Jamendo API, Vietnamese CDN, Archive.org          |\n"
        "|     - Middlewares: StreamProxyMiddleware (HTTP Range Chunks), MemoryCacheService        |\n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 1.1: Sơ đồ kiến trúc tổng thể 4 tầng của hệ thống MusicApp Desktop",
        schematic_arch,
        note="Sơ đồ khối 4 tầng độc lập theo chuẩn Modular Monolith và nguyên lý SOLID"
    )
    md_lines.append("```\n" + schematic_arch + "\n```\n")
    md_lines.append("*Hình 1.1: Sơ đồ kiến trúc tổng thể 4 tầng của hệ thống MusicApp Desktop*\n\n")
