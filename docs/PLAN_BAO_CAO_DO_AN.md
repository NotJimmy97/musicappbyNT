# KẾ HOẠCH VIẾT BÁO CÁO ĐỒ ÁN — MUSICAPP DESKTOP

> **Đề tài:** Xây dựng ứng dụng phát nhạc Desktop Native trên nền tảng WPF (.NET Framework 4.6.1) với kiến trúc MVVM, BFF cục bộ, Audio Engine DSP và hệ thống Persistence SQLite.
>
> **Môn học:** Lập trình ứng dụng .NET
>
> **Hình thức:** Báo cáo đồ án môn học (cô đọng 3 chương)

---

## 0. TỔNG QUAN DỰ ÁN — CƠ SỞ ĐỂ VIẾT BÁO CÁO

### 0.1. Thực tế dự án (đã xác minh từ source code)

| Hạng mục | Giá trị thực tế |
|---|---|
| Nền tảng | .NET Framework 4.6.1, C# 7.3 |
| UI Framework | WPF / XAML |
| Mô hình kiến trúc | MVVM + Modular Monolith + In-Process BFF |
| BFF cục bộ | OWIN Self-Host (Microsoft.Owin.SelfHost 4.2.2 + WebApi 5.2.9), cổng `5245` |
| Audio Engine | NAudio 1.10.0 (DSP BiQuad 10 băng tần + FFT 16 cột) |
| Persistence | SQLite + System.Data.SQLite (qua Repository Pattern) |
| ORM/ADO | ADO.NET thuần (không dùng Entity Framework) |
| Đóng gói | TagLibSharp 2.2.0 (đọc ID3), gong-wpf-dragdrop 2.3.2 |
| Test Framework | MSTest v2 — **60 test cases** (đã pass) |
| Cấu trúc solution | `MusicApp.sln` gồm 4 projects: WPF App, Core, AudioEngine, Bff + 1 test project |

### 0.2. Cấu trúc Solution (verbatim từ `MusicApp.sln`)

```
MusicApp.sln
├── MusicApp/                        # WPF Desktop (Presentation + App Startup)
│   ├── App.xaml(.cs)                # Composition root, khởi tạo DI tay, dispose chuỗi
│   ├── MainWindow.xaml(.cs)         # Shell 2 cột (Sidebar 220px + ContentControl)
│   ├── ViewModels/                  # 7 VM: Main, NowPlaying, PlayQueue, LocalLibrary,
│   │                                 #       Lyrics, DspEqualizer, EqualizerBand
│   ├── Views/                       # 6 UserControl: Sidebar, NowPlayingCard,
│   │                                 #           LocalLibraryScanner, PlayQueue,
│   │                                 #           LyricsSync, DspEqualizer
│   ├── Converters/                  # FrozenImage, BoolToVisibility, v.v.
│   └── Resources/Themes/            # DarkTheme.xaml, LightTheme.xaml
├── src/
│   ├── MusicApp.Core/               # Domain (POCO, Services, Persistence, DTOs)
│   │   ├── Common/                  # ObservableObject, RelayCommand, AsyncRelayCommand
│   │   ├── Dtos/                    # SearchResponseDto, TrackDto
│   │   ├── Interfaces/              # IAudioService, IDspEqualizerService, ILyricsService,
│   │   │                             #   ITrackRepository, IPlaylistRepository, …
│   │   ├── Models/                  # TrackModel, LyricLine, PlaybackState
│   │   ├── Services/                # LrcParser, LocalLibraryService, LyricsService
│   │   └── Persistence/             # DatabaseInitializer + 6 Repositories
│   ├── MusicApp.AudioEngine/        # NAudio + DSP pipeline
│   │   ├── Dsp/                     # BiQuadFilter, DspEqualizerSampleProvider,
│   │   │                             #   SampleAggregator, FftCalculator, SpectrumBin
│   │   ├── Stream/                  # BufferedHttpWaveStream (hỗ trợ HTTP Range)
│   │   └── NAudioService.cs
│   └── MusicApp.Bff/                # OWIN Self-Host (HTTP :5245)
│       ├── Controllers/             # TrackController (search + stream)
│       ├── Providers/               # MusicSourceRouter, Jamendo, Vietnamese source
│       ├── Startup.cs               # OWIN pipeline + Web API 2 routing
│       └── BffServerHost.cs         # Vòng đời máy chủ
└── tests/
    └── MusicApp.Tests/              # 60 test cases MSTest
```

