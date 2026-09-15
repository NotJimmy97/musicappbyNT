using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using MusicApp.AudioEngine.Dsp;
using MusicApp.Core.Interfaces;
using PlaybackState = MusicApp.Core.Models.PlaybackState;

namespace MusicApp.AudioEngine
{
    public class NAudioService : IAudioService
    {
        private IWavePlayer _wavePlayer;
        private WaveStream _audioReader;
        private SampleAggregator _sampleAggregator;
        private readonly Stopwatch _throttleStopwatch = new Stopwatch();
        private readonly object _lock = new object();
        private bool _isDisposed;

        private PlaybackState _currentState = PlaybackState.Stopped;
        public PlaybackState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    StateChanged?.Invoke(this, value);
                }
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                lock (_lock)
                {
                    return _audioReader != null ? _audioReader.CurrentTime : TimeSpan.Zero;
                }
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                lock (_lock)
                {
                    return _audioReader != null ? _audioReader.TotalTime : TimeSpan.Zero;
                }
            }
        }

        public float Volume
        {
            get => _wavePlayer != null ? _wavePlayer.Volume : 1.0f;
            private set
            {
                if (_wavePlayer != null)
                {
                    _wavePlayer.Volume = Math.Max(0.0f, Math.Min(1.0f, value));
                }
            }
        }

        public event EventHandler<float[]> SpectrumDataReady;
        public event EventHandler<PlaybackState> StateChanged;

        public Task InitializeAsync(string streamUrl)
        {
            if (string.IsNullOrWhiteSpace(streamUrl))
            {
                throw new ArgumentNullException(nameof(streamUrl));
            }

            return Task.Run(() =>
            {
                lock (_lock)
                {
                    CleanupPlaybackResources();
                    CurrentState = PlaybackState.Buffering;

                    try
                    {
                        bool isLocalFile = false;
                        try
                        {
                            isLocalFile = !streamUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                                          !streamUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                                          File.Exists(streamUrl);
                        }
                        catch
                        {
                            isLocalFile = false;
                        }

                        ISampleProvider sampleProvider;
                        if (isLocalFile)
                        {
                            // AudioFileReader provides zero-overhead local decoding for MP3, WAV, AIFF, and AAC files
                            var fileReader = new AudioFileReader(streamUrl);
                            _audioReader = fileReader;
                            sampleProvider = fileReader;
                        }
                        else
                        {
                            // MediaFoundationReader handles HTTP chunk streaming and remote media decoding
                            var mfReader = new MediaFoundationReader(streamUrl);
                            _audioReader = mfReader;
                            sampleProvider = mfReader.ToSampleProvider();
                        }

                        _sampleAggregator = new SampleAggregator(sampleProvider);
                        _sampleAggregator.FftCalculated += OnFftCalculated;

                        var waveOut = new WaveOutEvent
                        {
                            DesiredLatency = 100,
                            NumberOfBuffers = 3
                        };

                        waveOut.PlaybackStopped += OnPlaybackStopped;
                        waveOut.Init(_sampleAggregator);

                        _wavePlayer = waveOut;
                        _throttleStopwatch.Restart();

                        CurrentState = PlaybackState.Stopped;
                    }
                    catch
                    {
                        CurrentState = PlaybackState.Faulted;
                        throw;
                    }
                }
            });
        }

        public void Play()
        {
            lock (_lock)
            {
                if (_wavePlayer != null && (CurrentState == PlaybackState.Stopped || CurrentState == PlaybackState.Paused))
                {
                    _wavePlayer.Play();
                    _throttleStopwatch.Restart();
                    CurrentState = PlaybackState.Playing;
                }
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                if (_wavePlayer != null && CurrentState == PlaybackState.Playing)
                {
                    _wavePlayer.Pause();
                    CurrentState = PlaybackState.Paused;
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_wavePlayer != null)
                {
                    _wavePlayer.Stop();
                    if (_audioReader != null)
                    {
                        _audioReader.Position = 0;
                    }
                    CurrentState = PlaybackState.Stopped;
                }
            }
        }

        public void Seek(TimeSpan position)
        {
            lock (_lock)
            {
                if (_audioReader != null)
                {
                    if (position < TimeSpan.Zero) position = TimeSpan.Zero;
                    if (position > _audioReader.TotalTime) position = _audioReader.TotalTime;

                    _audioReader.CurrentTime = position;
                }
            }
        }

        public void SetVolume(float volume)
        {
            Volume = volume;
        }

        private void OnFftCalculated(object sender, float[] bins)
        {
            // Rate limit spectrum visualizer event dispatches to 30fps (33ms) to avoid saturating WPF Dispatcher
            if (_throttleStopwatch.ElapsedMilliseconds >= 33)
            {
                _throttleStopwatch.Restart();
                SpectrumDataReady?.Invoke(this, bins);
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            lock (_lock)
            {
                if (CurrentState == PlaybackState.Playing)
                {
                    CurrentState = PlaybackState.Stopped;
                }
            }
        }

        private void CleanupPlaybackResources()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.PlaybackStopped -= OnPlaybackStopped;
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }

            if (_sampleAggregator != null)
            {
                _sampleAggregator.FftCalculated -= OnFftCalculated;
                _sampleAggregator = null;
            }

            if (_audioReader != null)
            {
                _audioReader.Dispose();
                _audioReader = null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            lock (_lock)
            {
                CleanupPlaybackResources();
                CurrentState = PlaybackState.Stopped;
            }
        }
    }
}

