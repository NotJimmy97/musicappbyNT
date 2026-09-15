using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Converters;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;
using MusicApp.ViewModels;

namespace MusicApp.Tests
{
    [TestClass]
    public class ViewModelTests
    {
        private class FakeAudioService : IAudioService
        {
            public PlaybackState CurrentState { get; set; } = PlaybackState.Stopped;
            public TimeSpan CurrentTime { get; set; } = TimeSpan.Zero;
            public TimeSpan TotalTime { get; set; } = TimeSpan.FromMinutes(3);
            public float Volume { get; set; } = 1.0f;
            public IDspEqualizerService Equalizer { get; set; } = new FakeDspEqualizerService();

            public event EventHandler<float[]> SpectrumDataReady;
            public event EventHandler<PlaybackState> StateChanged;

            public bool InitializeCalled { get; private set; }
            public bool PlayCalled { get; private set; }
            public bool PauseCalled { get; private set; }
            public bool StopCalled { get; private set; }
            public TimeSpan? LastSeekPosition { get; private set; }

            public Task InitializeAsync(string streamUrl)
            {
                InitializeCalled = true;
                return Task.FromResult(0);
            }

            public void Play()
            {
                PlayCalled = true;
                CurrentState = PlaybackState.Playing;
                StateChanged?.Invoke(this, PlaybackState.Playing);
            }

            public void Pause()
            {
                PauseCalled = true;
                CurrentState = PlaybackState.Paused;
                StateChanged?.Invoke(this, PlaybackState.Paused);
            }

            public void Stop()
            {
                StopCalled = true;
                CurrentState = PlaybackState.Stopped;
                StateChanged?.Invoke(this, PlaybackState.Stopped);
            }

            public void Seek(TimeSpan position)
            {
                LastSeekPosition = position;
                CurrentTime = position;
            }

            public void SetVolume(float volume)
            {
                Volume = volume;
            }

            public void RaiseSpectrum(float[] bins)
            {
                SpectrumDataReady?.Invoke(this, bins);
            }

            public void Dispose()
            {
            }
        }

        private class FakeApiClient : IMusicApiClient
        {
            public Task<SearchResponseDto> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken)
            {
                var response = new SearchResponseDto
                {
                    Total = 1,
                    Items = new List<TrackDto>
                    {
                        new TrackDto
                        {
                            Id = "test-1",
                            Title = "Unit Test Track",
                            Artist = "Test Artist",
                            Album = "Test Album",
                            DurationSeconds = 180,
                            CoverImageUrl = "https://example.com/cover.jpg",
                            StreamEndpoint = "/api/v1/stream/test-1",
                            License = "CC-BY"
                        }
                    }
                };

                return Task.FromResult(response);
            }

            public Task<string> SearchTracksRawAsync(string query, int limit, CancellationToken cancellationToken)
            {
                return Task.FromResult("{\"total\":1,\"items\":[]}");
            }

            public void Dispose()
            {
            }
        }

        [TestMethod]
        public void NowPlayingViewModel_SetCurrentTrack_UpdatesDurationAndFormattedText()
        {
            var fakeAudio = new FakeAudioService();
            using (var vm = new NowPlayingViewModel(fakeAudio))
            {
                var track = new TrackModel
                {
                    Id = "t1",
                    Title = "Solar Waves",
                    Artist = "Nova",
                    Album = "Cosmos",
                    DurationSeconds = 195
                };

                vm.CurrentTrack = track;

                Assert.AreEqual(195, vm.TrackDurationSeconds);
                Assert.IsTrue(vm.FormattedDuration.Contains("03:15"));
            }
        }

        [TestMethod]
        public void NowPlayingViewModel_StateTransitions_ReflectInViewModelProperties()
        {
            var fakeAudio = new FakeAudioService();
            using (var vm = new NowPlayingViewModel(fakeAudio))
            {
                vm.CurrentTrack = new TrackModel { Id = "t1", Title = "Test", DurationSeconds = 120 };

                fakeAudio.Play();
                Assert.IsTrue(vm.IsPlaying);
                Assert.IsTrue(vm.CanSeek);

                fakeAudio.Pause();
                Assert.IsFalse(vm.IsPlaying);
                Assert.IsTrue(vm.CanSeek);

                fakeAudio.Stop();
                Assert.IsFalse(vm.IsPlaying);
                Assert.IsFalse(vm.CanSeek);
            }
        }

        [TestMethod]
        public void NowPlayingViewModel_EqualizerBins_Contains16Elements()
        {
            var fakeAudio = new FakeAudioService();
            using (var vm = new NowPlayingViewModel(fakeAudio))
            {
                Assert.AreEqual(16, vm.EqualizerBins.Count);
                for (int i = 0; i < 16; i++)
                {
                    Assert.AreEqual(0.0, vm.EqualizerBins[i]);
                }
            }
        }