### 0.3. Tài liệu dự án đã có (tận dụng làm nguồn trích dẫn)

- `README.md` (30.9K) — kiến trúc tổng quan, luồng dữ liệu, MVVM.
- `TECHNICAL_DESIGN_DOCUMENT.md` (36.8K) — sơ đồ Mermaid, đặc tả kiến trúc.
- `THIET_KE_KIEN_TRUC_DATABASE_VA_GOI_Y_SPOTIFY.md` (24.8K) — thiết kế DB SQLite, ERD.
- `HUONG_DAN_TRIEN_KHAI_CHI_TIET.md` (34.0K) — hướng dẫn cài đặt NuGet, cấu hình.
- `PLAN_NANG_CAP_TOAN_DIEN.md` (27.8K) — lộ trình phát triển, đánh giá.
- `DOCS_XAY_DUNG_VA_TOI_UU_MUSICAPP.md` (41.4K) — tối ưu & vận hành.
- `DANH_GIA_CHUC_NANG_VA_HIEN_TRANG.md` (17.1K) — đánh giá hiện trạng.

→ **Báo cáo sẽ trích dẫn trực tiếp từ các tệp này** (file:line), không chế lại từ đầu.

---

## 1. MỤC TIÊU CỦA FILE PLAN NÀY

Biến toàn bộ dự án MusicApp thành **một bản báo cáo đồ án môn học** theo đúng khung 3 chương chuẩn mực:

1. **Chương 1** — Tổng quan đề tài & công nghệ (giới thiệu + lý thuyết nền).
2. **Chương 2** — Phân tích, thiết kế hệ thống & cơ sở dữ liệu (UML, ERD, kiến trúc).
3. **Chương 3** — Cài đặt thực nghiệm & kết quả (code snippet, giao diện, kiểm thử, đánh giá).

Kèm các trang đầu (bìa, mục lục, danh mục hình/bảng) + Kết luận + Tài liệu tham khảo + Phụ lục.

---

## 2. KẾ HOẠCH THỰC THI — 6 GIAI ĐOẠN

### Giai đoạn 1 — Khảo sát bổ sung (nếu cần)

**Mục tiêu:** Bổ sung những thông tin còn thiếu mà 6 file .md chưa nêu rõ (test report thực tế, số liệu, giao diện thực tế).

| Hành động | Công cụ | Output |
|---|---|---|
| Đọc chi tiết file `THIET_KE_KIEN_TRUC_DATABASE_VA_GOI_Y_SPOTIFY.md` | Read | ERD, schema bảng, data dictionary |
| Đọc chi tiết `src/MusicApp.Core/Persistence/DatabaseInitializer.cs` | Read | Script SQL khởi tạo DB (cho Phụ lục) |
| Liệt kê 60 test case từ `tests/MusicApp.Tests/` | `mcp__semble__search` | Bảng test case cho Chương 3 |
| Xác minh số liệu: cổng 5245, công thức BiQuad, tần số 10 băng | Read (file code) | Trích dẫn `file:line` chính xác |

> **Lưu ý:** Nếu đã đủ thông tin để viết → bỏ qua giai đoạn này. README + TECHNICAL_DESIGN đã bao phủ 80% Chương 1–2.

---

### Giai đoạn 2 — Viết Chương 1 (Tổng quan)

**Nguồn trích:** `README.md` §1, §3, §5; `TECHNICAL_DESIGN_DOCUMENT.md` §1.

