using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MusicApp.Core.Common;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IMusicApiClient _apiClient;
        private CancellationTokenSource _debounceCts;
        private readonly List<TrackModel> _masterCatalog = new List<TrackModel>();

        public NowPlayingViewModel NowPlaying { get; }
        public ObservableCollection<TrackItemViewModel> SearchResults { get; } = new ObservableCollection<TrackItemViewModel>();

        public ObservableCollection<string> Genres { get; } = new ObservableCollection<string>
        {
            "All", "Synthwave", "Cyberpunk", "Outrun", "Ambient", "Chillout", "Electronic", "Lo-Fi", "Acoustic", "Dance", "Cinematic"
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

        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearSearchCommand { get; }
        public RelayCommand ToggleThemeCommand { get; }
        public RelayCommand FilterGenreCommand { get; }

        public MainViewModel(IMusicApiClient apiClient, NowPlayingViewModel nowPlaying)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            NowPlaying = nowPlaying ?? throw new ArgumentNullException(nameof(nowPlaying));

            // Wire playlist navigation
            NowPlaying.PlayNextAction = PlayNextTrack;
            NowPlaying.PlayPreviousAction = PlayPreviousTrack;

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

        private void ApplyLocalFilters()
        {
            var query = (SearchKeyword ?? "").Trim().ToLowerInvariant();
            var genre = (SelectedGenre ?? "All").Trim().ToLowerInvariant();

            var filtered = _masterCatalog.Where(t =>
            {
                bool genreMatch = genre == "all" || (t.Genre != null && t.Genre.ToLowerInvariant().Contains(genre));
                bool queryMatch = string.IsNullOrEmpty(query) ||
                                  (t.Title != null && t.Title.ToLowerInvariant().Contains(query)) ||
                                  (t.Artist != null && t.Artist.ToLowerInvariant().Contains(query)) ||
                                  (t.Album != null && t.Album.ToLowerInvariant().Contains(query)) ||
                                  (t.Genre != null && t.Genre.ToLowerInvariant().Contains(query));
                return genreMatch && queryMatch;
            }).ToList();

            UpdateSearchResults(filtered);
        }

        private void UpdateSearchResults(IEnumerable<TrackModel> tracks)
        {
            Action update = () =>
            {
                SearchResults.Clear();
                foreach (var track in tracks)
                {
                    SearchResults.Add(new TrackItemViewModel(track, t =>
                    {
                        Task.Run(async () => await NowPlaying.PlayTrackAsync(t).ConfigureAwait(false));
                    }));
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

        private void OnSearchKeywordChanged(string query)
        {
            // Instant local responsiveness
            ApplyLocalFilters();

            // Background debounce query for external API additions
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

        private void ExecuteSearchImmediate()
        {
            ApplyLocalFilters();

            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();

            var token = _debounceCts.Token;
            Task.Run(async () => await ExecuteSearchAsync(SearchKeyword, token).ConfigureAwait(false));
        }

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

        private void PlayNextTrack()
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

            int nextIndex = (currentIndex + 1) % SearchResults.Count;
            var nextTrack = SearchResults[nextIndex].Track;
            Task.Run(async () => await NowPlaying.PlayTrackAsync(nextTrack).ConfigureAwait(false));
        }

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

        public void Dispose()
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            NowPlaying?.Dispose();
            _apiClient?.Dispose();
        }
    }
}
