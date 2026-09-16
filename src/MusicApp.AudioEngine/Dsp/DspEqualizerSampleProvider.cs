using System;
using NAudio.Dsp;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Bo loc can bang am thanh ky thuat so 10 bang tan (10-Band DSP Parametric Equalizer).
    /// 
    /// Tac dung:
    /// - Ap dung mang cac bo loc IIR bac hai (Bi-quad Peaking EQ Filters) tren tung kenh am thanh (Stereo/Mono).
    /// - Cho phep tang giam do loi (Gain) tu -12dB den +12dB tren 10 dai tan so tieu chuan ISO tu 32Hz den 16kHz.
    /// - Cung cap co che cat am dinh cao (Soft Limiter) nham triet tieu hien tuong meo tieng ky thuat so (Digital Clipping).
    /// 
    /// Van de giai quyet:
    /// - Hieu nang toi uu tren Audio Thread:
    ///   + Khi tat bo loc hoac khi tat ca cac bang tan deu o muc 0 dB (Unity Gain), thuat toan tu dong kich hoat che do
    ///     chuyen tiep truc tiep (Bypass Optimization), bo qua toan bo phep toan ma tran tren 20 bo loc biquad.
    ///   + Khi nguoi dung keo thanh Slider tren UI, phuong thuc SetPeakingEq cap nhat truc tiep he so loc tren doi tuong cu
    ///     (In-place Coefficient Update) ma khong tao ra bat ky doi tuong moi nao tren Heap, tranh nghen luong am thanh.
    /// - An toan toan hoc (Nyquist Frequency Guard): Tan so bang cao nhat (16kHz) co the gay ra hien tuong suy bien toan hoc
    ///   neu tan so lay mau la 32kHz hoac 44.1kHz. Ham ClampFrequencyToNyquist tu dong gioi han tan so toi da o muc 48% SampleRate.
    /// 
    /// Cach thuc van hanh:
    /// - Khoi tao ma tran bo loc _filters[channels][10].
    /// - Trong ham Read, neu co bang tan hoat dong, tin hieu tung mau se duoc bien doi tuan tu qua 10 bo loc biquad.
    /// - Cuoi cung, mau duoc kep trong khoang [-1.0f, +1.0f] truoc khi ghi vao buffer dau ra.
    /// </summary>
    public class DspEqualizerSampleProvider : ISampleProvider
    {
        /// <summary>
        /// So luong bang tan am thanh tieu chuan cua bo can bang (10 bang tan).
        /// </summary>
        public const int BandCount = 10;

        /// <summary>
        /// He so pham chat Q mac dinh (Q = 1.4142f, tuong ung do rong bang thong 1 octave).
        /// </summary>
        public const float DefaultQ = 1.4142f;

        /// <summary>
        /// Mang cac tan so trung tam mac dinh (Hz) theo tieu chuan ISO.
        /// </summary>
        public static readonly float[] DefaultFrequencies = new float[BandCount]
        {
            32f, 64f, 125f, 250f, 500f, 1000f, 2000f, 4000f, 8000f, 16000f
        };

        private readonly ISampleProvider _source;
        private readonly float[] _frequencies = new float[BandCount];
        private readonly float[] _gains = new float[BandCount];
        private readonly bool[] _hasNonZeroGain = new bool[BandCount];
        private readonly object _lock = new object();

        private BiQuadFilter[][] _filters;
        private int _channels;
        private int _sampleRate;
        private bool _isEnabled = true;
        private bool _anyNonZeroGain;

        /// <summary>
        /// Dinh dang am thanh duoc ke thua tu nguon am thanh dau vao.
        /// </summary>
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Trang thai bat/tat bo Equalizer. Khi false, tin hieu se bo qua toan bo tinh toan loc.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                lock (_lock)
                {
                    _isEnabled = value;
                }
            }
        }

        /// <summary>
        /// Khoi tao bo loc DSP Equalizer voi cac tham so tan so va do loi ban dau.
        /// </summary>
        /// <param name="source">Nguon am thanh dau vao.</param>
        /// <param name="initialFrequencies">Mang tan so trung tam tuy chinh (neu null dung DefaultFrequencies).</param>
        /// <param name="initialGains">Mang do loi dB ban dau (neu null mac dinh la 0 dB).</param>
        /// <param name="isEnabled">Trang thai kich hoat ban dau.</param>
        public DspEqualizerSampleProvider(ISampleProvider source, float[] initialFrequencies = null, float[] initialGains = null, bool isEnabled = true)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _isEnabled = isEnabled;

            _channels = _source.WaveFormat.Channels;
            _sampleRate = _source.WaveFormat.SampleRate;

            float[] freqs = initialFrequencies ?? DefaultFrequencies;
            for (int i = 0; i < BandCount; i++)
            {
                _frequencies[i] = i < freqs.Length ? freqs[i] : DefaultFrequencies[i];
            }

            if (initialGains != null)
            {
                for (int i = 0; i < BandCount && i < initialGains.Length; i++)
                {
                    _gains[i] = Math.Max(-12.0f, Math.Min(12.0f, initialGains[i]));
                    _hasNonZeroGain[i] = Math.Abs(_gains[i]) > 0.01f;
                }
            }

            RebuildFilters();
        }

        /// <summary>
        /// Thiet lap do loi (dB) cho mot bang tan cu the.
        /// </summary>
        /// <param name="bandIndex">Chi so bang tan (0 den 9).</param>
        /// <param name="gainDb">Do loi tu -12.0f den +12.0f dB.</param>
        public void SetBandGain(int bandIndex, float gainDb)
        {
            if (bandIndex < 0 || bandIndex >= BandCount)
            {
                return;
            }

            lock (_lock)
            {
                float clampedGain = Math.Max(-12.0f, Math.Min(12.0f, gainDb));
                _gains[bandIndex] = clampedGain;
                _hasNonZeroGain[bandIndex] = Math.Abs(clampedGain) > 0.01f;

                UpdateFilterCoefficients(bandIndex);
                UpdateAnyNonZeroGain();
            }
        }

        /// <summary>
        /// Thiet lap dong thoi do loi cho tat ca cac bang tan (ap dung khi nguoi dung chon Preset).
        /// </summary>
        /// <param name="gains">Mang chua cac gia tri do loi dB.</param>
        public void SetAllBands(float[] gains)
        {
            if (gains == null) return;

            lock (_lock)
            {
                int limit = Math.Min(BandCount, gains.Length);
                for (int i = 0; i < limit; i++)
                {
                    float clamped = Math.Max(-12.0f, Math.Min(12.0f, gains[i]));
                    _gains[i] = clamped;
                    _hasNonZeroGain[i] = Math.Abs(clamped) > 0.01f;
                    UpdateFilterCoefficients(i);
                }
                UpdateAnyNonZeroGain();
            }
        }

        /// <summary>
        /// Doc va bien doi cac mau am thanh thong qua chuoi bo loc IIR Bi-quad.
        /// </summary>
        /// <param name="buffer">Bo dem chua mau am thanh.</param>
        /// <param name="offset">Vi tri bat dau trong bo dem.</param>
        /// <param name="count">So luong mau can doc.</param>
        /// <returns>So luong mau thuc te da duoc xu ly.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);
            if (samplesRead <= 0)
            {
                return samplesRead;
            }

            lock (_lock)
            {
                // Toi uu chuyen tiep (Bypass): Bo qua toan bo phep tinh neu EQ bi tat hoac tat ca cac dai deu la 0 dB
                if (!_isEnabled || !_anyNonZeroGain || _filters == null || _channels <= 0)
                {
                    return samplesRead;
                }

                int channels = _channels;
                for (int i = 0; i < samplesRead; i++)
                {
                    int ch = i % channels;
                    float sample = buffer[offset + i];

                    // Chi thuc hien bien doi tren nhung bang tan co gain khac 0
                    for (int b = 0; b < BandCount; b++)
                    {
                        if (_hasNonZeroGain[b])
                        {
                            sample = _filters[ch][b].Transform(sample);
                        }
                    }

                    // Soft limiter: Ngan ngua tran so am thanh (Clipping / Wrap-around)
                    if (sample > 1.0f) sample = 1.0f;
                    else if (sample < -1.0f) sample = -1.0f;

                    buffer[offset + i] = sample;
                }
            }

            return samplesRead;
        }

        /// <summary>
        /// Khoi tao ma tran bo loc biquad cho tat ca cac kenh va tat ca cac bang tan.
        /// </summary>
        private void RebuildFilters()
        {
            lock (_lock)
            {
                int channels = Math.Max(1, _channels);
                _filters = new BiQuadFilter[channels][];

                for (int ch = 0; ch < channels; ch++)
                {
                    _filters[ch] = new BiQuadFilter[BandCount];
                    for (int b = 0; b < BandCount; b++)
                    {
                        float safeFreq = ClampFrequencyToNyquist(_frequencies[b], _sampleRate);
                        _filters[ch][b] = BiQuadFilter.PeakingEQ(_sampleRate, safeFreq, DefaultQ, _gains[b]);
                    }
                }

                UpdateAnyNonZeroGain();
            }
        }

        /// <summary>
        /// Cap nhat lai he so bo loc tai cho (In-place) ma khong khoi tao lai doi tuong BiQuadFilter.
        /// </summary>
        private void UpdateFilterCoefficients(int bandIndex)
        {
            if (_filters == null) return;

            float safeFreq = ClampFrequencyToNyquist(_frequencies[bandIndex], _sampleRate);
            float gain = _gains[bandIndex];

            for (int ch = 0; ch < _channels; ch++)
            {
                if (_filters[ch] != null && _filters[ch][bandIndex] != null)
                {
                    _filters[ch][bandIndex].SetPeakingEq(_sampleRate, safeFreq, DefaultQ, gain);
                }
            }
        }

        /// <summary>
        /// Cap nhat bien co _anyNonZeroGain de quyet dinh co can chay qua bo loc hay bypass.
        /// </summary>
        private void UpdateAnyNonZeroGain()
        {
            bool any = false;
            for (int i = 0; i < BandCount; i++)
            {
                if (_hasNonZeroGain[i])
                {
                    any = true;
                    break;
                }
            }
            _anyNonZeroGain = any;
        }

        /// <summary>
        /// Kiem soat tan so bo loc de tranh vuot nguong Nyquist (SampleRate / 2).
        /// </summary>
        private static float ClampFrequencyToNyquist(float freq, int sampleRate)
        {
            // Gioi han toi da o 48% SampleRate de tranh loi suy bien so hoc tren bien tan so Nyquist
            float maxSafeFreq = sampleRate * 0.48f;
            return Math.Min(freq, maxSafeFreq);
        }
    }
}