| Mục | Nội dung | Trích từ |
|---|---|---|
| **1.1. Đặt vấn đề & Mục tiêu** | (a) Thực trạng: nghe nhạc online thiếu tùy biến âm sắc, offline rời rạc, lời bài hát độc lập. (b) Mục tiêu: ứng dụng Desktop tích hợp cả 4 luồng (search online + stream + offline + lyrics) với bộ cân bằng DSP 10 băng tần. (c) Đối tượng: người dùng cá nhân thích tùy biến âm thanh, hỗ trợ tiếng Việt. (d) Phạm vi: trong scope = 4 luồng trên + persistence; ngoài scope = cloud sync, mobile. | `README.md:1-9` |
| **1.2. Khảo sát nghiệp vụ & Yêu cầu** | (a) Use case tổng quát: Tìm kiếm, Phát nhạc, Quản lý hàng đợi, Quét thư viện cục bộ, Xem lời, Chỉnh EQ, Lưu preset, Yêu thích/Playlist. (b) Functional: liệt kê 8-10 chức năng chính + actor. (c) Non-functional: độ trễ DSP < 100ms, freeze `BitmapImage` chống leak, zero-allocation trong audio thread, đa luồng an toàn. | `README.md:26-160` |
| **1.3. Cơ sở công nghệ** | (a) .NET Framework 4.6.1 + C# 7.3 — lý do: tương thích Windows gốc, không cần redistributable mới. (b) MVVM pattern — tách code-behind, INotifyPropertyChanged, ICommand. (c) Repository Pattern cho persistence. (d) BFF OWIN — bảo vệ credentials, chuẩn hóa stream. (e) Công cụ: Visual Studio 2017+, MSBuild, MSTest, Git. | `README.md:221-237` |

**Kết quả Chương 1:** ~10 trang, 3-4 hình (sơ đồ tổng quan, sơ đồ MVVM).

---

### Giai đoạn 3 — Viết Chương 2 (Phân tích thiết kế)

**Nguồn trích:** `TECHNICAL_DESIGN_DOCUMENT.md` (mermaid), `README.md` §1.2, `THIET_KE_KIEN_TRUC_DATABASE_VA_GOI_Y_SPOTIFY.md`.

| Mục | Nội dung | Trích từ |
|---|---|---|
| **2.1. Phân tích chức năng & Use Case** | (a) Sơ đồ Use Case tổng quát (vẽ lại từ danh sách 8 chức năng). (b) Đặc tả 2-3 Use Case quan trọng nhất: **UC-Phát nhạc trực tuyến** (main flow + exception khi mất mạng), **UC-Quét thư viện cục bộ** (BFS, xử lý exception quyền truy cập). (c) Activity Diagram cho "Phát nhạc trực tuyến" (dựa trên README §1.1.B). | `README.md:38-46` |
| **2.2. Thiết kế CSDL** | (a) ERD: 6 bảng — `tracks`, `artists`, `albums`, `playlists`, `playlist_tracks`, `favorites`, `stream_cache`, `interactions`, `presets`, `settings`. (b) Data dictionary: 5 bảng chính, cột + kiểu + ràng buộc. (c) Lệnh `CREATE TABLE` (Phụ lục). | `THIET_KE_KIEN_TRUC_DATABASE_VA_GOI_Y_SPOTIFY.md` |
| **2.3. Thiết kế Kiến trúc** | (a) Sơ đồ lớp: 4 tầng (Presentation WPF / Core Services / AudioEngine / BFF OWIN) — vẽ lại từ `README.md:11-24`. (b) Sơ đồ tuần tự "Tìm kiếm → Trả về danh sách" (mermaid sequence) — tái sử dụng `TECHNICAL_DESIGN_DOCUMENT.md:17-47`. (c) Sơ đồ đồ thị âm thanh DSP (decoder → EQ → aggregator → FFT → output). (d) Class Diagram cho 6 Repository + 7 ViewModel. (e) Bảng phân công lớp theo Project (MusicApp / Core / AudioEngine / BFF). | `README.md:67-110, 130-160` |
| **2.4. Thiết kế giao diện** | (a) Wireframe 2 cột (Sidebar 220px + Content). (b) Sơ đồ chuyển View qua `CurrentViewName`. (c) Mapping DataTemplate → View. | `README.md:163-167` |

**Kết quả Chương 2:** ~12 trang, 6-8 hình (UML + ERD + Sequence).

---

### Giai đoạn 4 — Viết Chương 3 (Cài đặt & Kiểm thử)

**Nguồn trích:** `HUONG_DAN_TRIEN_KHAI_CHI_TIET.md`, `tests/MusicApp.Tests/*.cs`, `DANH_GIA_CHUC_NANG_VA_HIEN_TRANG.md`.