        [TestMethod]
        public void MainViewModel_ToggleTheme_FlipsThemeFlagAndButtonText()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                bool initialTheme = mainVm.IsDarkTheme;
                string initialText = mainVm.ThemeButtonText;

                mainVm.ToggleThemeCommand.Execute(null);

                Assert.AreNotEqual(initialTheme, mainVm.IsDarkTheme);
                Assert.AreNotEqual(initialText, mainVm.ThemeButtonText);
            }
        }

        [TestMethod]
        public void FrozenImageConverter_NullOrEmptyInput_ReturnsNull()
        {
            var converter = new FrozenImageConverter();

            object nullResult = converter.Convert(null, typeof(object), null, null);
            Assert.IsNull(nullResult);

            object emptyResult = converter.Convert(string.Empty, typeof(object), null, null);
            Assert.IsNull(emptyResult);
        }

        [TestMethod]
        public void MainViewModel_InitialCatalog_Contains24TracksIncludingVietnamese()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                Assert.AreEqual(24, mainVm.SearchResults.Count);
                Assert.IsTrue(mainVm.Genres.Contains("V-Pop"));
                Assert.IsTrue(mainVm.Genres.Contains("Acoustic Việt"));
                Assert.IsTrue(mainVm.Genres.Contains("Nhạc Trịnh"));
            }
        }

        [TestMethod]
        public void MainViewModel_FilterGenreAcousticViet_ReturnsOnlyAcousticVietTracks()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                mainVm.SelectedGenre = "Acoustic Việt";

                Assert.IsTrue(mainVm.SearchResults.Count > 0);
                foreach (var item in mainVm.SearchResults)
                {
                    Assert.AreEqual("Acoustic Việt", item.Track.Genre);
                }
            }
        }

        [TestMethod]
        public void MainViewModel_SearchWithoutDiacritics_MatchesVietnameseAccentedTitle()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                mainVm.SearchKeyword = "ha trang";

                Assert.AreEqual(1, mainVm.SearchResults.Count);
                Assert.AreEqual("Hạ Trắng", mainVm.SearchResults[0].Track.Title);
            }
        }

        [TestMethod]
        public void MainViewModel_InitialNavigationState_IsExploreWithSixNavItems()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                Assert.AreEqual("Explore", mainVm.CurrentViewName);
                Assert.IsNotNull(mainVm.SelectedNavigationItem);
                Assert.AreEqual("Explore", mainVm.SelectedNavigationItem.ViewKey);
                Assert.AreEqual(6, mainVm.NavigationItems.Count);
                Assert.IsTrue(mainVm.NavigationItems.Any(n => n.ViewKey == "Lyrics"));
                Assert.IsTrue(mainVm.NavigationItems.Any(n => n.ViewKey == "Equalizer"));
            }
        }

        [TestMethod]
        public void MainViewModel_NavigationCommand_SwitchesCurrentViewNameAndGenre()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                mainVm.NavigationCommand.Execute("VietnameseMusic");

                Assert.AreEqual("VietnameseMusic", mainVm.CurrentViewName);
                Assert.AreEqual("VietnameseMusic", mainVm.SelectedNavigationItem.ViewKey);
                Assert.AreEqual("Acoustic Việt", mainVm.SelectedGenre);

                mainVm.NavigationCommand.Execute("Explore");

                Assert.AreEqual("Explore", mainVm.CurrentViewName);
                Assert.AreEqual("Explore", mainVm.SelectedNavigationItem.ViewKey);
                Assert.AreEqual("All", mainVm.SelectedGenre);
            }
        }

        [TestMethod]
        public void MainViewModel_NavigationCommand_SwitchesToLocalLibraryAndQueue()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();
            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                mainVm.NavigationCommand.Execute("LocalLibrary");
                Assert.AreEqual("LocalLibrary", mainVm.CurrentViewName);
                Assert.AreEqual("LocalLibrary", mainVm.SelectedNavigationItem.ViewKey);

                mainVm.NavigationCommand.Execute("PlayQueue");
                Assert.AreEqual("PlayQueue", mainVm.CurrentViewName);
                Assert.AreEqual("PlayQueue", mainVm.SelectedNavigationItem.ViewKey);

                mainVm.NavigationCommand.Execute("Lyrics");
                Assert.AreEqual("Lyrics", mainVm.CurrentViewName);
                Assert.AreEqual("Lyrics", mainVm.SelectedNavigationItem.ViewKey);
            }
        }

        private class FakeDspEqualizerService : IDspEqualizerService
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

