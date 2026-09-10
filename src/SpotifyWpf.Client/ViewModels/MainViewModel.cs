using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SpotifyWpf.Core.Common;
using SpotifyWpf.Core.Dtos;
using SpotifyWpf.Core.Interfaces;
using SpotifyWpf.Core.Models;

namespace SpotifyWpf.Client.ViewModels
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IMusicApiClient _apiClient;
        private CancellationTokenSource _debounceCts;

        public NowPlayingViewModel NowPlaying { get; }
        public ObservableCollection<TrackItemViewModel> SearchResults { get; } = new ObservableCollection<TrackItemViewModel>();

        private string _searchKeyword = "electronic";
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
        public RelayCommand ToggleThemeCommand { get; }

        public MainViewModel(IMusicApiClient apiClient, NowPlayingViewModel nowPlaying)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            NowPlaying = nowPlaying ?? throw new ArgumentNullException(nameof(nowPlaying));

            SearchCommand = new RelayCommand(_ => ExecuteSearchImmediate());
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());

            // Initial query to populate initial catalog for the user
            ExecuteSearchImmediate();
        }

        private void OnSearchKeywordChanged(string query)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();

            var token = _debounceCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    // Debounce delay 400ms to suppress spam requests during keystrokes
                    await Task.Delay(400, token).ConfigureAwait(false);
                    await ExecuteSearchAsync(query, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // User typed another character, request cancelled gracefully
                }
            }, token);
        }

        private void ExecuteSearchImmediate()
        {
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
                Application.Current?.Dispatcher?.InvokeAsync(() => SearchResults.Clear());
                return;
            }

            IsSearching = true;

            try
            {
                SearchResponseDto result = await _apiClient.SearchTracksAsync(query, 20, token).ConfigureAwait(false);

                if (token.IsCancellationRequested || result == null)
                {
                    return;
                }

                Application.Current?.Dispatcher?.InvokeAsync(() =>
                {
                    SearchResults.Clear();
                    foreach (TrackDto item in result.Items)
                    {
                        var model = new TrackModel
                        {
                            Id = item.Id,
                            Title = item.Title,
                            Artist = item.Artist,
                            Album = item.Album,
                            DurationSeconds = item.DurationSeconds,
                            CoverImageUrl = item.CoverImageUrl,
                            StreamUrl = item.StreamEndpoint.StartsWith("http") ? item.StreamEndpoint : "http://localhost:5245" + item.StreamEndpoint,
                            License = item.License
                        };

                        SearchResults.Add(new TrackItemViewModel(model, track =>
                        {
                            Task.Run(async () => await NowPlaying.PlayTrackAsync(track).ConfigureAwait(false));
                        }));
                    }
                });
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

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            string themeFile = IsDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
            var uri = new Uri($"pack://application:,,,/SpotifyWpf.Client;component/Resources/Themes/{themeFile}", UriKind.Absolute);

            var newDict = new ResourceDictionary { Source = uri };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newDict);
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
