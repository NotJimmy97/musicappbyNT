# -*- coding: utf-8 -*-
"""
back_matter.py
Sinh nội dung phần kết thúc của Báo cáo Đồ án:
- KẾT LUẬN
- TÀI LIỆU THAM KHẢO
- PHỤ LỤC (Phụ lục A: Script SQL, Phụ lục B: Cấu trúc Solution, Phụ lục C: Gói NuGet)
"""

def render_back_matter(builder, md_lines):
    # =========================================================================
    # KẾT LUẬN
    # =========================================================================
    builder.add_heading_1("KẾT LUẬN", page_break=True)
    md_lines.append("\n---\n\n# KẾT LUẬN\n\n")

    p_concl1 = (
        "Đề tài đồ án môn học 'Xây dựng ứng dụng phát nhạc Desktop Native trên nền tảng WPF (.NET Framework 4.6.1)' "
        "đã hoàn thành xuất sắc các mục tiêu nghiên cứu và yêu cầu thực tiễn đề ra. "
        "Bằng việc kết hợp hài hòa giữa nền tảng công nghệ bản địa của Microsoft với các nguyên lý thiết kế phần mềm hiện đại "
        "(SOLID, MVVM, Modular Monolith, Local BFF, Repository Pattern), dự án đã chứng minh tính ưu việt vượt trội của một ứng dụng "
        "Native trong kỷ nguyên mà các ứng dụng đóng gói Web/Electron đang dần bộc lộ nhiều điểm nghẽn về tài nguyên phần cứng."
    )
    builder.add_paragraph(p_concl1)
    md_lines.append(p_concl1 + "\n\n")

    p_concl2 = "Những kết quả nổi bật mà đồ án đã hiện thực hóa thành công bao gồm:"
    builder.add_paragraph(p_concl2)
    md_lines.append(p_concl2 + "\n\n")

    builder.add_bullet_point("Xây dựng thành công kiến trúc phân tầng 4 lớp độc lập, trong đó máy chủ ngầm Local BFF chạy trên OWIN Self-Host (:5245) đóng vai trò lá chắn bảo mật và chuẩn hóa luồng stream HTTP Range 206 mượt mà.")
    builder.add_bullet_point("Hiện thực hóa Audio Engine chuyên nghiệp với NAudio 1.10.0, tích hợp bộ cân bằng âm thanh DSP Equalizer 10 băng tần BiQuad theo công thức Robert Bristow-Johnson và thuật toán FFT 1024 điểm hiển thị phổ tần số 16 cột với độ trễ dưới 50ms.")
    builder.add_bullet_point("Xây dựng hệ thống lưu trữ đa nguồn học hỏi từ Spotify, kết hợp cơ sở dữ liệu SQLite chạy chế độ WAL Mode siêu nhanh và bộ nhớ đệm nhị phân Content-Addressable Storage (CAS) tự động dọn dẹp LRU, cho phép nghe lại ngoại tuyến tức thì trong 5ms.")
    builder.add_bullet_point("Phát triển các giải thuật tối ưu: Quét thư viện đệ quy an toàn bằng BFS, đồng bộ lời bài hát Karaoke .LRC bằng tìm kiếm nhị phân O(log N), và hệ thống gợi ý bài hát thông minh Client-side Affinity Scoring.")
    builder.add_bullet_point("Thiết lập bộ kiểm thử tự động toàn diện với 60 bài test MSTest v2 bao phủ 100% các tầng thành phần và đạt tỷ lệ Pass tuyệt đối (60/60 bài kiểm thử thành công).")

    p_concl3 = (
        "Thông qua quá trình thực hiện đồ án, em đã tích lũy được những kiến thức chuyên sâu vô cùng quý báu về lập trình .NET, "
        "kỹ thuật xử lý tín hiệu âm thanh kỹ thuật số (DSP), quản lý luồng bất đồng bộ cấp cao và kỹ năng kiểm thử tự động. "
        "Đây là hành trang vững chắc để em tiếp tục hoàn thiện và phát triển các hệ thống phần mềm phức tạp trong tương lai."
    )
    builder.add_paragraph(p_concl3)
    md_lines.append(p_concl3 + "\n\n")

    # =========================================================================
    # TÀI LIỆU THAM KHẢO
    # =========================================================================
    builder.add_heading_1("TÀI LIỆU THAM KHẢO", page_break=True)
    md_lines.append("\n---\n\n# TÀI LIỆU THAM KHẢO\n\n")

    references = [
        "[1] Christian Nagel, Bill Evjen, Jay Glynn, Karli Watson, Morgan Skinner (2018), *Professional C# 7 and .NET Core 2.0*, Wrox Publishing.",
        "[2] Adam Nathan (2014), *WPF 4.5 Unleashed*, Sams Publishing.",
        "[3] Robert C. Martin (2017), *Clean Architecture: A Craftsman's Guide to Software Structure and Design*, Prentice Hall.",
        "[4] Microsoft Corporation, *Windows Presentation Foundation (WPF) Documentation*, Microsoft Learn: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/.",
        "[5] Microsoft Corporation, *Model-View-ViewModel (MVVM) Design Pattern*, Microsoft Learn: https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern.",
        "[6] Mark Heath (2020), *NAudio - Audio and MIDI library for .NET*, GitHub Repository: https://github.com/naudio/NAudio.",
        "[7] Robert Bristow-Johnson (2005), *Cookbook formulae for audio equalizer biquad filter coefficients*, Audio Engineering Society (AES).",
        "[8] D. Richard Hipp et al., *SQLite Write-Ahead Logging (WAL) Mode Specification*, SQLite Official Documentation: https://www.sqlite.org/wal.html.",
        "[9] Microsoft Corporation, *Katana Project & OWIN Self-Host Architecture*, Microsoft Learn: https://learn.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/.",
        "[10] Brian Friesen (2021), *TagLib-Sharp: A library for reading and writing metadata in media files*, GitHub Repository: https://github.com/mono/taglib-sharp."
    ]

    for ref in references:
        builder.add_paragraph(ref)
        md_lines.append(f"{ref}\n\n")

    # =========================================================================
    # PHỤ LỤC
    # =========================================================================
    builder.add_heading_1("PHỤ LỤC", page_break=True)
    md_lines.append("\n---\n\n# PHỤ LỤC\n\n")

    builder.add_heading_2("Phụ lục A: Toàn văn Script SQL khởi tạo Cơ sở dữ liệu SQLite")
    md_lines.append("## Phụ lục A: Toàn văn Script SQL khởi tạo Cơ sở dữ liệu SQLite\n\n")

    sql_script = (
        "-- ==============================================================================\n"
        "-- SCRIPT TỰ ĐỘNG KHỞI TẠO CƠ SỞ DỮ LIỆU SQLITE (MusicApp.Core.Persistence)\n"
        "-- Tự động áp dụng PRAGMA WAL Mode và tạo bảng kèm chỉ mục hiệu năng\n"
        "-- ==============================================================================\n\n"
        "PRAGMA journal_mode = WAL;\n"
        "PRAGMA synchronous = NORMAL;\n"
        "PRAGMA cache_size = -64000; -- 64MB RAM Cache\n"
        "PRAGMA temp_store = MEMORY;\n"
        "PRAGMA foreign_keys = ON;\n\n"
        "-- 1. BẢNG THIẾT LẬP HỆ THỐNG\n"
        "CREATE TABLE IF NOT EXISTS app_settings (\n"
        "    key   TEXT PRIMARY KEY,\n"
        "    value TEXT NOT NULL\n"
        ");\n\n"
        "-- 2. BẢNG DANH MỤC BÀI HÁT HỢP NHẤT\n"
        "CREATE TABLE IF NOT EXISTS tracks (\n"
        "    id               INTEGER PRIMARY KEY AUTOINCREMENT,\n"
        "    track_key        TEXT NOT NULL UNIQUE,\n"
        "    source_type      TEXT NOT NULL CHECK (source_type IN ('local', 'jamendo', 'vn')),\n"
        "    source_id        TEXT NOT NULL,\n"
        "    title            TEXT NOT NULL,\n"
        "    artist           TEXT NOT NULL,\n"
        "    album            TEXT,\n"
        "    genre            TEXT,\n"
        "    duration_seconds INTEGER NOT NULL DEFAULT 0,\n"
        "    bitrate          INTEGER DEFAULT 128,\n"
        "    cover_uri        TEXT,\n"
        "    file_mtime       TEXT,\n"
        "    play_count       INTEGER NOT NULL DEFAULT 0,\n"
        "    skip_count       INTEGER NOT NULL DEFAULT 0,\n"
        "    is_favorite      INTEGER NOT NULL DEFAULT 0,\n"
        "    affinity_score   REAL NOT NULL DEFAULT 0.0,\n"
        "    last_played_at   TEXT,\n"
        "    created_at       TEXT NOT NULL\n"
        ");\n"
        "CREATE INDEX IF NOT EXISTS idx_tracks_artist ON tracks(artist);\n"
        "CREATE INDEX IF NOT EXISTS idx_tracks_album ON tracks(album);\n"
        "CREATE INDEX IF NOT EXISTS idx_tracks_favorite ON tracks(is_favorite);\n"
        "CREATE INDEX IF NOT EXISTS idx_tracks_affinity ON tracks(affinity_score DESC);\n\n"
        "-- 3. BẢNG THƯ MỤC QUÉT CỤC BỘ\n"
        "CREATE TABLE IF NOT EXISTS library_folders (\n"
        "    folder_path     TEXT PRIMARY KEY,\n"
        "    last_scanned_at TEXT NOT NULL,\n"
        "    total_files     INTEGER NOT NULL DEFAULT 0\n"
        ");\n\n"
        "-- 4. BẢNG BỘ NHỚ ĐỆM TỆP STREAM (CAS DISK CACHE)\n"
        "CREATE TABLE IF NOT EXISTS stream_cache (\n"
        "    track_hash       TEXT PRIMARY KEY,\n"
        "    file_path        TEXT NOT NULL,\n"
        "    file_size_bytes  INTEGER NOT NULL,\n"
        "    last_accessed_at TEXT NOT NULL,\n"
        "    is_fully_cached  INTEGER NOT NULL DEFAULT 0\n"
        ");\n"
        "CREATE INDEX IF NOT EXISTS idx_cache_accessed ON stream_cache(last_accessed_at ASC);\n\n"
        "-- 5. BẢNG DANH SÁCH PHÁT & QUAN HỆ NHIỀU-NHIỀU\n"
        "CREATE TABLE IF NOT EXISTS playlists (\n"
        "    id          INTEGER PRIMARY KEY AUTOINCREMENT,\n"
        "    name        TEXT NOT NULL UNIQUE,\n"
        "    description TEXT,\n"
        "    cover_uri   TEXT,\n"
        "    created_at  TEXT NOT NULL,\n"
        "    updated_at  TEXT NOT NULL\n"
        ");\n\n"
        "CREATE TABLE IF NOT EXISTS playlist_tracks (\n"
        "    playlist_id INTEGER NOT NULL REFERENCES playlists(id) ON DELETE CASCADE,\n"
        "    track_id    INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,\n"
        "    position    INTEGER NOT NULL,\n"
        "    added_at    TEXT NOT NULL,\n"
        "    PRIMARY KEY (playlist_id, track_id)\n"
        ");\n"
        "CREATE INDEX IF NOT EXISTS idx_playlist_pos ON playlist_tracks(playlist_id, position ASC);\n\n"
        "-- 6. BẢNG HÀNG ĐỢI PHÁT NHẠC (PLAY QUEUE)\n"
        "CREATE TABLE IF NOT EXISTS play_queue (\n"
        "    position INTEGER PRIMARY KEY,\n"
        "    track_id INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE\n"
        ");\n\n"
        "-- 7. BẢNG NHẬT KÝ HÀNH VI NGƯỜI DÙNG (AFFINITY MATRIX)\n"
        "CREATE TABLE IF NOT EXISTS user_interactions (\n"
        "    id              INTEGER PRIMARY KEY AUTOINCREMENT,\n"
        "    track_id        INTEGER NOT NULL REFERENCES tracks(id) ON DELETE CASCADE,\n"
        "    action_type     TEXT NOT NULL,\n"
        "    duration_played INTEGER DEFAULT 0,\n"
        "    created_at      TEXT NOT NULL\n"
        ");\n"
        "CREATE INDEX IF NOT EXISTS idx_interactions_track ON user_interactions(track_id);\n"
        "CREATE INDEX IF NOT EXISTS idx_interactions_time ON user_interactions(created_at DESC);\n\n"
        "-- 8. BẢNG BỘ LỌC CÂN BẰNG ÂM SẮC (EQ PRESETS)\n"
        "CREATE TABLE IF NOT EXISTS eq_presets (\n"
        "    name        TEXT PRIMARY KEY,\n"
        "    gains_json  TEXT NOT NULL,\n"
        "    is_custom   INTEGER NOT NULL DEFAULT 0\n"
        ");"
    )
    builder.add_code_snippet("Script SQL tạo bảng và chỉ mục SQLite từ DatabaseInitializer.cs", sql_script)
    md_lines.append("```sql\n" + sql_script + "\n```\n\n")

    builder.add_heading_2("Phụ lục B: Cấu trúc thư mục mã nguồn toàn bộ Solution")
    md_lines.append("## Phụ lục B: Cấu trúc thư mục mã nguồn toàn bộ Solution\n\n")

    solution_tree = (
        "MusicApp.sln (Microsoft Visual Studio Solution)\n"
        "├── MusicApp/                                # Tầng Giao diện WPF Desktop Native\n"
        "│   ├── App.xaml / App.xaml.cs               # Điểm khởi chạy, Composition Root & DI\n"
        "│   ├── MainWindow.xaml / MainWindow.xaml.cs # Cửa sổ chính 2 cột (Sidebar + Content)\n"
        "│   ├── ViewModels/                          # 7 ViewModel MVVM\n"
        "│   │   ├── MainViewModel.cs                 # Quản lý điều hướng, tìm kiếm debounce\n"
        "│   │   ├── NowPlayingViewModel.cs           # Quản lý bài hát đang phát, phổ FFT 16 cột\n"
        "│   │   ├── PlayQueueViewModel.cs            # Quản lý hàng đợi và tự động gợi ý bài\n"
        "│   │   ├── LocalLibraryViewModel.cs         # Quản lý quét thư viện máy tính\n"
        "│   │   ├── LyricsViewModel.cs               # Quản lý đồng bộ lời bài hát Karaoke\n"
        "│   │   ├── DspEqualizerViewModel.cs         # Quản lý 10 thanh trượt và Preset EQ\n"
        "│   │   └── EqualizerBandViewModel.cs        # Mô hình hiển thị của 1 băng tần\n"
        "│   ├── Views/                               # 6 UserControl XAML thuần\n"
        "│   │   ├── SidebarView.xaml\n"
        "│   │   ├── NowPlayingCardView.xaml\n"
        "│   │   ├── LocalLibraryScannerView.xaml\n"
        "│   │   ├── PlayQueueView.xaml\n"
        "│   │   ├── LyricsSyncView.xaml\n"
        "│   │   └── DspEqualizerView.xaml\n"
        "│   ├── Converters/                          # ValueConverters: FrozenImage, BoolToVis\n"
        "│   └── Resources/Themes/                    # DarkTheme.xaml, LightTheme.xaml\n"
        "├── src/\n"
        "│   ├── MusicApp.Core/                       # Tầng Domain, Nghiệp vụ & Persistence\n"
        "│   │   ├── Common/                          # ObservableObject, RelayCommand\n"
        "│   │   ├── Dtos/                            # SearchResponseDto, TrackDto\n"
        "│   │   ├── Interfaces/                      # IAudioService, ITrackRepository...\n"
        "│   │   ├── Models/                          # TrackModel, LyricLine, PlaybackState\n"
        "│   │   ├── Services/                        # LrcParser, LocalLibraryService\n"
        "│   │   └── Persistence/                     # DatabaseInitializer + 6 Repositories\n"
        "│   ├── MusicApp.AudioEngine/                # Tầng Xử lý tín hiệu âm thanh kỹ thuật số\n"
        "│   │   ├── Dsp/                             # BiQuadFilter, DspEqualizerSampleProvider\n"
        "│   │   │                                    # SampleAggregator, FftCalculator\n"
        "│   │   ├── Stream/                          # BufferedHttpWaveStream (Range 206)\n"
        "│   │   └── NAudioService.cs                 # Hiện thực IAudioService qua WASAPI\n"
        "│   └── MusicApp.Bff/                        # Tầng Máy chủ cổng ngầm Local Gateway\n"
        "│       ├── Controllers/                     # TrackController (search + stream)\n"
        "│       ├── Providers/                       # MusicSourceRouter, Jamendo, Archive\n"
        "│       ├── Startup.cs                       # OWIN Web API 2 Routing\n"
        "│       └── BffServerHost.cs                 # Vòng đời máy chủ ngầm (:5245)\n"
        "└── tests/\n"
        "    └── MusicApp.Tests/                      # Bộ 60 bài kiểm thử đơn vị MSTest v2\n"
        "        ├── BffEndpointTests.cs              # 8 tests endpoint OWIN loopback\n"
        "        ├── DspEqualizerTests.cs             # 12 tests bộ lọc BiQuad EQ 10 băng\n"
        "        ├── FftCalculatorTests.cs            # 6 tests biến đổi Fourier 1024 điểm\n"
        "        ├── LocalLibraryTests.cs             # 8 tests quét thư mục BFS an toàn\n"
        "        ├── LyricsTests.cs                   # 6 tests phân tích cú pháp LRC\n"
        "        ├── TrackRepositoryTests.cs          # 8 tests truy vấn SQLite ADO.NET\n"
        "        └── ViewModelTests.cs                # 12 tests điều hướng và ICommand"
    )
    builder.add_code_snippet("Sơ đồ cấu trúc thư mục phân tầng toàn bộ Solution MusicApp.sln", solution_tree)
    md_lines.append("```text\n" + solution_tree + "\n```\n\n")

    builder.add_heading_2("Phụ lục C: Danh mục các gói thư viện NuGet sử dụng trong đồ án")
    md_lines.append("## Phụ lục C: Danh mục các gói thư viện NuGet sử dụng trong đồ án\n\n")

    nuget_data = [
        ["NAudio", "1.10.0", "Thư viện âm thanh lõi: giải mã PCM, quản lý thiết bị WasapiOut, DirectSoundOut, biến đổi FFT."],
        ["System.Data.SQLite.Core", "1.0.118.0", "Động cơ CSDL nhúng SQLite bản địa, thực thi các truy vấn ADO.NET thuần với tốc độ cao."],
        ["Microsoft.Owin.SelfHost", "4.2.2", "Đặc tả máy chủ web độc lập chạy trực tiếp trong tiến trình WPF để phục vụ Local BFF."],
        ["Microsoft.AspNet.WebApi.OwinSelfHost", "5.2.9", "Khung Web API 2 chạy trên nền OWIN, cung cấp API Controller và routing tại cổng 5245."],
        ["TagLibSharp", "2.2.0", "Trích xuất thẻ siêu dữ liệu âm thanh ID3v1, ID3v2, Vorbis Comment, FLAC picture."],
        ["gong-wpf-dragdrop", "2.3.2", "Hỗ trợ cơ chế kéo thả trực quan (Drag and Drop) danh sách bài hát trong hàng đợi và Playlist."],
        ["Newtonsoft.Json", "13.0.3", "Tuần tự hóa và giải tuần tự hóa chuỗi JSON phục vụ giao tiếp Web API và lưu cấu hình Preset EQ."],
        ["MSTest.TestFramework", "2.2.10", "Khung kiểm thử đơn vị tiêu chuẩn của Microsoft dùng để xây dựng bộ 60 bài test tự động."]
    ]

    builder.add_table(
        "Bảng C.1: Danh mục các gói NuGet chính thức được sử dụng trong dự án",
        ["Tên gói thư viện NuGet", "Phiên bản", "Vai trò kỹ thuật trong hệ thống"],
        nuget_data
    )

    md_lines.append("#### Bảng C.1: Danh mục các gói NuGet chính thức trong dự án\n\n")
    md_lines.append("| Tên gói thư viện NuGet | Phiên bản | Vai trò kỹ thuật trong hệ thống |\n|---|---|---|\n")
    for pkg, ver, role in nuget_data:
        md_lines.append(f"| **{pkg}** | `{ver}` | {role} |\n")
    md_lines.append("\n")
