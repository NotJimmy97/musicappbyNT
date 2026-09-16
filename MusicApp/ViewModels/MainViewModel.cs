using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MusicApp.Core.Common;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel goc dieu phoi toan bo hoat dong cua ung dung WPF (Root Main Coordinator ViewModel).
    /// 
    /// Tac dung:
    /// - Nam giu va dieu phoi cac sub-ViewModel thanh phan:
    ///   + NowPlayingViewModel (Trinh phat nhac & Visualizer).
    ///   + LocalLibraryViewModel (Quet va quan ly thu vien offline).
    ///   + PlayQueueViewModel (Hang doi phat nhac keo tha Drag & Drop).
    ///   + LyricsViewModel (Dong bo loi bai hat Karaoke).
    ///   + DspEqualizerViewModel (Bo can bang am thanh 10 bang tan).
    /// - Quan ly dieu huong giao dien dong (Dynamic ContentControl Navigation) thong qua CurrentViewName va NavigationItems.
    /// - Quan ly co che tim kiem hai lop (Two-tier Search):
    ///   + Lop 1: Loc tuc thi tren danh muc noi bo _masterCatalog (Instant Local Filtering) cho do tre bang 0.
    ///   + Lop 2: Goi tim kiem bat dong bo tu xa toi BFF thong qua IMusicApiClient voi bo tri hoan Debounce 400ms.
    /// - Quan ly chuyen doi giao dien Sang/Toi (Dark/Light Theme) bang cach nap dong ResourceDictionary.
    /// 
    /// Van de giai quyet:
    /// - Ket noi chat che cac module ma van dam bao nguyen ly Loose Coupling: cac View khong can biet nhau ma deu trao doi qua MainViewModel.
    /// - Triet tieu hien tuong man hinh trong (Empty State): Khoi tao ngay danh muc _masterCatalog chua ca nhac Viet Nam va Jamendo.
    /// - Chong spam yeu cau mang: Co che Debounce 400ms giup ung dung khong ban API lien tuc khi nguoi dung dang go phim.
    /// 
    /// Cach thuc van hanh:
    /// - Khi PlayTrack duoc goi tu bat ky dau (Explore, LocalLibrary, PlayQueue), MainViewModel dong thoi kich hoat
    ///   NowPlaying.PlayTrackAsync va Lyrics.LoadLyricsForTrackAsync tren hai Task rieng biet.
    /// - Khi ca khuc ket thuc, PlayNextTrack uu tien lay bai tiep theo trong PlayQueue (Priority 1), neu khong co se chuyen sang SearchResults (Priority 2).
    /// </summary>
    public class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IMusicApiClient _apiClient;
        private CancellationTokenSource _debounceCts;
        private readonly List<TrackModel> _masterCatalog = new List<TrackModel>();

        public NowPlayingViewModel NowPlaying { get; }
        public ObservableCollection<TrackItemViewModel> SearchResults { get; } = new ObservableCollection<TrackItemViewModel>();

        public ObservableCollection<string> Genres { get; } = new ObservableCollection<string>
        {
            "All", "V-Pop", "Acoustic Việt", "Nhạc Trịnh", "Synthwave", "Cyberpunk", "Outrun", "Ambient", "Chillout", "Electronic", "Lo-Fi", "Acoustic", "Dance", "Cinematic"
        };

        private string _selectedGenre = "All";
        public string SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (SetProperty(ref _selectedGenre, value))
                {
                    ApplyLocalFilters();
                }
            }
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    OnSearchKeywordChanged(value);
                }
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        private bool _isDarkTheme = true;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    OnPropertyChanged(nameof(ThemeButtonText));
                }
            }
        }

        public string ThemeButtonText => IsDarkTheme ? "Light Mode" : "Dark Mode";

        public ObservableCollection<NavigationItemViewModel> NavigationItems { get; } = new ObservableCollection<NavigationItemViewModel>
        {
            new NavigationItemViewModel { Title = "Khám Phá", ViewKey = "Explore", IconSymbol = "✦", Category = "MENU CHÍNH" },
            new NavigationItemViewModel { Title = "Nhạc Việt Nam", ViewKey = "VietnameseMusic", IconSymbol = "♫", Category = "MENU CHÍNH" },
            new NavigationItemViewModel { Title = "Thư Viện Cá Nhân", ViewKey = "LocalLibrary", IconSymbol = "☷", Category = "THƯ VIỆN" },
            new NavigationItemViewModel { Title = "Hàng Đợi", ViewKey = "PlayQueue", IconSymbol = "☰", Category = "THƯ VIỆN" },
            new NavigationItemViewModel { Title = "Lời Bài Hát", ViewKey = "Lyrics", IconSymbol = "♫", Category = "TRÌNH PHÁT" },
            new NavigationItemViewModel { Title = "Bộ Chỉnh Âm (EQ)", ViewKey = "Equalizer", IconSymbol = "≡", Category = "TRÌNH PHÁT" }
        };

        private string _currentViewName = "Explore";
        public string CurrentViewName
        {
            get => _currentViewName;
            set
            {
                if (SetProperty(ref _currentViewName, value))
                {
                    OnCurrentViewNameChanged(value);
                }
            }
        }

        private NavigationItemViewModel _selectedNavigationItem;
        public NavigationItemViewModel SelectedNavigationItem
        {
            get => _selectedNavigationItem;
            set
            {
                if (SetProperty(ref _selectedNavigationItem, value) && value != null)
                {
                    CurrentViewName = value.ViewKey;
                }
            }
        }

        public LocalLibraryViewModel LocalLibrary { get; }
        public PlayQueueViewModel PlayQueue { get; }
        public LyricsViewModel Lyrics { get; }
        public DspEqualizerViewModel Equalizer { get; }

        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearSearchCommand { get; }
        public RelayCommand ToggleThemeCommand { get; }
        public RelayCommand FilterGenreCommand { get; }
        public RelayCommand NavigationCommand { get; }
        public RelayCommand OpenEqualizerCommand => new RelayCommand(_ => NavigationCommand.Execute("Equalizer"));
        public RelayCommand EnqueueCommand => PlayQueue.EnqueueCommand;

        public MainViewModel(IMusicApiClient apiClient, NowPlayingViewModel nowPlaying, ILocalLibraryService localLibraryService = null, IDspEqualizerService equalizerService = null)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            NowPlaying = nowPlaying ?? throw new ArgumentNullException(nameof(nowPlaying));

            PlayQueue = new PlayQueueViewModel(PlayTrack);
            var libraryService = localLibraryService ?? new MusicApp.Core.Services.LocalLibraryService();
            LocalLibrary = new LocalLibraryViewModel(libraryService, PlayTrack);

            var lyricsService = new MusicApp.Core.Services.LyricsService();
            Lyrics = new LyricsViewModel(lyricsService, pos => NowPlaying.SeekCommand.Execute(pos.TotalSeconds));
            NowPlaying.PositionChanged = pos => Lyrics.UpdatePosition(pos);

            var eqService = equalizerService ?? nowPlaying.AudioService?.Equalizer ?? new FallbackEqualizerService();
            Equalizer = new DspEqualizerViewModel(eqService);

            // Wire playlist navigation
            NowPlaying.PlayNextAction = PlayNextTrack;
            NowPlaying.PlayPreviousAction = PlayPreviousTrack;

            _selectedNavigationItem = NavigationItems.FirstOrDefault(n => n.ViewKey == "Explore");

            SearchCommand = new RelayCommand(_ => ExecuteSearchImmediate());
            ClearSearchCommand = new RelayCommand(_ =>
            {
                SearchKeyword = "";
                SelectedGenre = "All";
            });
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
            FilterGenreCommand = new RelayCommand(p =>
            {
                if (p is string g)
                {
                    SelectedGenre = g;
                }
            });
            NavigationCommand = new RelayCommand(p =>
            {
                if (p is string key && !string.IsNullOrWhiteSpace(key))
                {
                    CurrentViewName = key;
                }
            });

            // Initialize catalog immediately so user never sees an empty screen
            InitializeMasterCatalog();
            ApplyLocalFilters();
        }

        private void InitializeMasterCatalog()
        {
            var defaultTracks = new List<TrackModel>
            {
                new TrackModel
                {
                    Id = "vn_track_01",
                    Title = "Diễm Xưa",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 251,
                    CoverImageUrl = "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_01",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_02",
                    Title = "Hạ Trắng",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 326,
                    CoverImageUrl = "https://images.unsplash.com/photo-1445985543470-41fdd5c31447?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_02",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_03",
                    Title = "Còn Tuổi Nào Cho Em",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 324,
                    CoverImageUrl = "https://images.unsplash.com/photo-1465847899084-d164df4dedc6?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_03",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_04",
                    Title = "Mưa Hồng",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 297,
                    CoverImageUrl = "https://images.unsplash.com/photo-1515694346937-94d85e41e6f0?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_04",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_05",
                    Title = "Nắng Thủy Tinh",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 318,
                    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_05",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_06",
                    Title = "Biển Nhớ",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 316,
                    CoverImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_06",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_07",
                    Title = "Cát Bụi",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 305,
                    CoverImageUrl = "https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_07",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_08",
                    Title = "Gọi Tên Bốn Mùa",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 398,
                    CoverImageUrl = "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_08",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_09",
                    Title = "Một Cõi Đi Về",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 278,
                    CoverImageUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_09",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_10",
                    Title = "Như Cánh Vạc Bay",
                    Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                    Album = "Gọi Tên Bốn Mùa",
                    DurationSeconds = 368,
                    CoverImageUrl = "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_10",
                    License = "CC BY-ND 4.0",
                    Genre = "Acoustic Việt"
                },
                new TrackModel
                {
                    Id = "vn_track_11",
                    Title = "Chiều Tây Đô",
                    Artist = "Hương Lan",
                    Album = "Tình Ca Quê Hương",
                    DurationSeconds = 275,
                    CoverImageUrl = "https://images.unsplash.com/photo-1528127269322-539801943592?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_11",
                    License = "Public Domain",
                    Genre = "V-Pop"
                },
                new TrackModel
                {
                    Id = "vn_track_12",
                    Title = "Ướt Mi",
                    Artist = "Hà Thanh",
                    Album = "Trịnh Công Sơn Tuyển Chọn",
                    DurationSeconds = 300,
                    CoverImageUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/vn_track_12",
                    License = "Public Domain",
                    Genre = "Nhạc Trịnh"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1849201",
                    Title = "Neon Skyline",
                    Artist = "Synthwave Collective",
                    Album = "Cybernetic Horizons",
                    DurationSeconds = 195,
                    CoverImageUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1849201",
                    License = "CC BY-SA 3.0",
                    Genre = "Synthwave"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1885627",
                    Title = "Cybernetic Pulse",
                    Artist = "Digital Dreamers",
                    Album = "Grid Runner 2099",
                    DurationSeconds = 210,
                    CoverImageUrl = "https://images.unsplash.com/photo-1508700115892-45ecd05ae2ad?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1885627",
                    License = "CC BY 4.0",
                    Genre = "Cyberpunk"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1795325",
                    Title = "Midnight Drive",
                    Artist = "Retrograde",
                    Album = "Outrun Aesthetics",
                    DurationSeconds = 180,
                    CoverImageUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1795325",
                    License = "CC BY-NC 3.0",
                    Genre = "Outrun"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1802908",
                    Title = "Solar Flares",
                    Artist = "Cosmic Sound",
                    Album = "Astral Horizon",
                    DurationSeconds = 225,
                    CoverImageUrl = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1802908",
                    License = "CC BY 3.0",
                    Genre = "Ambient"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1800109",
                    Title = "Starlight Reverie",
                    Artist = "Astral Waves",
                    Album = "Nebula Dreamscapes",
                    DurationSeconds = 204,
                    CoverImageUrl = "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1800109",
                    License = "CC BY-SA 4.0",
                    Genre = "Chillout"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1765089",
                    Title = "Quantum Echoes",
                    Artist = "Hyperdrive",
                    Album = "Subatomic Shift",
                    DurationSeconds = 192,
                    CoverImageUrl = "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1765089",
                    License = "CC BY 3.0",
                    Genre = "Electronic"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1532761",
                    Title = "Deep Space Meditation",
                    Artist = "Mind Odyssey",
                    Album = "Inner Universe",
                    DurationSeconds = 240,
                    CoverImageUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1532761",
                    License = "CC BY 4.0",
                    Genre = "Lo-Fi"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1668862",
                    Title = "Summer Breeze",
                    Artist = "Acoustic Dreams",
                    Album = "Warm Horizons",
                    DurationSeconds = 175,
                    CoverImageUrl = "https://images.unsplash.com/photo-1465847899084-d164df4dedc6?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1668862",
                    License = "CC BY-NC 3.0",
                    Genre = "Acoustic"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1393276",
                    Title = "Electro Rush",
                    Artist = "Bass Velocity",
                    Album = "Club Ignition",
                    DurationSeconds = 198,
                    CoverImageUrl = "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1393276",
                    License = "CC BY 4.0",
                    Genre = "Dance"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1214935",
                    Title = "Rainy City",
                    Artist = "LoFi Beats",
                    Album = "Midnight Study Session",
                    DurationSeconds = 160,
                    CoverImageUrl = "https://images.unsplash.com/photo-1515694346937-94d85e41e6f0?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1214935",
                    License = "CC BY-SA 3.0",
                    Genre = "Lo-Fi"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1118121",
                    Title = "Epic Horizons",
                    Artist = "Cinematic Orchestra",
                    Album = "The Legend Begins",
                    DurationSeconds = 230,
                    CoverImageUrl = "https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1118121",
                    License = "CC BY 3.0",
                    Genre = "Cinematic"
                },
                new TrackModel
                {
                    Id = "jamendo_track_1855932",
                    Title = "Tokyo Neon Night",
                    Artist = "Future Retro",
                    Album = "Shinjuku Lights",
                    DurationSeconds = 215,
                    CoverImageUrl = "https://images.unsplash.com/photo-1503899036084-c55cdd92da26?w=300&q=80",
                    StreamUrl = "http://localhost:5245/api/v1/stream/jamendo_track_1855932",
                    License = "CC BY 4.0",
                    Genre = "Synthwave"
                }
            };

            _masterCatalog.Clear();
            _masterCatalog.AddRange(defaultTracks);
        }

        /// <summary>
        /// Loc danh sach bai hat tuc thi trong danh muc noi bo theo tu khoa va the loai.
        /// </summary>
        private void ApplyLocalFilters()
        {
            var rawQuery = (SearchKeyword ?? "").Trim().ToLowerInvariant();
            var queryNorm = RemoveDiacritics(rawQuery);
            var rawGenre = (SelectedGenre ?? "All").Trim().ToLowerInvariant();
            var genreNorm = RemoveDiacritics(rawGenre);

            var filtered = _masterCatalog.Where(t =>
            {
                string tGenreNorm = RemoveDiacritics(t.Genre ?? string.Empty).ToLowerInvariant();
                bool genreMatch = genreNorm == "all" || tGenreNorm.Contains(genreNorm);

                if (string.IsNullOrEmpty(queryNorm))
                {
                    return genreMatch;
                }

                string titleNorm = RemoveDiacritics(t.Title ?? string.Empty).ToLowerInvariant();
                string artistNorm = RemoveDiacritics(t.Artist ?? string.Empty).ToLowerInvariant();
                string albumNorm = RemoveDiacritics(t.Album ?? string.Empty).ToLowerInvariant();

                bool queryMatch = titleNorm.Contains(queryNorm) ||
                                  artistNorm.Contains(queryNorm) ||
                                  albumNorm.Contains(queryNorm) ||
                                  tGenreNorm.Contains(queryNorm);

                return genreMatch && queryMatch;
            }).ToList();

            UpdateSearchResults(filtered);
        }

        /// <summary>
        /// Xu ly khi view hien tai duoc thay doi thong qua Sidebar Navigation.
        /// </summary>
        private void OnCurrentViewNameChanged(string viewKey)
        {
            if (SelectedNavigationItem?.ViewKey != viewKey)
            {
                var matched = NavigationItems.FirstOrDefault(n => n.ViewKey == viewKey);
                if (matched != null)
                {
                    SetProperty(ref _selectedNavigationItem, matched, nameof(SelectedNavigationItem));
                }
            }

            if (viewKey == "VietnameseMusic")
            {
                SelectedGenre = "Acoustic Việt";
            }
            else if (viewKey == "Explore")
            {
                SelectedGenre = "All";
            }

            System.Diagnostics.Debug.WriteLine("[Navigation] Current view changed to: " + viewKey);
        }

        /// <summary>
        /// Phat ban nhac duoc chi dinh va tu dong nap loi dong bo.
        /// </summary>
        /// <param name="track">Ban nhac can phat.</param>
        public void PlayTrack(TrackModel track)
        {
            if (track != null)
            {
                Task.Run(async () => await NowPlaying.PlayTrackAsync(track).ConfigureAwait(false));
                Task.Run(async () => await Lyrics.LoadLyricsForTrackAsync(track).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Cap nhat danh sach ket qua tim kiem tren giao dien WPF.
        /// </summary>
        private void UpdateSearchResults(IEnumerable<TrackModel> tracks)
        {
            Action update = () =>
            {
                SearchResults.Clear();
                foreach (var track in tracks)
                {
                    SearchResults.Add(new TrackItemViewModel(track, PlayTrack));
                }
            };

            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.InvokeAsync(update);
            }
            else
            {
                update();
            }
        }

        /// <summary>
        /// Xu ly khi tu khoa tim kiem thay doi kem theo co che Debounce 400ms.
        /// </summary>
        private void OnSearchKeywordChanged(string query)
        {
            // Phan hoi tuc thi tren danh muc cuc bo
            ApplyLocalFilters();

            // Tranh spam yeu cau mang: doi 400ms sau khi nguoi dung ngung go moi goi API
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();

            var token = _debounceCts.Token;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(400, token).ConfigureAwait(false);
                    await ExecuteSearchAsync(query, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }, token);
        }

        /// <summary>
        /// Thuc hien tim kiem ngay lap tuc khi nhan nut Enter hoac nut Search.
        /// </summary>
        private void ExecuteSearchImmediate()
        {
            ApplyLocalFilters();

            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();

            var token = _debounceCts.Token;
            Task.Run(async () => await ExecuteSearchAsync(SearchKeyword, token).ConfigureAwait(false));
        }

        /// <summary>
        /// Gui yeu cau tim kiem bat dong bo toi may chu BFF API.
        /// </summary>
        private async Task ExecuteSearchAsync(string query, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            IsSearching = true;

            try
            {
                SearchResponseDto result = await _apiClient.SearchTracksAsync(query, 20, token).ConfigureAwait(false);

                if (token.IsCancellationRequested || result == null || result.Items == null || result.Items.Count == 0)
                {
                    return;
                }

                var models = new List<TrackModel>();
                foreach (var item in result.Items)
                {
                    models.Add(new TrackModel
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Artist = item.Artist,
                        Album = item.Album,
                        DurationSeconds = item.DurationSeconds,
                        CoverImageUrl = item.CoverImageUrl,
                        StreamUrl = item.StreamEndpoint.StartsWith("http") ? item.StreamEndpoint : "http://localhost:5245" + item.StreamEndpoint,
                        License = item.License,
                        Genre = item.Genre ?? "Electronic"
                    });
                }

                UpdateSearchResults(models);
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine("Search error: " + ex.Message);
                }
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// Chuyen toi phat bai hat tiep theo: Uu tien 1 tu PlayQueue, Uu tien 2 xoay vong trong SearchResults.
        /// </summary>
        private void PlayNextTrack()
        {
            // Uu tien 1: Lay bai hat tiep theo trong hang doi PlayQueue neu co
            var nextQueued = PlayQueue?.DequeueNext();
            if (nextQueued != null)
            {
                PlayTrack(nextQueued);
                return;
            }

            // Uu tien 2: Fallback chuyen bai ke tiep trong danh sach hien tai (vong tron)
            if (SearchResults.Count == 0) return;

            int currentIndex = -1;
            for (int i = 0; i < SearchResults.Count; i++)
            {
                if (SearchResults[i].Track.Id == NowPlaying.CurrentTrack?.Id)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % SearchResults.Count;
            var nextTrack = SearchResults[nextIndex].Track;
            PlayTrack(nextTrack);
        }

        /// <summary>
        /// Quay lai phat ca khuc phia truoc trong danh sach.
        /// </summary>
        private void PlayPreviousTrack()
        {
            if (SearchResults.Count == 0) return;

            int currentIndex = -1;
            for (int i = 0; i < SearchResults.Count; i++)
            {
                if (SearchResults[i].Track.Id == NowPlaying.CurrentTrack?.Id)
                {
                    currentIndex = i;
                    break;
                }
            }

            int prevIndex = (currentIndex - 1 + SearchResults.Count) % SearchResults.Count;
            var prevTrack = SearchResults[prevIndex].Track;
            Task.Run(async () => await NowPlaying.PlayTrackAsync(prevTrack).ConfigureAwait(false));
        }

        /// <summary>
        /// Chuyen doi chu de giao dien Sang / Toi (Dark/Light Mode) thong qua MergedDictionaries.
        /// </summary>
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            string themeFile = IsDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
            var uri = new Uri($"pack://application:,,,/MusicApp;component/Resources/Themes/{themeFile}", UriKind.Absolute);

            var newDict = new ResourceDictionary { Source = uri };
            if (Application.Current != null && Application.Current.Resources != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(newDict);
            }
        }

        /// <summary>
        /// Thuat toan loai bo dau tieng Viet Unicode FormD phuc vu tim kiem chinh xac.
        /// </summary>
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }

        /// <summary>
        /// Giai phong cac tai nguyen va ViewModel con khi MainViewModel bi huy.
        /// </summary>
        public void Dispose()
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            NowPlaying?.Dispose();
            _apiClient?.Dispose();
        }

        /// <summary>
        /// Lop trien khai du phong IDspEqualizerService khi chay kiem thu hoac khi Audio Engine chua san sang.
        /// </summary>
        private class FallbackEqualizerService : IDspEqualizerService
        {
            public bool IsEnabled { get; set; } = true;
            public float[] BandFrequencies { get; set; } = new float[] { 32f, 64f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f };
            public float[] BandGains { get; set; } = new float[10];
            public event EventHandler EqualizerChanged;

            public void SetBandGain(int bandIndex, float gainDb)
            {
                if (bandIndex >= 0 && bandIndex < BandGains.Length)
                {
                    BandGains[bandIndex] = gainDb;
                    EqualizerChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public void SetAllBands(float[] gains)
            {
                if (gains != null)
                {
                    for (int i = 0; i < Math.Min(BandGains.Length, gains.Length); i++)
                    {
                        BandGains[i] = gains[i];
                    }
                    EqualizerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