| Mục | Nội dung | Trích từ |
|---|---|---|
| **3.1. Môi trường triển khai** | (a) Cấu hình: Windows 10/11, VS 2017+, .NET Framework 4.6.1 Dev Pack. (b) Connection string SQLite (`MusicApp.db` trong AppData). (c) Script `DatabaseInitializer.cs` chạy khi App.OnStartup. | `HUONG_DAN_TRIEN_KHAI_CHI_TIET.md` |
| **3.2. Hiện thực chức năng chính** | (a) **Tìm kiếm**: snippet `MainViewModel` (debounce 300ms) + `IMusicApiClient`. (b) **Phát nhạc**: snippet `NAudioService.InitializeAsync` + luồng proxy. (c) **Quét thư viện**: snippet `LocalLibraryService.ScanDirectoryAsync` (BFS). (d) **Lyrics**: snippet `LrcParser` (regex, O(log N) binary search). (e) **DSP EQ**: snippet `DspEqualizerSampleProvider` (10 band, 20 filter cho stereo). (f) **Persistence**: snippet `TrackRepository` (ADO.NET SQLite). (g) **Hình ảnh giao diện**: 6 màn hình chính — chèn ảnh thực tế (nếu user cung cấp) hoặc dùng placeholder ghi chú "Xem Hình 3.x". | `README.md:28-62, 67-110` |
| **3.3. Kiểm thử** | (a) Chiến lược: Unit Test với MSTest v2, phủ 4 tầng (BFF endpoint / DSP / Core / ViewModel). (b) Bảng 10 test case tiêu biểu (chọn từ 60): test FFT, test EQ BiQuad, test LRC parser, test BFS scan, test BFF endpoint, test ViewModel navigation. (c) Kết quả: `Total: 60, Passed: 60`. | `README.md:317-326` |
| **3.4. Đánh giá & Hướng phát triển** | (a) Ưu điểm: 60 test pass, kiến trúc phân lớp rõ, zero-alloc audio thread, MVVM thuần. (b) Hạn chế: chỉ chạy Windows, chưa có cloud sync, chưa responsive mobile. (c) Hướng phát triển: port .NET 8 + cross-platform, cloud sync playlist, AI gợi ý, mobile companion. | `PLAN_NANG_CAP_TOAN_DIEN.md`, `DANH_GIA_CHUC_NANG_VA_HIEN_TRANG.md` |

**Kết quả Chương 3:** ~12 trang, 5-7 hình (UI) + 1 bảng 10 test case.

---

### Giai đoạn 5 — Viết các phần phụ trợ

| Phần | Nội dung |
|---|---|
| **Trang bìa** | Tên trường, khoa, môn, tên đề tài, GVHD, SVTH, niên khóa (placeholder để user điền). |
| **Lời cảm ơn + Cam đoan** | 1 trang mỗi phần. |
| **Mục lục** | 3 cấp (1, 1.1, 1.1.1). |
| **Danh mục hình/bảng** | Auto-list theo số Hình X.Y, Bảng X.Y. |
| **Kết luận** | 1 trang tóm tắt: đã làm được gì, kết quả thực nghiệm, hướng phát triển. |
| **Tài liệu tham khảo** | (1) Microsoft Learn — .NET docs. (2) NAudio GitHub. (3) WPF MVVM pattern docs. (4) MSTest v2 docs. (5) OWIN Self-Host docs. (6) TagLibSharp. (7) SQLite docs. |
| **Phụ lục** | (A) Script `CREATE TABLE` từ `DatabaseInitializer.cs`. (B) Sơ đồ thư mục source. (C) Danh sách NuGet packages. (D) Hướng dẫn build bằng MSBuild. |

---

### Giai đoạn 6 — Định dạng Word theo Guideline

