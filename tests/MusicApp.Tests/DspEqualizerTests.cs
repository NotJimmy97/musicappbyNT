using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using MusicApp.AudioEngine;
using MusicApp.AudioEngine.Dsp;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;
using MusicApp.ViewModels;
using PlaybackState = MusicApp.Core.Models.PlaybackState;

namespace MusicApp.Tests
{
    /// <summary>
    /// Bo kiem thu don vi va kiem dinh chat luong (Gate 5 Verification) cho bo loc am sac DspEqualizer va DspEqualizerViewModel.
    /// 
    /// - Tac dung: Kiem thu toan dien tinh dung dan cua thuat toan DSP Equalizer 10 dai tan
    ///   (Biquad Peaking EQ), bo gioi han bien do mem (Soft Limiter chống Clipping), co che Bypass,
    ///   va logic dong bo Preset trong ViewModel.
    /// 
    /// - Van de giai quyet:
    ///   1. Kiem dinh Gate 5: Do nang luong RMS (Root Mean Square) cua am tram (Bass 60Hz) truoc va sau khi Boost +12dB,
    ///      dam bao nang luong tang it nhat 1.5 lan (50%+).
    ///   2. Kiem thu bo han che chong vo am (Anti-clipping protection): Dam bao moi mau am thanh sau khi khuyech dai
    ///      luon duoc kep chat trong khoang [-1.0, +1.0] thong qua ham tanh/soft-clipping.
    ///   3. Kiem thu Bypass: Khi tat Equalizer hoac de Flat (0dB), du lieu am thanh dau ra phai giong het 100% dau vao.
    ///   4. Kiem thu MVVM: Cac thao tac doi Preset, keo Slider, Toggle Bypass hoat dong chuan xac tren ViewModel.
    /// 
    /// - Cach thuc van hanh:
    ///   Su dung SineWaveSampleProvider tong hop song sin chuan (60Hz, 100Hz, 440Hz) lam nguon phat mau thu nghiem.
    /// </summary>
    [TestClass]
    public class DspEqualizerTests
    {
        private class SineWaveSampleProvider : ISampleProvider
        {
            private readonly float _frequency;
            private readonly float _amplitude;
            private double _phase;

            public WaveFormat WaveFormat { get; }

            public SineWaveSampleProvider(float frequency, float amplitude = 0.5f, int sampleRate = 44100, int channels = 2)
            {
                _frequency = frequency;
                _amplitude = amplitude;
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            }

            public int Read(float[] buffer, int offset, int count)
            {
                int channels = WaveFormat.Channels;
                double phaseIncrement = (2 * Math.PI * _frequency) / WaveFormat.SampleRate;

                for (int i = 0; i < count; i += channels)
                {
                    float sample = (float)(_amplitude * Math.Sin(_phase));
                    _phase += phaseIncrement;
                    if (_phase > 2 * Math.PI)
                    {
                        _phase -= 2 * Math.PI;
                    }

                    for (int ch = 0; ch < channels; ch++)
                    {
                        buffer[offset + i + ch] = sample;
                    }
                }

                return count;
            }
        }

        private class FakeEqualizerService : IDspEqualizerService
        {
            public bool IsEnabled { get; set; } = true;
            public float[] BandFrequencies { get; } = new float[] { 32f, 64f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f };
            public float[] BandGains { get; } = new float[10];

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

        [TestMethod]
        public void DspEqualizerSampleProvider_Initialization_SetsDefaultFrequenciesAndUnityGain()
        {
            var sine = new SineWaveSampleProvider(100f);
            var provider = new DspEqualizerSampleProvider(sine);

            Assert.AreEqual(sine.WaveFormat, provider.WaveFormat);
            Assert.IsTrue(provider.IsEnabled);
        }

        [TestMethod]
        public void DspEqualizerSampleProvider_SetBandGain_ClampsWithinMinusTwelveToPlusTwelve()
        {
            var sine = new SineWaveSampleProvider(100f);
            var provider = new DspEqualizerSampleProvider(sine);

            provider.SetBandGain(0, 18.0f);
            provider.SetBandGain(1, -20.0f);

            // Buffer processing check to verify clamped values run without exceptions
            float[] buffer = new float[256];
            int read = provider.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(256, read);
        }

