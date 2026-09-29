# -*- coding: utf-8 -*-
"""
chapter2.py
Sinh nội dung CHƯƠNG 2: PHÂN TÍCH, THIẾT KẾ HỆ THỐNG VÀ CƠ SỞ DỮ LIỆU
- 2.1. Phân tích chức năng và Thiết kế Use Case
- 2.2. Thiết kế Cơ sở dữ liệu (Database Design)
- 2.3. Thiết kế Kiến trúc phần mềm (.NET Solution Structure)
Gồm Bảng 2.1 đến 2.6 và Hình 2.1 đến 2.9.
"""

def render_chapter_2(builder, md_lines):
    builder.add_heading_1("CHƯƠNG 2: PHÂN TÍCH, THIẾT KẾ HỆ THỐNG VÀ CƠ SỞ DỮ LIỆU", page_break=True)
    md_lines.append("\n---\n\n# CHƯƠNG 2: PHÂN TÍCH, THIẾT KẾ HỆ THỐNG VÀ CƠ SỞ DỮ LIỆU\n\n")

    # =========================================================================
    # 2.1. PHÂN TÍCH CHỨC NĂNG VÀ THIẾT KẾ USE CASE
    # =========================================================================
    builder.add_heading_2("2.1. Phân tích chức năng và Thiết kế Use Case")
    md_lines.append("## 2.1. Phân tích chức năng và Thiết kế Use Case\n\n")

    builder.add_heading_3("2.1.1. Sơ đồ Use Case tổng quát của hệ thống")
    md_lines.append("### 2.1.1. Sơ đồ Use Case tổng quát của hệ thống\n\n")

    p_uc_intro = (
        "Hệ thống MusicApp được thiết kế xoay quanh một tác nhân chính (Primary Actor) duy nhất là **Người dùng (User)**. "
        "Toàn bộ các chức năng nghiệp vụ được nhóm thành 6 phân hệ Use Case cốt lõi: Quản lý phát nhạc trực tuyến, "
        "Quản lý thư viện cục bộ, Quản lý danh sách phát và hàng đợi, Tùy chỉnh bộ cân bằng âm thanh DSP, "
        "Đồng bộ lời bài hát Karaoke và Tương tác hệ thống gợi ý thông minh."
    )
    builder.add_paragraph(p_uc_intro)
    md_lines.append(p_uc_intro + "\n\n")

    # Dẫn dắt Hình 2.1
    p_fig21_lead = "Cấu trúc phân rã Use Case tổng quát của hệ thống MusicApp được thể hiện chi tiết trong Hình 2.1:"
    builder.add_paragraph(p_fig21_lead)
    md_lines.append(p_fig21_lead + "\n\n")

    schematic_uc = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|                    SƠ ĐỒ USE CASE TỔNG QUÁT HỆ THỐNG MUSICAPP                           |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "                                 +-----------------------+\n"
        "                                 |   NGƯỜI DÙNG (USER)   |\n"
        "                                 +-----------+-----------+\n"
        "                                             |\n"
        "         ┌───────────────────┬───────────────┼───────────────┬───────────────────┐\n"
        "         ▼                   ▼               ▼               ▼                   ▼\n"
        "  (UC01: Tìm kiếm)   (UC02: Quét thư viện) (UC03: Phát nhạc) (UC04: Quản lý PL)  (UC05: Chỉnh EQ)\n"
        "         |                   |               |               |                   |\n"
        "         | <<extend>>        |               | <<include>>   |                   | <<extend>>\n"
        "         v                   |               v               |                   v\n"
        "  (UC01.1: Gợi ý     (UC02.1: Trích xuất  (UC03.1: Ghi đệm  (UC04.1: Thêm/Xóa  (UC05.1: Lưu\n"
        "   từ khóa)           thẻ ID3)            CAS Cache)          bài hát)            Preset)\n"
        "                                             |\n"
        "                                             | <<include>>\n"
        "                                             v\n"
        "                                      (UC03.2: Đồng bộ\n"
        "                                       lời bài hát LRC)\n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.1: Sơ đồ Use Case tổng quát toàn bộ hệ thống MusicApp",
        schematic_uc,
        note="Sơ đồ phân rã Use Case theo chuẩn UML 2.0 thể hiện các quan hệ include và extend"
    )
    md_lines.append("```\n" + schematic_uc + "\n```\n")
    md_lines.append("*Hình 2.1: Sơ đồ Use Case tổng quát toàn bộ hệ thống MusicApp*\n\n")

    builder.add_heading_3("2.1.2. Đặc tả chi tiết các Use Case cốt lõi")
    md_lines.append("### 2.1.2. Đặc tả chi tiết các Use Case cốt lõi\n\n")

    # Đặc tả UC-01
    builder.add_paragraph("Đặc tả chi tiết Use Case UC-01: Phát nhạc trực tuyến (Streaming Playback)", bold_prefix="a) ")
    builder.add_bullet_point("Tên Use Case: Phát nhạc trực tuyến từ nguồn đám mây (Online Streaming).")
    builder.add_bullet_point("Tác nhân chính: Người dùng (User).")
    builder.add_bullet_point("Tiền điều kiện: Máy tính có kết nối mạng Internet; máy chủ Local BFF (:5245) đang ở trạng thái lắng nghe.")
    builder.add_bullet_point("Hậu điều kiện: Luồng âm thanh được phát ra loa/tai nghe qua NAudio; khối dữ liệu nhị phân được ghi nền vào thư mục Cache.")
    builder.add_bullet_point("Luồng sự kiện chính (Main Flow):\n"
                             "1. Người dùng bấm nút Play trên một bài hát trực tuyến trong danh sách tìm kiếm.\n"
                             "2. MainViewModel gửi yêu cầu phát nhạc tới NowPlayingViewModel và AudioService.\n"
                             "3. AudioService gửi yêu cầu GET /api/v1/stream/{id} kèm tiêu đề 'Range: bytes=0-' tới Local BFF.\n"
                             "4. Local BFF kiểm tra bộ nhớ đệm đĩa CAS: Nếu tệp {hash}.audio đã tồn tại hoàn chỉnh, BFF chuyển hướng phát thẳng từ ổ cứng (độ trễ 0ms).\n"
                             "5. Nếu chưa có cache, BFF proxy kết nối tới CDN của Jamendo/Archive.org, nhận phản hồi HTTP 206 Partial Content.\n"
                             "6. Audio Engine nạp các khối byte vào BufferedWaveProvider, giải mã PCM và phát ra thiết bị âm thanh đồng thời kích hoạt Task ghi cache nhị phân nền.\n"
                             "7. Giao diện cập nhật trạng thái Playing, kích hoạt đĩa than xoay và thanh tiến trình thời gian.")
    builder.add_bullet_point("Luồng ngoại lệ (Exception Flow):\n"
                             "- E1 (Mất kết nối mạng): Nếu không thể kết nối tới CDN và bài hát chưa có cache cục bộ, hệ thống hiển thị thông báo lỗi 'Không thể kết nối máy chủ âm thanh' và tự động chuyển sang bài tiếp theo trong hàng đợi sau 3 giây.\n"
                             "- E2 (Thiết bị âm thanh bị ngắt): Nếu người dùng rút tai nghe, NAudio bắt ngoại lệ MmException, tạm dừng phát (Pause) và giữ nguyên vị trí phát.")

    # Đặc tả UC-02
    builder.add_paragraph("Đặc tả chi tiết Use Case UC-02: Quét thư viện âm nhạc cục bộ (Local Library BFS Scan)", bold_prefix="b) ")
    builder.add_bullet_point("Tên Use Case: Quét và lập chỉ mục thư viện âm nhạc trên máy tính.")
    builder.add_bullet_point("Tác nhân chính: Người dùng (User).")
    builder.add_bullet_point("Tiền điều kiện: Người dùng đã cấp đường dẫn thư mục hợp lệ trên ổ cứng.")
    builder.add_bullet_point("Hậu điều kiện: Danh mục toàn bộ bài hát hợp lệ được lưu bền vững vào bảng tracks trong SQLite.")
    builder.add_bullet_point("Luồng sự kiện chính (Main Flow):\n"
                             "1. Người dùng chọn mục 'Thư viện cục bộ' và bấm nút 'Thêm thư mục quét'.\n"
                             "2. Cửa sổ FolderBrowserDialog hiển thị cho phép người dùng chọn đường dẫn thư mục.\n"
                             "3. LocalLibraryService khởi tạo hàng đợi Queue<string> chứa đường dẫn gốc và bắt đầu duyệt theo giải thuật BFS.\n"
                             "4. Với mỗi thư mục lấy ra từ hàng đợi, hệ thống lấy danh sách các tệp có phần mở rộng .mp3, .wav, .flac.\n"
                             "5. Sử dụng TagLibSharp đọc thẻ siêu dữ liệu (Title, Artist, Album, Duration, CoverArt).\n"
                             "6. Sinh mã băm chuẩn hóa Fingerprint Key = Normalize(Title) + '::' + Normalize(Artist).\n"
                             "7. Thực thi lệnh INSERT OR REPLACE vào bảng tracks trong một Transaction duy nhất để tối ưu tốc độ ghi đĩa.\n"
                             "8. Cập nhật thanh tiến trình hiển thị tỷ lệ hoàn thành lên giao diện người dùng.")
    builder.add_bullet_point("Luồng ngoại lệ (Exception Flow):\n"
                             "- E1 (Thư mục bị từ chối truy cập): Nếu gặp lỗi UnauthorizedAccessException trên một thư mục con, hệ thống ghi log cảnh báo và tiếp tục duyệt các thư mục còn lại trong hàng đợi mà không làm dừng ứng dụng.\n"
                             "- E2 (Tệp âm thanh hỏng): Nếu TagLibSharp không thể đọc cấu trúc tệp, hệ thống lấy tên tệp làm tiêu đề mặc định và gán Artist là 'Unknown Artist'.")

    # Đặc tả UC-03
    builder.add_paragraph("Đặc tả chi tiết Use Case UC-03: Tùy biến âm sắc và lưu cấu hình Equalizer", bold_prefix="c) ")
    builder.add_bullet_point("Tên Use Case: Cân bằng âm sắc kỹ thuật số DSP và quản lý Preset.")
    builder.add_bullet_point("Tác nhân chính: Người dùng (User).")
    builder.add_bullet_point("Tiền điều kiện: Audio Engine đang hoạt động.")
    builder.add_bullet_point("Hậu điều kiện: Âm sắc đầu ra thay đổi ngay lập tức; Preset tùy chỉnh được lưu vào bảng eq_presets.")
    builder.add_bullet_point("Luồng sự kiện chính (Main Flow):\n"
                             "1. Người dùng mở tab 'Bộ cân bằng âm thanh (DSP Equalizer)'.\n"
                             "2. Người dùng di chuyển thanh trượt của một dải tần (ví dụ: kéo băng 62Hz lên +6dB để tăng âm trầm).\n"
                             "3. ViewModel tính toán lại hệ số lọc số BiQuad theo công thức Robert Bristow-Johnson và cập nhật trực tiếp vào mảng bộ lọc của DspEqualizerSampleProvider mà không cần dừng luồng âm thanh.\n"
                             "4. Người dùng bấm nút 'Lưu cấu hình Preset', nhập tên (ví dụ: 'My Rock EQ').\n"
                             "5. Hệ thống tuần tự hóa mảng 10 giá trị Gain thành chuỗi JSON và lưu vào bảng eq_presets trong SQLite.")

    builder.add_heading_3("2.1.3. Thiết kế Sơ đồ tuần tự (Sequence Diagram)")
    md_lines.append("### 2.1.3. Thiết kế Sơ đồ tuần tự (Sequence Diagram)\n\n")

    p_seq_intro = (
        "Sơ đồ tuần tự thể hiện rõ rệt sự tương tác trao đổi thông điệp theo trục thời gian giữa các tầng thành phần. "
        "Dưới đây là 2 sơ đồ tuần tự then chốt nhất của hệ thống:"
    )
    builder.add_paragraph(p_seq_intro)
    md_lines.append(p_seq_intro + "\n\n")

    # Dẫn dắt Hình 2.2
    p_fig22_lead = "Luồng tuần tự từ khi người dùng nhập từ khóa tìm kiếm đến khi nhận được kết quả danh sách bài hát được mô tả trong Hình 2.2:"
    builder.add_paragraph(p_fig22_lead)
    md_lines.append(p_fig22_lead + "\n\n")

    schematic_seq_search = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|                 SƠ ĐỒ TUẦN TỰ: TÌM KIẾM BÀI HÁT TRỰC TUYẾN QUA LOCAL BFF                |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "  User         WPF View          MainViewModel        MusicApiClient     Local BFF (:5245) Jamendo CDN\n"
        "   |               |                   |                    |                   |              |\n"
        "   |-- Gõ từ khóa->|                   |                    |                   |              |\n"
        "   |   'Hoàng Dũng'|-- Debounce 300ms->|                    |                   |              |\n"
        "   |               |                   |-- SearchAsync() -->|                   |              |\n"
        "   |               |                   |                    |-- GET /search --->|              |\n"
        "   |               |                   |                    |   ?query=...      |-- Call API ->|\n"
        "   |               |                   |                    |                   |<-- Raw JSON -|\n"
        "   |               |                   |                    |                   | [Sanitize]   |\n"
        "   |               |                   |                    |                   | [Cache Mem]  |\n"
        "   |               |                   |                    |<-- TrackDto List -|              |\n"
        "   |               |                   |<-- List<TrackDto> -|                   |              |\n"
        "   |               |<-- Bind Items ----|                    |                   |              |\n"
        "   |<-- Render ----|                   |                    |                   |              |\n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.2: Sơ đồ tuần tự (Sequence Diagram) - Luồng tìm kiếm bài hát trực tuyến",
        schematic_seq_search,
        note="Sơ đồ tuần tự thể hiện cơ chế Debounce 300ms và định tuyến qua máy chủ ngầm Local BFF"
    )
    md_lines.append("```\n" + schematic_seq_search + "\n```\n")
    md_lines.append("*Hình 2.2: Sơ đồ tuần tự (Sequence Diagram) - Luồng tìm kiếm bài hát trực tuyến*\n\n")

    # Dẫn dắt Hình 2.3
    p_fig23_lead = "Luồng tuần tự điều khiển phát nhạc kết hợp ghi bộ nhớ đệm nhị phân CAS Disk Cache được mô tả trong Hình 2.3:"
    builder.add_paragraph(p_fig23_lead)
    md_lines.append(p_fig23_lead + "\n\n")

    schematic_seq_stream = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|         SƠ ĐỒ TUẦN TỰ: PHÁT NHẠC VÀ GHI BỘ NHỚ ĐỆM CAS CACHE NỀN                        |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "  WPF UI        NowPlayingVM        AudioEngine          Local BFF (:5245)      Disk Storage   CDN Stream\n"
        "    |                 |                  |                       |                   |             |\n"
        "    |-- Click Play -->|                  |                       |                   |             |\n"
        "    |                 |-- PlayTrack() -->|                       |                   |             |\n"
        "    |                 |                  |-- Check Local Cache ->|                   |             |\n"
        "    |                 |                  |                       |-- Exists(hash)? ->|             |\n"
        "    |                 |                  |                       |<-- Not Found -----|             |\n"
        "    |                 |                  |-- GET /stream/{id} -->|                                 |\n"
        "    |                 |                  |   (Range: 0-)         |-- HTTP 206 Range -------------->|\n"
        "    |                 |                  |                       |<-- Audio Chunk (128kbps) -------|\n"
        "    |                 |                  |<-- Stream Response ---|                                 |\n"
        "    |                 |                  | [Decode PCM to WASAPI]|-- Async Write Chunk ----------->|\n"
        "    |<-- UI State: Playing --------------|                       |   %LOCALAPPDATA%\\Cache\\*.audio  |\n"
        "    |    (Vinyl Spin & Spectrum 30fps)   |                       |<-- Cache Registered in SQLite---||\n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.3: Sơ đồ tuần tự (Sequence Diagram) - Luồng phát nhạc và ghi bộ nhớ đệm CAS",
        schematic_seq_stream,
        note="Sơ đồ tuần tự luồng streaming HTTP 206 kết hợp ghi cache nhị phân nền Content-Addressable Storage"
    )
    md_lines.append("```\n" + schematic_seq_stream + "\n```\n")
    md_lines.append("*Hình 2.3: Sơ đồ tuần tự (Sequence Diagram) - Luồng phát nhạc và ghi bộ nhớ đệm CAS*\n\n")

    builder.add_heading_3("2.1.4. Thiết kế Sơ đồ hoạt động (Activity Diagram)")
    md_lines.append("### 2.1.4. Thiết kế Sơ đồ hoạt động (Activity Diagram)\n\n")

    p_act_intro = (
        "Quy trình quét thư viện âm nhạc cục bộ sử dụng giải thuật duyệt cây theo chiều rộng (BFS) nhằm giải quyết triệt để "
        "nguy cơ tràn ngăn xếp (Stack Overflow) khi gặp cây thư mục phân cấp sâu. "
        "Sơ đồ hoạt động trong Hình 2.4 mô tả chi tiết logic xử lý bất đồng bộ này:"
    )
    builder.add_paragraph(p_act_intro)
    md_lines.append(p_act_intro + "\n\n")

    schematic_act_bfs = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|           SƠ ĐỒ HOẠT ĐỘNG (ACTIVITY DIAGRAM): QUÉT THƯ VIỆN CỤC BỘ BẰNG BFS             |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "                                      ( Bắt đầu )                                          \n"
        "                                           |                                               \n"
        "                             [ Người dùng chọn thư mục gốc ]                               \n"
        "                                           |                                               \n"
        "                           [ Khởi tạo Queue<string> thư mục ]                              \n"
        "                                           |                                               \n"
        "                               +----->[ Hàng đợi rỗng? ] --( Đúng )--> [ Cập nhật xong UI ]\n"
        "                               |           |                                    |          \n"
        "                               |        ( Sai )                             ( Kết thúc )   \n"
        "                               |           v                                               \n"
        "                               |     [ Dequeue thư mục D ]                                 \n"
        "                               |           |                                               \n"
        "                               |   [ Kiểm tra quyền truy cập? ]                            \n"
        "                               |      |                  |                                 \n"
        "                               |  ( Bị cấm )          ( Hợp lệ )                           \n"
        "                               |      v                  v                                 \n"
        "                               |  [ Bỏ qua ]      [ Lấy file .mp3, .flac, .wav ]           \n"
        "                               |                         |                                 \n"
        "                               |                 [ Còn file trong D? ]                     \n"
        "                               |                  |              |                         \n"
        "                               |               ( Đúng )        ( Sai )                     \n"
        "                               |                  v              v                         \n"
        "                               |          [ TagLibSharp đọc ID3] [ Đưa thư mục con vào Queue]\n"
        "                               |                  |              |                         \n"
        "                               |          [ Sinh FingerprintKey ]+                         \n"
        "                               |                  |                                        \n"
        "                               |          [ Lưu vào SQLite ]                               \n"
        "                               +------------------+                                        \n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.4: Sơ đồ hoạt động (Activity Diagram) - Luồng quét thư viện cục bộ BFS",
        schematic_act_bfs,
        note="Sơ đồ hoạt động mô tả giải thuật duyệt cây theo chiều rộng BFS chống tràn ngăn xếp"
    )
    md_lines.append("```\n" + schematic_act_bfs + "\n```\n")
    md_lines.append("*Hình 2.4: Sơ đồ hoạt động (Activity Diagram) - Luồng quét thư viện cục bộ BFS*\n\n")

    # =========================================================================
    # 2.2. THIẾT KẾ CƠ SỞ DỮ LIỆU
    # =========================================================================
    builder.add_heading_2("2.2. Thiết kế Cơ sở dữ liệu (Database Design)")
    md_lines.append("## 2.2. Thiết kế Cơ sở dữ liệu (Database Design)\n\n")

    builder.add_heading_3("2.2.1. Mô hình thực thể kết hợp (ERD - Entity Relationship Diagram)")
    md_lines.append("### 2.2.1. Mô hình thực thể kết hợp (ERD - Entity Relationship Diagram)\n\n")

    p_erd_intro = (
        "Cơ sở dữ liệu SQLite của MusicApp được thiết kế ở dạng chuẩn 3NF (Third Normal Form) nhằm đảm bảo tính toàn vẹn dữ liệu, "
        "triệt tiêu trùng lặp siêu dữ liệu và tối ưu hóa tốc độ truy vấn trên ổ cứng. Hệ thống bao gồm 8 thực thể dữ liệu chính: "
        "tracks (kho bài hát hợp nhất), library_folders (thư mục quét), stream_cache (bộ đệm đĩa), playlists (danh sách phát), "
        "playlist_tracks (quan hệ nhiều-nhiều danh sách bài hát), play_queue (hàng đợi phát), user_interactions (nhật ký hành vi gợi ý), "
        "và eq_presets (cấu hình bộ lọc âm sắc)."
    )
    builder.add_paragraph(p_erd_intro)
    md_lines.append(p_erd_intro + "\n\n")

    # Dẫn dắt Hình 2.5
    p_fig25_lead = "Mối quan hệ tương tác giữa các thực thể trong cơ sở dữ liệu được biểu diễn qua Sơ đồ ERD trong Hình 2.5:"
    builder.add_paragraph(p_fig25_lead)
    md_lines.append(p_fig25_lead + "\n\n")

    schematic_erd = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|               MÔ HÌNH THỰC THỂ KẾT HỢP (ERD) CƠ SỞ DỮ LIỆU SQLITE                       |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "  +----------------------+             +-----------------------+                          \n"
        "  |   library_folders    |             |       playlists       |                          \n"
        "  +----------------------+             +-----------------------+                          \n"
        "  | PK folder_path       |             | PK id                 |                          \n"
        "  |    last_scanned_at   |             |    name               |                          \n"
        "  |    total_files       |             |    description        |                          \n"
        "  +----------------------+             +-----------+-----------+                          \n"
        "                                                   | 1                                    \n"
        "                                                   |                                      \n"
        "                                                   | N                                    \n"
        "  +----------------------+             +-----------+-----------+                          \n"
        "  |     stream_cache     |             |    playlist_tracks    |                          \n"
        "  +----------------------+             +-----------------------+                          \n"
        "  | PK track_hash        |             | PK,FK1 playlist_id    |                          \n"
        "  |    file_path         |             | PK,FK2 track_id       |                          \n"
        "  |    file_size_bytes   |             |        position       |                          \n"
        "  |    last_accessed_at  |             +-----------+-----------+                          \n"
        "  +----------------------+                         | N                                    \n"
        "                                                   |                                      \n"
        "                                                   | 1                                    \n"
        "                                       +-----------+-----------+                          \n"
        "                                       |        tracks         |                          \n"
        "                                       +-----------------------+                          \n"
        "                                       | PK id                 |<-------+                 \n"
        "                                       |    track_key (UNIQUE) |        |                 \n"
        "                                       |    title, artist      |        |                 \n"
        "                                       |    duration_seconds   |        |                 \n"
        "                                       |    affinity_score     |        |                 \n"
        "                                       +-----+-----------+-----+        |                 \n"
        "                                           1 |         1 |              |                 \n"
        "                         +-------------------+           +--------------+                 \n"
        "                         | N                             | N                              \n"
        "             +-----------+-----------+       +-----------+-----------+                    \n"
        "             |   user_interactions   |       |      play_queue       |                    \n"
        "             +-----------------------+       +-----------------------+                    \n"
        "             | PK id                 |       | PK position           |                    \n"
        "             | FK track_id           |       | FK track_id           |                    \n"
        "             |    action_type        |       +-----------------------+                    \n"
        "             |    duration_played    |                                                    \n"
        "             +-----------------------+                                                    \n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.5: Sơ đồ thực thể liên kết (ERD) Cơ sở dữ liệu SQLite",
        schematic_erd,
        note="Sơ đồ ERD chuẩn 3NF thể hiện mối quan hệ 1-N và N-M giữa các thực thể cốt lõi"
    )
    md_lines.append("```\n" + schematic_erd + "\n```\n")
    md_lines.append("*Hình 2.5: Sơ đồ thực thể liên kết (ERD) Cơ sở dữ liệu SQLite*\n\n")

    builder.add_heading_3("2.2.2. Sơ đồ quan hệ dữ liệu vật lý (Physical Schema Diagram)")
    md_lines.append("### 2.2.2. Sơ đồ quan hệ dữ liệu vật lý (Physical Schema Diagram)\n\n")

    p_phy_intro = (
        "Trên môi trường vật lý SQLite, các bảng được cài đặt ràng buộc khóa chính (PRIMARY KEY), khóa ngoại (FOREIGN KEY) "
        "kèm hành động toàn vẹn ON DELETE CASCADE. Đặc biệt, để đáp ứng tiêu chuẩn truy vấn dưới 5 mili-giây cho hàng chục nghìn bài hát, "
        "các chỉ mục (INDEX) chiến lược đã được thiết lập như mô tả trong Hình 2.6:"
    )
    builder.add_paragraph(p_phy_intro)
    md_lines.append(p_phy_intro + "\n\n")

    schematic_phy = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|         SƠ ĐỒ QUAN HỆ VẬT LÝ VÀ CHỈ MỤC TỐI ƯU TRUY VẤN (SQLITE PHYSICAL SCHEMA)        |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "  TABLE: tracks                                     TABLE: playlists                      \n"
        "  - id: INTEGER (PK, AUTOINCREMENT)                 - id: INTEGER (PK, AUTOINCREMENT)     \n"
        "  - track_key: TEXT (UNIQUE)                        - name: TEXT (NOT NULL, UNIQUE)       \n"
        "  - source_type: TEXT ('local'|'jamendo'|'vn')      - description: TEXT                   \n"
        "  - source_id: TEXT (FilePath / StreamID)           - cover_uri: TEXT                     \n"
        "  - title: TEXT (NOT NULL)                          - created_at: TEXT                    \n"
        "  - artist: TEXT (NOT NULL)                         ------------------------------------- \n"
        "  - album: TEXT                                                                           \n"
        "  - duration_seconds: INTEGER                       TABLE: playlist_tracks                \n"
        "  - is_favorite: INTEGER (0|1)                      - playlist_id: INT (PK, FK playlists) \n"
        "  - affinity_score: REAL                            - track_id: INT (PK, FK tracks)       \n"
        "  -------------------------------------------       - position: INT                       \n"
        "  * INDEXES:                                        * INDEX: idx_pl_pos (playlist_id,pos) \n"
        "    - idx_tracks_artist (artist ASC)                ------------------------------------- \n"
        "    - idx_tracks_album (album ASC)                                                        \n"
        "    - idx_tracks_favorite (is_favorite)             TABLE: stream_cache                   \n"
        "    - idx_tracks_affinity (affinity_score DESC)     - track_hash: TEXT (PK, SHA-1)        \n"
        "                                                    - file_path: TEXT                     \n"
        "  TABLE: user_interactions                          - file_size_bytes: INTEGER            \n"
        "  - id: INTEGER (PK, AUTOINCREMENT)                 - last_accessed_at: TEXT              \n"
        "  - track_id: INTEGER (FK tracks ON CASCADE)        * INDEX: idx_cache_acc (last_acc ASC) \n"
        "  - action_type: TEXT                               ------------------------------------- \n"
        "  - duration_played: INTEGER                        TABLE: eq_presets                     \n"
        "  - created_at: TEXT                                - name: TEXT (PK)                     \n"
        "  * INDEX: idx_interactions_track (track_id)        - gains_json: TEXT                    \n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.6: Sơ đồ vật lý Cơ sở dữ liệu (Physical Schema Diagram) với các chỉ mục Index",
        schematic_phy,
        note="Sơ đồ vật lý chi tiết các cột, kiểu dữ liệu, khóa ngoại CASCADE và các chỉ mục hiệu năng"
    )
    md_lines.append("```\n" + schematic_phy + "\n```\n")
    md_lines.append("*Hình 2.6: Sơ đồ vật lý Cơ sở dữ liệu (Physical Schema Diagram) với các chỉ mục Index*\n\n")

    builder.add_heading_3("2.2.3. Bảng từ điển dữ liệu chi tiết (Data Dictionary)")
    md_lines.append("### 2.2.3. Bảng từ điển dữ liệu chi tiết (Data Dictionary)\n\n")

    p_dict_intro = "Cấu trúc và ý nghĩa chi tiết từng trường dữ liệu trong các bảng được định nghĩa trong các bảng dưới đây:"
    builder.add_paragraph(p_dict_intro)
    md_lines.append(p_dict_intro + "\n\n")

    # Bảng 2.1: tracks
    tracks_dict = [
        ["id", "INTEGER", "PK, AI", "Không", "Mã định danh duy nhất tự tăng của bài hát."],
        ["track_key", "TEXT", "UNIQUE", "Không", "Khóa ngón tay chuẩn hóa: Normalize(Title)::Normalize(Artist)."],
        ["source_type", "TEXT", "CHECK", "Không", "Loại nguồn nhạc: 'local', 'jamendo', 'vn'."],
        ["source_id", "TEXT", "", "Không", "Đường dẫn tệp trên đĩa (nếu local) hoặc ID luồng CDN (nếu online)."],
        ["title", "TEXT", "", "Không", "Tên hiển thị của bài hát."],
        ["artist", "TEXT", "INDEX", "Không", "Tên nghệ sĩ hoặc nhóm nhạc biểu diễn."],
        ["album", "TEXT", "INDEX", "Có", "Tên album chứa bài hát."],
        ["genre", "TEXT", "", "Có", "Thể loại âm nhạc (Pop, Rock, Ballad, EDM...)."],
        ["duration_seconds", "INTEGER", "DEFAULT 0", "Không", "Tổng thời lượng phát của bài hát tính bằng giây."],
        ["bitrate", "INTEGER", "DEFAULT 128", "Có", "Tốc độ bit âm thanh (kbps), ví dụ: 128, 256, 320."],
        ["cover_uri", "TEXT", "", "Có", "Đường dẫn tệp ảnh bìa lưu tạm trong thư mục Cache."],
        ["file_mtime", "TEXT", "", "Có", "Thời gian sửa đổi tệp gần nhất dùng cho quét gia tăng."],
        ["play_count", "INTEGER", "DEFAULT 0", "Không", "Tổng số lần người dùng đã nghe bài hát này."],
        ["skip_count", "INTEGER", "DEFAULT 0", "Không", "Số lần người dùng bấm chuyển bài khi nghe chưa tới 10 giây."],
        ["is_favorite", "INTEGER", "INDEX, 0|1", "Không", "Trạng thái bài hát yêu thích: 1 là yêu thích, 0 là bình thường."],
        ["affinity_score", "REAL", "INDEX, DESC", "Không", "Điểm số quan hệ dùng cho thuật toán gợi ý bài hát."]
    ]
    builder.add_table(
        "Bảng 2.1: Từ điển dữ liệu bảng tracks (Danh mục bài hát hợp nhất)",
        ["Tên cột", "Kiểu dữ liệu", "Ràng buộc", "Cho phép Null", "Ý nghĩa và quy tắc nghiệp vụ"],
        tracks_dict
    )

    md_lines.append("#### Bảng 2.1: Từ điển dữ liệu bảng tracks\n\n")
    md_lines.append("| Tên cột | Kiểu dữ liệu | Ràng buộc | Cho phép Null | Ý nghĩa và quy tắc nghiệp vụ |\n|---|---|---|---|---|\n")
    for col, dtype, cons, nll, desc in tracks_dict:
        md_lines.append(f"| `{col}` | {dtype} | {cons} | {nll} | {desc} |\n")
    md_lines.append("\n")

    # Bảng 2.2: library_folders & stream_cache
    table22_data = [
        ["folder_path", "TEXT", "PK", "Không", "Đường dẫn tuyệt đối của thư mục trên ổ cứng (ví dụ: D:\\Music)."],
        ["last_scanned_at", "TEXT", "", "Không", "Thời điểm quét hoàn thành gần nhất theo chuẩn ISO 8601."],
        ["total_files", "INTEGER", "DEFAULT 0", "Không", "Tổng số tệp âm thanh hợp lệ đã trích xuất từ thư mục."],
        ["track_hash", "TEXT", "PK", "Không", "Mã băm SHA-1 định danh tệp stream trực tuyến."],
        ["file_path", "TEXT", "", "Không", "Đường dẫn tệp nhị phân cache trên đĩa (%LOCALAPPDATA%\\MusicApp\\Cache)."],
        ["file_size_bytes", "INTEGER", "", "Không", "Kích thước tệp đệm trên đĩa tính bằng byte."],
        ["last_accessed_at", "TEXT", "INDEX, ASC", "Không", "Mốc thời gian truy xuất gần nhất, dùng cho giải thuật dọn dẹp LRU."]
    ]
    builder.add_table(
        "Bảng 2.2: Từ điển dữ liệu bảng library_folders và stream_cache",
        ["Tên cột", "Kiểu dữ liệu", "Ràng buộc", "Cho phép Null", "Ý nghĩa và quy tắc nghiệp vụ"],
        table22_data
    )

    # Bảng 2.3: playlists & playlist_tracks
    table23_data = [
        ["id", "INTEGER", "PK, AI", "Không", "Mã định danh duy nhất tự tăng của Playlist."],
        ["name", "TEXT", "UNIQUE", "Không", "Tên danh sách phát (ví dụ: 'My Favorite 2024')."],
        ["description", "TEXT", "", "Có", "Mô tả ngắn gọn về danh sách phát."],
        ["cover_uri", "TEXT", "", "Có", "Ảnh đại diện tùy chỉnh của danh sách phát."],
        ["playlist_id", "INTEGER", "PK, FK", "Không", "Khóa ngoại tham chiếu playlists(id) ON DELETE CASCADE."],
        ["track_id", "INTEGER", "PK, FK", "Không", "Khóa ngoại tham chiếu tracks(id) ON DELETE CASCADE."],
        ["position", "INTEGER", "INDEX, ASC", "Không", "Thứ tự sắp xếp của bài hát trong danh sách phát (0, 1, 2...)."],
        ["added_at", "TEXT", "", "Không", "Thời điểm bài hát được thêm vào danh sách phát."]
    ]
    builder.add_table(
        "Bảng 2.3: Từ điển dữ liệu bảng playlists và playlist_tracks",
        ["Tên cột", "Kiểu dữ liệu", "Ràng buộc", "Cho phép Null", "Ý nghĩa và quy tắc nghiệp vụ"],
        table23_data
    )

    # Bảng 2.4: play_queue & user_interactions
    table24_data = [
        ["position", "INTEGER", "PK", "Không", "Vị trí bài hát trong hàng đợi phát nhạc."],
        ["track_id", "INTEGER", "FK tracks", "Không", "Khóa ngoại tham chiếu tracks(id) ON DELETE CASCADE."],
        ["id", "INTEGER", "PK, AI", "Không", "Mã định danh tự tăng của bản ghi tương tác."],
        ["track_id", "INTEGER", "FK tracks", "Không", "Bài hát mà người dùng đã thực hiện tương tác."],
        ["action_type", "TEXT", "", "Không", "Hành vi: 'play_start', 'play_complete', 'skip', 'favorite'."],
        ["duration_played", "INTEGER", "DEFAULT 0", "Có", "Số giây đã nghe bài hát trước khi chuyển bài."]
    ]
    builder.add_table(
        "Bảng 2.4: Từ điển dữ liệu bảng play_queue và user_interactions",
        ["Tên cột", "Kiểu dữ liệu", "Ràng buộc", "Cho phép Null", "Ý nghĩa và quy tắc nghiệp vụ"],
        table24_data
    )

    # Bảng 2.5: eq_presets & app_settings
    table25_data = [
        ["name", "TEXT", "PK", "Không", "Tên cấu hình Preset (ví dụ: 'Flat', 'Rock', 'Bass Boost')."],
        ["gains_json", "TEXT", "", "Không", "Chuỗi JSON chứa mảng 10 giá trị Gain (ví dụ: '[0.0, 3.5, -2.0, ...]')."],
        ["is_custom", "INTEGER", "DEFAULT 0", "Không", "1 nếu là Preset do người dùng tự tạo; 0 nếu là mặc định hệ thống."],
        ["key", "TEXT", "PK", "Không", "Khóa cấu hình hệ thống (ví dụ: 'Volume', 'LastFolder', 'Theme')."],
        ["value", "TEXT", "", "Không", "Giá trị thiết lập dạng văn bản."]
    ]
    builder.add_table(
        "Bảng 2.5: Từ điển dữ liệu bảng eq_presets và app_settings",
        ["Tên cột", "Kiểu dữ liệu", "Ràng buộc", "Cho phép Null", "Ý nghĩa và quy tắc nghiệp vụ"],
        table25_data
    )

    # =========================================================================
    # 2.3. THIẾT KẾ KIẾN TRÚC PHẦN MỀM
    # =========================================================================
    builder.add_heading_2("2.3. Thiết kế Kiến trúc phần mềm (.NET Solution Structure)")
    md_lines.append("## 2.3. Thiết kế Kiến trúc phần mềm (.NET Solution Structure)\n\n")

    builder.add_heading_3("2.3.1. Cấu trúc phân tầng dự án (Layered Solution Architecture)")
    md_lines.append("### 2.3.1. Cấu trúc phân tầng dự án (Layered Solution Architecture)\n\n")

    p_sol_intro = (
        "Dự án MusicApp được tổ chức dưới dạng Solution bao gồm 4 Project độc lập và 1 Test Project. "
        "Mỗi Project đảm nhận một ranh giới trách nhiệm riêng biệt (Separation of Concerns), đảm bảo tính ghép lỏng (Loose Coupling) "
        "và gắn kết cao (High Cohesion) như tổng hợp trong Bảng 2.6:"
    )
    builder.add_paragraph(p_sol_intro)
    md_lines.append(p_sol_intro + "\n\n")

    sol_project_data = [
        ["MusicApp (WPF Native)", "Presentation Layer", "Chứa toàn bộ Views (XAML), ViewModels, DataTemplate, Converters, Resources/Themes. Đóng vai trò Composition Root tại App.xaml.cs."],
        ["MusicApp.Core", "Domain & Persistence Layer", "Chứa POCO Models, DTOs, Repository Interfaces, Services (LrcParser, LocalLibraryService), SQLite DatabaseInitializer và 6 Repository implementations."],
        ["MusicApp.AudioEngine", "Digital Signal Processing", "Đóng gói toàn bộ thư viện NAudio, WasapiOut/DirectSound, BiQuadFilter 10 băng tần, SampleAggregator, FftCalculator, BufferedHttpWaveStream."],
        ["MusicApp.Bff", "Local Gateway / Server Host", "Máy chủ OWIN Self-Host (Microsoft.Owin.Hosting), Web API 2 Controllers, MusicSourceRouter, JamendoProvider, StreamProxyMiddleware."],
        ["MusicApp.Tests", "Test Automation Suite", "Bộ 60 bài kiểm thử đơn vị MSTest v2 bao phủ toàn diện 4 tầng chức năng: BFF endpoint, Audio Engine, Core Persistence, ViewModel Navigation."]
    ]

    builder.add_table(
        "Bảng 2.6: Phân bổ trách nhiệm các Project thành phần trong Solution",
        ["Tên Project trong Visual Studio", "Tầng kiến trúc tương ứng", "Trách nhiệm kỹ thuật và các thành phần chính"],
        sol_project_data
    )

    md_lines.append("#### Bảng 2.6: Phân bổ trách nhiệm các Project thành phần\n\n")
    md_lines.append("| Tên Project trong Visual Studio | Tầng kiến trúc tương ứng | Trách nhiệm kỹ thuật và các thành phần chính |\n|---|---|---|\n")
    for prj, lyr, resp in sol_project_data:
        md_lines.append(f"| `{prj}` | {lyr} | {resp} |\n")
    md_lines.append("\n")

    builder.add_heading_3("2.3.2. Sơ đồ chuỗi xử lý tín hiệu âm thanh DSP Pipeline")
    md_lines.append("### 2.3.2. Sơ đồ chuỗi xử lý tín hiệu âm thanh DSP Pipeline\n\n")

    p_dsp_intro = (
        "Đường ống xử lý tín hiệu âm thanh kỹ thuật số (DSP Pipeline) là trái tim công nghệ của MusicApp. "
        "Mọi luồng âm thanh từ tệp đĩa cục bộ hay luồng mạng đều được chuẩn hóa thành định dạng mẫu IEEE Float PCM 32-bit (44.1kHz Stereo) "
        "trước khi đi qua chuỗi xử lý thời gian thực được mô tả trong Hình 2.7:"
    )
    builder.add_paragraph(p_dsp_intro)
    md_lines.append(p_dsp_intro + "\n\n")

    schematic_dsp = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|        CHUỖI ĐƯỜNG ỐNG XỬ LÝ TÍN HIỆU ÂM THANH KỸ THUẬT SỐ (DSP PIPELINE)               |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "  [ NGUỒN ÂM THANH ] ---> Local File (FileStream) HOẶC Stream Online (BufferedHttpStream) \n"
        "           |                                                                              \n"
        "           v                                                                              \n"
        "  [ GIẢI MÃ ĐẦU VÀO ] ---> Mp3FileReader / WaveFileReader / MediaFoundationReader        \n"
        "           |               (Chuẩn hóa thành định dạng PCM 16-bit / 44.100 Hz / Stereo)   \n"
        "           v                                                                              \n"
        "  [ CHUYỂN ĐỔI MẪU ] ---> WaveToSampleProvider (Chuyển đổi sang IEEE Float 32-bit)       \n"
        "           |                                                                              \n"
        "           v                                                                              \n"
        "  [ BỘ CÂN BẰNG ÂM SẮC DSP ] ---> DspEqualizerSampleProvider                             \n"
        "                                  - 10 Băng tần Peaking BiQuad Filter cho Kênh Trái (L)   \n"
        "                                  - 10 Băng tần Peaking BiQuad Filter cho Kênh Phải (R)  \n"
        "                                  (Điều chỉnh biên độ +/-12dB bằng công thức Bristow-Johnson)\n"
        "           |                                                                              \n"
        "           v                                                                              \n"
        "  [ BỘ TRÍCH MẪU KHÔNG RÁC ] ---> SampleAggregator (Zero-Allocation Buffer Ring)          \n"
        "           |                                         |                                    \n"
        "           | (Truyền tiếp mẫu âm thanh)               | (Trích xuất khối 1024 điểm mẫu)  \n"
        "           v                                         v                                    \n"
        "  [ THIẾT BỊ XUẤT ÂM THANH ]              [ TÍNH TOÁN FFT 1024 ĐIỂM ]                    \n"
        "  - WasapiOut (Shared Mode)               - Fast Fourier Transform (FftCalculator)       \n"
        "  - Hoặc DirectSoundOut                   - Phân nhóm thành 16 Frequency Bins            \n"
        "  (Đưa ra Loa / Tai nghe)                            |                                    \n"
        "                                                     v                                    \n"
        "                                          [ CẬP NHẬT GIAO DIỆN ]                          \n"
        "                                          - Dispatcher cập nhật Spectrum Bars 30fps       \n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.7: Sơ đồ đường ống xử lý tín hiệu âm thanh kỹ thuật số (DSP Pipeline) trong NAudio",
        schematic_dsp,
        note="Sơ đồ khối DSP thể hiện luồng xử lý tín hiệu âm thanh từ giải mã, lọc số đến phân tích phổ FFT"
    )
    md_lines.append("```\n" + schematic_dsp + "\n```\n")
    md_lines.append("*Hình 2.7: Sơ đồ đường ống xử lý tín hiệu âm thanh kỹ thuật số (DSP Pipeline) trong NAudio*\n\n")

    builder.add_heading_3("2.3.3. Sơ đồ lớp chi tiết (Class Diagram)")
    md_lines.append("### 2.3.3. Sơ đồ lớp chi tiết (Class Diagram)\n\n")

    p_class_intro = (
        "Hệ thống lớp của MusicApp được thiết kế theo đúng nguyên lý Hướng đối tượng và các mẫu thiết kế kinh điển: "
        "ObservableObject thực thi INotifyPropertyChanged cho tầng ViewModel; Repository Pattern cho tầng truy xuất dữ liệu SQLite. "
        "Hai sơ đồ lớp chi tiết dưới đây đặc tả đầy đủ các thuộc tính và phương thức cốt lõi:"
    )
    builder.add_paragraph(p_class_intro)
    md_lines.append(p_class_intro + "\n\n")

    # Dẫn dắt Hình 2.8
    p_fig28_lead = "Cấu trúc phân cấp lớp của hệ thống ViewModel và cơ chế Data Binding được thể hiện trong Hình 2.8:"
    builder.add_paragraph(p_fig28_lead)
    md_lines.append(p_fig28_lead + "\n\n")

    schematic_vm_class = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|                  SƠ ĐỒ LỚP (CLASS DIAGRAM): HỆ THỐNG VIEWMODEL                          |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "                          +----------------------------------+                            \n"
        "                          |   ObservableObject (Core.Common) |                            \n"
        "                          +----------------------------------+                            \n"
        "                          | # SetProperty<T>()               |                            \n"
        "                          | # OnPropertyChanged()            |                            \n"
        "                          +-----------------+----------------+                            \n"
        "                                            |                                             \n"
        "         +----------------------------------+----------------------------------+          \n"
        "         |                                  |                                  |          \n"
        "         v                                  v                                  v          \n"
        "+-------------------------+  +-------------------------------+  +-------------------------+\n"
        "|      MainViewModel      |  |      NowPlayingViewModel      |  |   DspEqualizerViewModel | \n"
        "+-------------------------+  +-------------------------------+  +-------------------------+\n"
        "| - _apiClient            |  | - _audioService: IAudioService|  | - _dspService           |\n"
        "| - SearchQuery: string   |  | - CurrentTrack: TrackModel    |  | + Bands: List<BandVM>   |\n"
        "| + SearchResults         |  | - PlaybackState               |  | + Presets: List<string> |\n"
        "| + SearchCommand         |  | + SpectrumBins: double[16]    |  | + SelectedPreset        |\n"
        "| + NavigateCommand       |  | + PlayPauseCommand            |  | + ApplyPresetCommand    |\n"
        "+-------------------------+  +-------------------------------+  +-------------------------+\n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.8: Sơ đồ lớp (Class Diagram) - Hệ thống ViewModel và Data Binding",
        schematic_vm_class,
        note="Sơ đồ lớp kế thừa từ ObservableObject và quản lý trạng thái hiển thị bằng ICommand"
    )
    md_lines.append("```\n" + schematic_vm_class + "\n```\n")
    md_lines.append("*Hình 2.8: Sơ đồ lớp (Class Diagram) - Hệ thống ViewModel và Data Binding*\n\n")

    # Dẫn dắt Hình 2.9
    p_fig29_lead = "Cấu trúc phân cấp lớp của hệ thống Persistence và mẫu thiết kế Repository Pattern được thể hiện trong Hình 2.9:"
    builder.add_paragraph(p_fig29_lead)
    md_lines.append(p_fig29_lead + "\n\n")

    schematic_repo_class = (
        "+-----------------------------------------------------------------------------------------+\n"
        "|           SƠ ĐỒ LỚP (CLASS DIAGRAM): PERSISTENCE & REPOSITORY PATTERN                   |\n"
        "+-----------------------------------------------------------------------------------------+\n"
        "  <<Interface>>                                                <<Interface>>              \n"
        "  ITrackRepository                                             IPlaylistRepository        \n"
        "  + GetAllTracksAsync(): Task<IEnumerable<TrackModel>>         + GetAllPlaylistsAsync()   \n"
        "  + GetTrackByKeyAsync(key): Task<TrackModel>                  + CreatePlaylistAsync()    \n"
        "  + UpsertTrackAsync(track): Task                              + AddTrackToPlaylistAsync()\n"
        "  + SetFavoriteAsync(id, isFav): Task                          + RemoveTrackAsync()       \n"
        "         ^                                                            ^                   \n"
        "         | implements                                                 | implements        \n"
        "  +------+--------------------+                                +------+-------------------+\n"
        "  |   TrackRepository         |                                |  PlaylistRepository      |\n"
        "  +---------------------------+                                +--------------------------+\n"
        "  | - _connString: string     |                                | - _connString: string    |\n"
        "  | + UpsertTrackAsync()      |                                | + AddTrackToPlaylist()   |\n"
        "  +---------------------------+                                +--------------------------+\n"
        "                 \\                                                            /           \n"
        "                  +-----------------------------+----------------------------+            \n"
        "                                                |                                         \n"
        "                                                v                                         \n"
        "                               +---------------------------------+                        \n"
        "                               |     DatabaseInitializer         |                        \n"
        "                               +---------------------------------+                        \n"
        "                               | + InitializeDatabase()          |                        \n"
        "                               | - ExecuteSqlScript()            |                        \n"
        "                               | - ApplyWalPragma()              |                        \n"
        "                               +---------------------------------+                        \n"
        "+-----------------------------------------------------------------------------------------+"
    )
    builder.add_figure_placeholder(
        "Hình 2.9: Sơ đồ lớp (Class Diagram) - Hệ thống Persistence và Repository Pattern",
        schematic_repo_class,
        note="Sơ đồ lớp trừu tượng hóa truy xuất dữ liệu SQLite ADO.NET thuần qua Repository Pattern"
    )
    md_lines.append("```\n" + schematic_repo_class + "\n```\n")
    md_lines.append("*Hình 2.9: Sơ đồ lớp (Class Diagram) - Hệ thống Persistence và Repository Pattern*\n\n")
