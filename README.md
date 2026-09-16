# MusicApp Desktop (musicappbyNT)

Native desktop music player built with Windows Presentation Foundation (WPF) on .NET Framework 4.6.1, powered by an internal OWIN Self-Host Backend-for-Frontend (BFF), NAudio low-latency audio processing engine, and a 10-Band Graphic DSP Equalizer.

---

## Architectural Overview

The application follows a clean, decoupled Modular Monolith architecture based on strict MVVM separation and an in-process BFF communication pattern:

```
[ WPF Desktop Presentation Layer (MVVM) ]
                 |
                 v
   +---------------------------+---------------------------+
   |                           |                           |
   v                           v                           v
[ Local OWIN BFF ]      [ NAudio DSP Engine ]     [ Core Domain & Services ]
- Self-host (port 5245) - BiQuad Peaking EQ       - LrcParser & LyricsService
- Track Search API      - SampleAggregator        - LocalLibraryService (TagLib)
- Range Stream Proxy    - FftCalculator (16 Bins) - PlayQueue & Auto-Advance
- Music Source Router   - AudioFileReader (Local) - Domain Models & Contracts
                        - MediaFoundation (HTTP)
```

---

## Core Capabilities & Features

### 1. Two-Column Desktop Shell & Navigation
- Spotify-style navigation sidebar (220px fixed) with categorized views: Khám Phá, Nhạc Việt Nam, Thư Viện Cá Nhân, Hàng Đợi, Lời Bài Hát, Bộ Chỉnh Âm (EQ).
- Dynamic view host using WPF `ContentControl` paired with dedicated `DataTemplate` resources.
- Real-time internal BFF connectivity status indicator.

### 2. Local OWIN BFF & Audio Streaming
- Self-hosted OWIN HTTP server listening on `http://localhost:5245`.
- Audio streaming proxy with full HTTP `Range` request support (`206 Partial Content`) for instant seeking and chunked byte streaming.
- Composite `MusicSourceRouter` supporting both international catalogs and curated Vietnamese music collections.
- Diacritic-insensitive search matching Vietnamese accented queries without network latency.

### 3. Local Library Scanner & Offline Playback
- Queue-based breadth-first folder scanner traversing local directories safely with defensive exception handling (`UnauthorizedAccessException`, `PathTooLongException`, `SecurityException`).
- ID3 metadata parsing (Title, Artist, Album, Genre, Duration) and cover art extraction via `TagLibSharp 2.2.0`.
- Memory leak prevention using `BitmapImage.Freeze()` inside `FrozenImageConverter`.
- Direct local offline audio decoding through `NAudio.Wave.AudioFileReader`.

### 4. Play Queue & Interactive Drag-and-Drop Reordering
- Interactive drag-and-drop queue management powered by `gong-wpf-dragdrop 2.3.2`.
- "Up Next" list supporting reordering, track removal, queue clearing, and immediate playback.
- Automatic playback advance: `NowPlayingViewModel` auto-dequeues the next upcoming track when the active song finishes.
- Quick "Add to Queue" (`+`) buttons across both remote catalog cards and local library rows.

### 5. Real-Time Synchronized Lyrics (.LRC)
- High-performance, zero-dependency regex `.LRC` parser supporting standard timestamps (`[mm:ss.xx]`), multi-timestamp tags, time offset correction (`[offset:+/-ms]`), and metadata stripping.
- $O(\log N)$ binary search active line tracking.
- Anti-stutter event gating: scroll adjustments dispatch only on active line index transitions, preventing Dispatcher saturation.
- Spotify-style glowing green typography highlight with vertical auto-centering scroll and interactive click-to-seek.
- Dual-source resolution: companion disk files (`{trackPath}.lrc`) and curated embedded catalog lyrics.

### 6. 10-Band Graphic DSP Equalizer
- NAudio `ISampleProvider` filter chain implementing 10 peaking EQ bands at standard ISO octave center frequencies:
  - 32 Hz (Sub-Bass)
  - 64 Hz (Bass)
  - 125 Hz (Low-Mid)
  - 250 Hz (Low Midrange)
  - 500 Hz (Midrange)
  - 1000 Hz (High-Mid)
  - 2000 Hz (Presence)
  - 4000 Hz (Presence)
  - 8000 Hz (Brilliance)
  - 16000 Hz (Air / Top-End)
