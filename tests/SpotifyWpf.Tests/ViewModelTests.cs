using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpotifyWpf.Client.Converters;
using SpotifyWpf.Client.ViewModels;
using SpotifyWpf.Core.Dtos;
using SpotifyWpf.Core.Interfaces;
using SpotifyWpf.Core.Models;

namespace SpotifyWpf.Tests
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
    }
}
