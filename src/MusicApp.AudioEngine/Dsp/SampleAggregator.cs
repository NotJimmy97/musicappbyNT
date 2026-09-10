using System;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Dsp
{
    public class SampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly FftCalculator _fftCalculator = new FftCalculator();

        // Pre-allocated static buffer for collecting FFT window samples without GC allocations
        private readonly float[] _sampleRingBuffer = new float[FftCalculator.FftSize];
        private int _ringBufferIndex = 0;

        public event EventHandler<float[]> FftCalculated;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public SampleAggregator(ISampleProvider source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            // Audio stream processing loop: Strictly no new float[] or new byte[] allocations allowed here
            for (int i = 0; i < samplesRead; i++)
            {
                _sampleRingBuffer[_ringBufferIndex] = buffer[offset + i];
                _ringBufferIndex++;

                if (_ringBufferIndex >= FftCalculator.FftSize)
                {
                    _ringBufferIndex = 0;
                    float[] bins = _fftCalculator.Calculate(_sampleRingBuffer);
                    FftCalculated?.Invoke(this, bins);
                }
            }

            return samplesRead;
        }
    }
}

