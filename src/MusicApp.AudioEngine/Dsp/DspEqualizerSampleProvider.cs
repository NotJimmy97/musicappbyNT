using System;
using NAudio.Dsp;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Dsp
{
    public class DspEqualizerSampleProvider : ISampleProvider
    {
        public const int BandCount = 10;
        public const float DefaultQ = 1.4142f;

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

        public WaveFormat WaveFormat => _source.WaveFormat;

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

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);
            if (samplesRead <= 0)
            {
                return samplesRead;
            }

            lock (_lock)
            {
                // Bypass filter calculation when disabled or when all bands are at 0 dB unity gain
                if (!_isEnabled || !_anyNonZeroGain || _filters == null || _channels <= 0)
                {
                    return samplesRead;
                }

                int channels = _channels;
                for (int i = 0; i < samplesRead; i++)
                {
                    int ch = i % channels;
                    float sample = buffer[offset + i];

                    for (int b = 0; b < BandCount; b++)
                    {
                        if (_hasNonZeroGain[b])
                        {
                            sample = _filters[ch][b].Transform(sample);
                        }
                    }

                    // Soft limiter to prevent digital wrap-around distortion when excessive gain is applied
                    if (sample > 1.0f) sample = 1.0f;
                    else if (sample < -1.0f) sample = -1.0f;

                    buffer[offset + i] = sample;
                }
            }

            return samplesRead;
        }

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

        private static float ClampFrequencyToNyquist(float freq, int sampleRate)
        {
            // The Nyquist limit is sampleRate / 2; clamp below 0.49 * sampleRate to prevent math singularities
            float maxSafeFreq = sampleRate * 0.48f;
            return Math.Min(freq, maxSafeFreq);
        }
    }
}