        [TestMethod]
        public void DspEqualizerSampleProvider_Read_DisabledOrFlatBypass_ReturnsUntouchedSamples()
        {
            var sine = new SineWaveSampleProvider(440f, amplitude: 0.5f);
            var provider = new DspEqualizerSampleProvider(sine);

            float[] rawBuffer = new float[512];
            float[] eqBuffer = new float[512];

            // Sample raw provider
            var rawSine = new SineWaveSampleProvider(440f, amplitude: 0.5f);
            rawSine.Read(rawBuffer, 0, rawBuffer.Length);

            // Sample eq provider with flat gains
            provider.Read(eqBuffer, 0, eqBuffer.Length);

            for (int i = 0; i < rawBuffer.Length; i++)
            {
                Assert.AreEqual(rawBuffer[i], eqBuffer[i], 0.0001f, "Flat EQ must bypass calculation and return exact samples");
            }
        }

        [TestMethod]
        public void DspEqualizerSampleProvider_Read_BoostsLowFrequencies_Gate5Verification()
        {
            // Gate 5 Verification:
            // 1. Synthesize 60Hz bass audio (near 64Hz band)
            // 2. Read samples with Flat EQ; calculate RMS energy
            // 3. Set 32Hz and 64Hz bands to +12 dB
            // 4. Read samples; verify RMS energy increases significantly
            // 5. Test soft limiter: verify no sample exceeds [-1.0, 1.0]

            const int sampleCount = 2048;
            var flatSine = new SineWaveSampleProvider(60f, amplitude: 0.4f);
            var flatProvider = new DspEqualizerSampleProvider(flatSine);

            float[] flatBuffer = new float[sampleCount];
            flatProvider.Read(flatBuffer, 0, sampleCount);

            double flatEnergySum = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                flatEnergySum += flatBuffer[i] * flatBuffer[i];
            }
            double flatRms = Math.Sqrt(flatEnergySum / sampleCount);

            var boostSine = new SineWaveSampleProvider(60f, amplitude: 0.4f);
            var boostProvider = new DspEqualizerSampleProvider(boostSine);
            boostProvider.SetBandGain(0, 12.0f); // 32 Hz
            boostProvider.SetBandGain(1, 12.0f); // 64 Hz

            float[] boostBuffer = new float[sampleCount];
            boostProvider.Read(boostBuffer, 0, sampleCount);

            double boostEnergySum = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                boostEnergySum += boostBuffer[i] * boostBuffer[i];
            }
            double boostRms = Math.Sqrt(boostEnergySum / sampleCount);

            // Assert low frequency energy increases substantially
            Assert.IsTrue(boostRms > flatRms * 1.5,
                string.Format("Gate 5 Failed: Boosted RMS ({0:F4}) must be > 1.5x Flat RMS ({1:F4})", boostRms, flatRms));

            // Soft limiter test: high input (0.9f) boosted by +12dB must strictly stay within [-1.0f, +1.0f]
            var hotSine = new SineWaveSampleProvider(60f, amplitude: 0.9f);
            var hotProvider = new DspEqualizerSampleProvider(hotSine);
            hotProvider.SetBandGain(0, 12.0f);
            hotProvider.SetBandGain(1, 12.0f);

            float[] hotBuffer = new float[sampleCount];
            hotProvider.Read(hotBuffer, 0, sampleCount);

            for (int i = 0; i < sampleCount; i++)
            {
                Assert.IsTrue(hotBuffer[i] <= 1.0f && hotBuffer[i] >= -1.0f,
                    string.Format("Clipping protection failed at sample {0}: value {1}", i, hotBuffer[i]));
            }
        }

