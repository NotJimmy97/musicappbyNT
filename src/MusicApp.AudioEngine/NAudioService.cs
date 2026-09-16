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
    /// <summary>
    /// Lop dich vu am thanh trung tam thuc thi boi NAudio (Central NAudio Service Coordinator).
    /// 
    /// Tac dung:
    /// - Hien thuc hoa hai giao dien cot loi: IAudioService (Dieu khien phat nhac) va IDspEqualizerService (Can bang am thanh DSP).
    /// - Xay dung va quan ly vong doi cua do thi xu ly tin hieu am thanh (Audio Graph Pipeline):
    ///   Reader (AudioFileReader / MediaFoundationReader) 
    ///     -> DspEqualizerSampleProvider (10-Band Bi-quad Filter)
    ///       -> SampleAggregator (FFT Calculator) 
    ///         -> WaveOutEvent (Direct Hardware Output).
    /// 
    /// Van de giai quyet:
    /// - Tu dong phan loai nguon am thanh:
    ///   + Voi tap tin cuc bo tren o cung (Local File): Su dung AudioFileReader de doc va giai ma voi do tre bang 0 (Zero Latency).
    ///   + Voi dia chi mang (HTTP Stream): Su dung MediaFoundationReader de ho tro stream chunk va tu dong xu ly cac codec mang.
    /// - Chong nghen hang doi giao dien WPF (Dispatcher Queue Flooding):
    ///   Tan so FFT duoc tinh toan lien tuc trong Audio Thread co the tao ra hang tram event moi giay.
    ///   NAudioService su dung Stopwatch de khao sat toc do (Rate Limiting) va chi phat su kien SpectrumDataReady
    ///   toi da 30 khung hinh moi giay (chu ky 33ms), dam bao giao dien WPF hoat dong muot ma khong bi treo.
    /// - An toan da luong (Thread Safety): Dong bo hoa moi thao tac Play, Pause, Seek, SetVolume va SetBandGain
    ///   thong qua doi tuong khoa _lock, ngan chan xung dot giua Audio Render Thread va UI Dispatcher Thread.
    /// </summary>
    public class NAudioService : IAudioService, IDspEqualizerService
    {
        private IWavePlayer _wavePlayer;
        private WaveStream _audioReader;
        private DspEqualizerSampleProvider _equalizerProvider;
        private SampleAggregator _sampleAggregator;
        private readonly Stopwatch _throttleStopwatch = new Stopwatch();
        private readonly object _lock = new object();
        private bool _isDisposed;

        // Mang luu tru cau hinh tan so va do loi hien tai cua 10 bang tan
        private readonly float[] _bandFrequencies = (float[])DspEqualizerSampleProvider.DefaultFrequencies.Clone();
        private readonly float[] _bandGains = new float[DspEqualizerSampleProvider.BandCount];
        private bool _isEqualizerEnabled = true;

        /// <summary>
        /// Tham chieu toi chinh doi tuong hien tai de thuc thi giao dien IDspEqualizerService.
        /// </summary>
        public IDspEqualizerService Equalizer => this;

        /// <summary>
        /// Trang thai bat/tat bo loc Equalizer.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEqualizerEnabled;
            set
            {
                lock (_lock)
                {
                    if (_isEqualizerEnabled != value)
                    {
                        _isEqualizerEnabled = value;
                        if (_equalizerProvider != null)
                        {
                            _equalizerProvider.IsEnabled = value;
                        }
                    }
                }
                EqualizerChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sao chep an toan mang cac tan so trung tam cua 10 bang tan.
        /// </summary>
        public float[] BandFrequencies
        {
            get
            {
                lock (_lock)
                {
                    return (float[])_bandFrequencies.Clone();
                }
            }
        }

        /// <summary>
        /// Sao chep an toan mang cac gia tri do loi hien tai cua 10 bang tan.
        /// </summary>
        public float[] BandGains
        {
            get
            {
                lock (_lock)
                {
                    return (float[])_bandGains.Clone();
                }
            }
        }

        /// <summary>
        /// Su kien thong bao khi thong so Equalizer duoc thay doi.
        /// </summary>
        public event EventHandler EqualizerChanged;

        private PlaybackState _currentState = PlaybackState.Stopped;

        /// <summary>
        /// Trang thai phat am thanh hien tai cua he thong.
        /// </summary>
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

        /// <summary>
        /// Vi tri thoi gian phat hien tai cua ban nhac.
        /// </summary>
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

        /// <summary>
        /// Tong thoi luong cua ban nhac dang phat.
        /// </summary>
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

        /// <summary>
        /// Muc am luong dau ra hien tai (0.0f den 1.0f).
        /// </summary>
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

        /// <summary>
        /// Su kien phat du lieu bien do 16 cot tan so FFT (duoc gioi han tan suat 30fps).
        /// </summary>
        public event EventHandler<float[]> SpectrumDataReady;

        /// <summary>
        /// Su kien thong bao khi trang thai phat (Playing, Paused, Stopped, Buffering, Faulted) thay doi.
        /// </summary>
        public event EventHandler<PlaybackState> StateChanged;

        /// <summary>
        /// Khoi tao luong am thanh bat dong bo tren luong Worker Thread.
        /// Tu dong don dep phien phat truoc do va thiet lap do thi DSP moi.
        /// </summary>
        /// <param name="streamUrl">Duong dan HTTP stream hoac duong dan file cuc bo tren o dia.</param>
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
                        // Kiem tra xem duong dan co phai la file offline tren he thong tap tin hay khong
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
                            // Doc tap tin am thanh cuc bo bang AudioFileReader
                            var fileReader = new AudioFileReader(streamUrl);
                            _audioReader = fileReader;
                            sampleProvider = fileReader;
                        }
                        else
                        {
                            // Doc luong am thanh truc tuyen qua mang bang MediaFoundationReader
                            var mfReader = new MediaFoundationReader(streamUrl);
                            _audioReader = mfReader;
                            sampleProvider = mfReader.ToSampleProvider();
                        }

                        // Khoi tao nut DSP Equalizer trong do thi am thanh
                        _equalizerProvider = new DspEqualizerSampleProvider(sampleProvider, _bandFrequencies, _bandGains, _isEqualizerEnabled);

                        // Khoi tao nut FFT Sample Aggregator thu thap mau cho Visualizer
                        _sampleAggregator = new SampleAggregator(_equalizerProvider);
                        _sampleAggregator.FftCalculated += OnFftCalculated;

                        // Khoi tao thiet bi phat am thanh WaveOutEvent voi do tre toi uu 100ms va 3 buffers
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

        /// <summary>
        /// Bat dau phat hoac tiep tuc phat tu vi tri hien tai.
        /// </summary>
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

        /// <summary>
        /// Tam dung luong phat am thanh tai moc thoi gian hien tai.
        /// </summary>
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

        /// <summary>
        /// Dung han phat am thanh va dua con tro vi tri ve 0.
        /// </summary>
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

        /// <summary>
        /// Tua con tro phat den vi tri thoi gian cu the, dam bao nam trong gioi han do dai ban nhac.
        /// </summary>
        /// <param name="position">Vi tri can tua den.</param>
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

        /// <summary>
        /// Thiet lap muc am luong cua thiet bi phat.
        /// </summary>
        /// <param name="volume">Gia tri am luong tu 0.0f den 1.0f.</param>
        public void SetVolume(float volume)
        {
            Volume = volume;
        }

        /// <summary>
        /// Dieu chinh do loi cho mot bang tan cu the tren bo loc DSP.
        /// </summary>
        /// <param name="bandIndex">Chi so bang tan (0 den 9).</param>
        /// <param name="gainDb">Muc do loi tinh theo dB (-12dB den +12dB).</param>
        public void SetBandGain(int bandIndex, float gainDb)
        {
            if (bandIndex < 0 || bandIndex >= DspEqualizerSampleProvider.BandCount)
            {
                return;
            }

            lock (_lock)
            {
                float clamped = Math.Max(-12.0f, Math.Min(12.0f, gainDb));
                _bandGains[bandIndex] = clamped;
                _equalizerProvider?.SetBandGain(bandIndex, clamped);
            }

            EqualizerChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Dieu chinh dong thoi toan bo 10 bang tan (dung cho Preset).
        /// </summary>
        /// <param name="gains">Mang 10 gia tri do loi dB.</param>
        public void SetAllBands(float[] gains)
        {
            if (gains == null) return;

            lock (_lock)
            {
                int limit = Math.Min(DspEqualizerSampleProvider.BandCount, gains.Length);
                for (int i = 0; i < limit; i++)
                {
                    _bandGains[i] = Math.Max(-12.0f, Math.Min(12.0f, gains[i]));
                }
                _equalizerProvider?.SetAllBands(gains);
            }

            EqualizerChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Xu ly su kien tinh toan FFT hoan tat tu SampleAggregator.
        /// Gioi han toc do phat su kien o muc toi da 30fps (33ms) de bao ve UI Thread.
        /// </summary>
        private void OnFftCalculated(object sender, float[] bins)
        {
            if (_throttleStopwatch.ElapsedMilliseconds >= 33)
            {
                _throttleStopwatch.Restart();
                SpectrumDataReady?.Invoke(this, bins);
            }
        }

        /// <summary>
        /// Xu ly su kien thiet bi phat am thanh dung do het ban nhac.
        /// </summary>
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

        /// <summary>
        /// Don dep dong bo toan bo tai nguyen am thanh unmanaged va huy dang ky su kien.
        /// </summary>
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

            _equalizerProvider = null;

            if (_audioReader != null)
            {
                _audioReader.Dispose();
                _audioReader = null;
            }
        }

        /// <summary>
        /// Giai phong hoan toan doi tuong NAudioService khi ung dung tat.
        /// </summary>
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