- Isolated filter memory states per audio channel ($2 \times 10 = 20$ filters for stereo) using `NAudio.Dsp.BiQuadFilter` with $Q = 1.4142$ (one-octave bandwidth).
- Dynamic in-place coefficient mutation via `filter.SetPeakingEq`: zero heap allocations during playback.
- Built-in soft limiter clamping output samples strictly within $[-1.0f, +1.0f]$ to prevent digital overflow wrap-around.
- Audio graph ordering: `Source -> DspEqualizerSampleProvider -> SampleAggregator -> WaveOutEvent`. Equalizer adjustments immediately reflect on the real-time FFT spectrum visualizer.
- Curated presets: Flat, Rock, Pop, Jazz, Classical, Bass Boost, Vocal Boost, with auto-detection of "Custom" curves.

### 7. Real-Time FFT Spectrum Visualizer & Vinyl Animation
- 16-band logarithmic FFT spectrum analyzer matching Spotify-Readme specifications.
- 30 fps (33ms) dispatch rate limiting to ensure smooth 60 fps WPF UI thread rendering.
- Continuous vinyl CD rotation animation during active playback.
- Global Dark and Light theme switching with high-contrast color palettes.

---

## Technical Stack & Dependencies

| Layer / Role | Technology | Version | Purpose |
|---|---|---|---|
| Target Framework | .NET Framework | 4.6.1 | Windows native runtime compatibility |
| Language | C# | 7.3 | Deterministic, strongly-typed codebase |
| UI Framework | WPF / XAML | 4.6.1 | Hardware-accelerated desktop presentation |
| Audio Engine | NAudio | 1.10.0 | Low-latency audio graph, decoding, DSP filters |
| ID3 Metadata | TagLibSharp | 2.2.0 | Audio tag parsing and embedded picture extraction |
| Drag and Drop | gong-wpf-dragdrop | 2.3.2 | WPF drag-and-drop playlist reordering |
| In-Process BFF | Microsoft.Owin.SelfHost | 4.2.2 | OWIN HTTP self-host server |
| Web API | Microsoft.AspNet.WebApi.OwinSelfHost | 5.2.9 | REST endpoints for search and stream proxying |
| Unit Testing | MSTest v2 | 15.9.1 | Test execution and verification gates |

---

## Project Structure