| Mục | Thông số |
|---|---|
| Font body | Times New Roman 13 **hoặc** Calibri 12 |
| Line spacing | 1.3 – 1.5 |
| Paragraph spacing | Before 3pt, After 6pt |
| Lề | Trái 3.0–3.5 cm; Phải/Trên/Dưới 2.0 cm |
| Số đề mục | Tối đa 3 cấp (1, 1.1, 1.1.1) |
| Hình | Nhãn **bên dưới**, căn giữa, format `Hình X.Y: <mô tả>` |
| Bảng | Nhãn **bên trên**, căn giữa/trái, format `Bảng X.Y: <mô tả>` |
| Caption bắt buộc | Mọi hình/bảng phải có câu dẫn dắt trong đoạn văn trước |
| Code snippet | Font Consolas / Courier New 9.5–10, nền xám nhẹ, **chỉ chèn logic quan trọng** (không paste tràn trang) |

---

## 3. PHÂN BỔ KHỐI LƯỢNG — ƯỚC TÍNH

| Phần | Số trang | Số hình/bảng |
|---|---|---|
| Trang bìa + Lời cảm ơn + Cam đoan + Mục lục | 5 | – |
| Chương 1 | 10 | 3 hình |
| Chương 2 | 12 | 7 hình (UML, ERD, Sequence, Class) |
| Chương 3 | 12 | 5 hình UI + 1 bảng 10 test case |
| Kết luận | 1 | – |
| Tài liệu tham khảo | 1 | – |
| Phụ lục | 3 | 1 bảng script SQL |
| **Tổng** | **~45 trang** | **~15 hình, ~3 bảng** |

---

## 4. CHECKLIST CHẤT LƯỢNG TRƯỚC KHI BÀN GIAO

- [ ] Mỗi hình/bảng có caption + câu dẫn dắt trong đoạn văn.
- [ ] Mỗi code snippet có comment dòng đầu giải thích mục đích.
- [ ] Số liệu kỹ thuật phải trích `file:line` (ví dụ: "theo `README.md:208`…").
- [ ] Từ viết tắt lần đầu xuất hiện phải giải nghĩa (BFF, MVVM, EQ, FFT, BFS, ID3, OWIN, INotifyPropertyChanged).
- [ ] Không có đoạn "Sure! I'd be happy to…" — văn phong học thuật, khách quan.
- [ ] Font chữ, lề, line spacing đúng chuẩn báo cáo.
- [ ] 60 test case MSTest đã verify pass, có ít nhất 5-10 case trích vào Chương 3.
- [ ] Có đủ 5 file Markdown nguồn trong TLTK: README, TDD, HUONG_DAN_TRIEN_KHAI, PLAN_NANG_CAP, TECHNICAL_DESIGN.

---

## 5. LỆNH KHỞI CHẠY — SAU KHI PLAN ĐƯỢC DUYỆT

Khi user duyệt plan này, giai đoạn tiếp theo là viết nội dung báo cáo theo thứ tự:

```
Bước 1: Viết Chương 1 → file BAO_CAO_DO_AN_MUSICAPP.md (markdown trung gian)
Bước 2: Viết Chương 2 → nối tiếp file trên
Bước 3: Viết Chương 3 + Kết luận + TLTK + Phụ lục
Bước 4: Convert Markdown → Word (.docx) bằng pandoc:
        pandoc BAO_CAO_DO_AN_MUSICAPP.md -o BAO_CAO_DO_AN_MUSICAPP.docx \
        --reference-doc=template.docx
Bước 5: Apply font/lề/spacing bằng template.docx chuẩn.
Bước 6: Insert ảnh giao diện thực tế (nếu user cung cấp).
Bước 7: Final QA theo checklist §4.
```

**Câu hỏi cần user xác nhận trước khi viết nội dung:**

1. Có ảnh chụp giao diện thực tế của ứng dụng không? (Cần cho Chương 3 mục 3.2).
2. Tên trường, khoa, GVHD, tên SV, niên khóa?
3. Có dùng `pandoc` để convert Markdown → Word không, hay viết trực tiếp trong Word?

---

## 6. CÁC FILE SẼ TẠO RA

| File | Mô tả |
|---|---|
| `BAO_CAO_DO_AN_MUSICAPP.md` | Bản markdown nguồn (khoảng 45 trang A4 khi in). |
| `BAO_CAO_DO_AN_MUSICAPP.docx` | Bản Word cuối cùng sau khi convert + format. |
| `template.docx` (nếu cần) | Template chuẩn font/lề/spacing dùng cho pandoc. |

> Plan này **không tự động viết nội dung** — chờ user duyệt trước.