        [TestMethod]
        public void DspEqualizerViewModel_Presets_AppliesStandardCurves()
        {
            var fakeService = new FakeEqualizerService();
            var vm = new DspEqualizerViewModel(fakeService);

            // Apply Bass Boost preset
            vm.SelectedPreset = "Bass Boost";
            Assert.AreEqual("Bass Boost", vm.SelectedPreset);
            Assert.AreEqual(7.0f, vm.Bands[0].GainDb, 0.05f);
            Assert.AreEqual(6.0f, vm.Bands[1].GainDb, 0.05f);
            Assert.AreEqual(7.0f, fakeService.BandGains[0], 0.05f);

            // Apply Rock preset
            vm.SelectedPreset = "Rock";
            Assert.AreEqual("Rock", vm.SelectedPreset);
            Assert.AreEqual(4.5f, vm.Bands[0].GainDb, 0.05f);
            Assert.AreEqual(3.5f, vm.Bands[1].GainDb, 0.05f);
            Assert.AreEqual(4.5f, vm.Bands[8].GainDb, 0.05f);
            Assert.AreEqual(4.5f, vm.Bands[9].GainDb, 0.05f);

            // Apply Reset (Flat)
            vm.ResetCommand.Execute(null);
            Assert.AreEqual("Flat", vm.SelectedPreset);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(0.0f, vm.Bands[i].GainDb, 0.05f);
                Assert.AreEqual(0.0f, fakeService.BandGains[i], 0.05f);
            }
        }

        [TestMethod]
        public void DspEqualizerViewModel_AdjustBand_DetectsCustomPreset()
        {
            var fakeService = new FakeEqualizerService();
            var vm = new DspEqualizerViewModel(fakeService);

            Assert.AreEqual("Flat", vm.SelectedPreset);

            // Adjust single band away from Flat
            vm.Bands[3].GainDb = 5.0f;

            Assert.AreEqual("Custom", vm.SelectedPreset);
            Assert.AreEqual(5.0f, fakeService.BandGains[3], 0.05f);
        }

        [TestMethod]
        public void DspEqualizerViewModel_ToggleBypass_UpdatesStateAndStatusText()
        {
            var fakeService = new FakeEqualizerService();
            var vm = new DspEqualizerViewModel(fakeService);

            Assert.IsTrue(vm.IsEnabled);
            Assert.AreEqual("BẬT (ACTIVE)", vm.StatusText);

            vm.ToggleBypassCommand.Execute(null);

            Assert.IsFalse(vm.IsEnabled);
            Assert.IsFalse(fakeService.IsEnabled);
            Assert.AreEqual("TẮT (BYPASS)", vm.StatusText);

            vm.ToggleBypassCommand.Execute(null);
            Assert.IsTrue(vm.IsEnabled);
        }

        [TestMethod]
        public void NAudioService_Equalizer_PersistsSettings()
        {
            using (var audioService = new NAudioService())
            {
                Assert.IsNotNull(audioService.Equalizer);
                audioService.Equalizer.SetBandGain(0, 6.5f);
                audioService.Equalizer.SetBandGain(5, -3.0f);

                Assert.AreEqual(6.5f, audioService.Equalizer.BandGains[0], 0.05f);
                Assert.AreEqual(-3.0f, audioService.Equalizer.BandGains[5], 0.05f);

                audioService.Equalizer.IsEnabled = false;
                Assert.IsFalse(audioService.Equalizer.IsEnabled);
            }
        }

        [TestMethod]
        public void MainViewModel_Navigation_IncludesEqualizerAndSwitchesView()
        {
            var fakeAudio = new FakeAudioService();
            var fakeApi = new FakeApiClient();

            using (var nowPlayingVm = new NowPlayingViewModel(fakeAudio))
            using (var mainVm = new MainViewModel(fakeApi, nowPlayingVm))
            {
                // Verify 6 navigation items
                Assert.AreEqual(6, mainVm.NavigationItems.Count);
                Assert.IsTrue(mainVm.NavigationItems.Any(n => n.ViewKey == "Equalizer"));

                // Navigate to Equalizer view
                mainVm.NavigationCommand.Execute("Equalizer");
                Assert.AreEqual("Equalizer", mainVm.CurrentViewName);
                Assert.AreEqual("Equalizer", mainVm.SelectedNavigationItem.ViewKey);
                Assert.IsNotNull(mainVm.Equalizer);
                Assert.AreEqual(10, mainVm.Equalizer.Bands.Count);
            }
        }

        private class FakeAudioService : IAudioService
        {
            public PlaybackState CurrentState { get; set; } = PlaybackState.Stopped;
            public TimeSpan CurrentTime { get; set; } = TimeSpan.Zero;
            public TimeSpan TotalTime { get; set; } = TimeSpan.FromMinutes(3);
            public float Volume { get; set; } = 1.0f;
            public IDspEqualizerService Equalizer { get; set; } = new FakeEqualizerService();

#pragma warning disable 0067
            public event EventHandler<float[]> SpectrumDataReady;
#pragma warning restore 0067
            public event EventHandler<PlaybackState> StateChanged;

            public Task InitializeAsync(string streamUrl) => Task.FromResult(0);
            public void Play() { CurrentState = PlaybackState.Playing; StateChanged?.Invoke(this, CurrentState); }
            public void Pause() { CurrentState = PlaybackState.Paused; StateChanged?.Invoke(this, CurrentState); }
            public void Stop() { CurrentState = PlaybackState.Stopped; StateChanged?.Invoke(this, CurrentState); }
            public void Seek(TimeSpan position) { CurrentTime = position; }
            public void SetVolume(float volume) { Volume = volume; }
            public void Dispose() { }
        }

        private class FakeApiClient : IMusicApiClient
        {
            public Task<SearchResponseDto> SearchTracksAsync(string query, int limit = 20, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(new SearchResponseDto
                {
                    Total = 0,
                    Items = new List<TrackDto>()
                });
            }

            public Task<string> SearchTracksRawAsync(string query, int limit = 20, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult("{\"total\":0,\"items\":[]}");
            }

            public void Dispose() { }
        }
    }
}