```
MusicApp/
├── MusicApp.sln                               # Master Visual Studio Solution
├── README.md                                  # Architectural documentation and guide
├── .gitignore                                 # Git ignore rules for .NET and Visual Studio
├── MusicApp/                                  # WPF Desktop Application (Presentation)
│   ├── App.xaml / App.xaml.cs                 # Application bootstrap & lifecycle management
│   ├── MainWindow.xaml / MainWindow.xaml.cs   # Master desktop shell (2-column layout)
│   ├── Converters/                            # Value converters (FrozenImage, Visibility, etc.)
│   ├── Resources/Themes/                      # DarkTheme.xaml and LightTheme.xaml
│   ├── ViewModels/                            # Presentation logic (MVVM)
│   │   ├── MainViewModel.cs                   # Root navigation and catalog orchestration
│   │   ├── NowPlayingViewModel.cs             # Transport controls, timeline, volume, FFT
│   │   ├── LocalLibraryViewModel.cs           # Local folder scanning and filtering
│   │   ├── PlayQueueViewModel.cs              # Drag-and-drop queue management
│   │   ├── LyricsViewModel.cs                 # Synchronized lyrics state & gating
│   │   ├── DspEqualizerViewModel.cs           # 10-band EQ settings and presets
│   │   └── EqualizerBandViewModel.cs          # Individual band slider model
│   └── Views/                                 # UserControls
│       ├── SidebarNavigationView.xaml         # 220px fixed navigation sidebar
│       ├── NowPlayingCardView.xaml            # Bottom playback bar with spectrum & CD
│       ├── LocalLibraryScannerView.xaml       # Local collection scanning view
│       ├── PlayQueueView.xaml                 # Up Next queue drawer with drag-drop
│       ├── LyricsSyncView.xaml                # Real-time centered lyrics view
│       └── DspEqualizerView.xaml              # 10-band graphic EQ console
├── src/
│   ├── MusicApp.Core/                         # Platform-agnostic domain layer
│   │   ├── Common/                            # ObservableObject, RelayCommand, AsyncRelayCommand
│   │   ├── Dtos/                              # SearchResponseDto, TrackDto
│   │   ├── Interfaces/                        # IAudioService, IDspEqualizerService, ILyricsService
│   │   ├── Models/                            # TrackModel, LyricLine, PlaybackState
│   │   └── Services/                          # LrcParser, LocalLibraryService, LyricsService
│   ├── MusicApp.AudioEngine/                  # Audio processing and streaming
│   │   ├── Dsp/                               # DSP pipeline components
│   │   │   ├── BiQuadFilter                   # Peaking EQ filter algorithm
│   │   │   ├── DspEqualizerSampleProvider.cs  # 10-band multi-channel equalizer provider
│   │   │   ├── SampleAggregator.cs            # FFT sampling bridge
│   │   │   ├── FftCalculator.cs               # 16-band spectrum calculation
│   │   │   └── SpectrumBin.cs                 # Spectrum bin data structure
│   │   ├── Stream/                            # BufferedHttpWaveStream
│   │   └── NAudioService.cs                   # Audio engine lifecycle and device output
│   └── MusicApp.Bff/                          # Internal BFF service
│       ├── Controllers/                       # TrackController (search, stream endpoints)
│       ├── Providers/                         # MusicSourceRouter, Jamendo, Vietnamese providers
│       ├── Startup.cs                         # OWIN Web API routing configuration
│       └── BffServerHost.cs                   # Self-host startup and shutdown hooks
└── tests/
    └── MusicApp.Tests/                        # Comprehensive test suite (60 tests)
        ├── BffEndpointTests.cs                # HTTP endpoints and streaming tests
        ├── DspEqualizerTests.cs               # EQ coefficients, DSP gain, and Gate 5 verification
        ├── FftCalculatorTests.cs              # FFT bin calculation tests
        ├── LocalLibraryTests.cs               # ID3 extraction and BFS folder scan tests
        ├── LyricsTests.cs                     # LRC parsing and Gate 4 synchronization tests
        ├── PlayQueueTests.cs                  # Drag-drop reordering and Gate 3 auto-advance tests
        ├── RelayCommandTests.cs               # MVVM command execution tests
        └── ViewModelTests.cs                  # Navigation, filtering, and theme switching tests
```

---

## Build and Execution Instructions

### Prerequisites
1. Windows 10 or later.
2. Visual Studio 2017, 2019, or 2022 (Community, Professional, or Enterprise).
3. .NET Framework 4.6.1 Developer Pack.
4. MSBuild version 15.0 or higher.

### Compilation via Command Line (MSBuild)
Open PowerShell or Command Prompt in the repository root directory:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" MusicApp.sln /t:Build /p:Configuration=Debug /v:m
```

### Running Automated Unit Tests (VSTest)
Execute all 60 tests across the BFF, AudioEngine, Core, and ViewModel layers:

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" tests\MusicApp.Tests\bin\Debug\MusicApp.Tests.dll
```

Expected output:
```text
Total tests: 60. Passed: 60. Failed: 0. Skipped: 0.
Test Run Successful.
```

### Launching the Desktop Application
Run the compiled executable directly:

```powershell
.\MusicApp\bin\Debug\MusicApp.exe
```

Upon launch, the application automatically initializes the local OWIN BFF on `http://localhost:5245`, binds the NAudio playback engine, loads the catalog, and opens the master desktop shell.

---

## Engineering Standards & Architecture Rules

- **Strict MVVM**: View code-behind files (`.xaml.cs`) contain zero business logic and are strictly limited to visual tree operations (e.g. `ScrollViewer.ScrollToVerticalOffset` calculations). All commands and state reside in ViewModels.
- **Zero Heap Allocations in Audio Loop**: The audio rendering thread operates with pre-allocated ring buffers. Equalizer filter updates and FFT spectrum calculations allocate 0 bytes on the heap during streaming.
- **Memory Safety**: Embedded image bytes extracted from audio metadata are frozen via `BitmapImage.Freeze()` to eliminate cross-thread memory leaks.
- **Backward Compatibility**: Strictly compiled against .NET Framework 4.6.1 without relying on C# 8.0+ syntax features.
