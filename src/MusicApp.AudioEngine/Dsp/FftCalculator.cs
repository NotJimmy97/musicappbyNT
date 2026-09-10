using System;
using NAudio.Dsp;

namespace MusicApp.AudioEngine.Dsp
{
    public class FftCalculator
    {
        public const int FftSize = 1024;
        public const int M = 10;
        public const int BinCount = 16;
        public const float MaxOutputValue = 35.0f;

        // Pre-allocated static buffers to eliminate Large Object Heap (LOH) churn and GC pressure
        private readonly Complex[] _complexBuffer = new Complex[FftSize];
        private readonly float[] _outputBins = new float[BinCount];
        private readonly float[] _window = new float[FftSize];

        // Bin boundary index ranges across 512 positive frequency spectrum bins
        private static readonly int[] BinCutoffs = new int[BinCount + 1]
        {
            1, 2, 4, 7, 11, 16, 23, 33, 47, 67, 95, 135, 191, 270, 381, 450, 512
        };

        public FftCalculator()
        {
            // Pre-calculate Hann window coefficients once during instantiation
            for (int i = 0; i < FftSize; i++)
            {
                _window[i] = (float)(0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (FftSize - 1))));
            }
        }

        public float[] Calculate(float[] inputSamples)
        {
            if (inputSamples == null)
            {
                throw new ArgumentNullException(nameof(inputSamples));
            }

            int count = Math.Min(inputSamples.Length, FftSize);

            // Copy and window samples into pre-allocated Complex buffer without any heap allocations
            for (int i = 0; i < count; i++)
            {
                _complexBuffer[i].X = inputSamples[i] * _window[i];
                _complexBuffer[i].Y = 0f;
            }

            for (int i = count; i < FftSize; i++)
            {
                _complexBuffer[i].X = 0f;
                _complexBuffer[i].Y = 0f;
            }

            // In-place Radix-2 FFT execution
            FastFourierTransform.FFT(true, M, _complexBuffer);

            // Aggregate 512 frequency samples into 16 perceptual frequency bins
            for (int binIndex = 0; binIndex < BinCount; binIndex++)
            {
                int start = BinCutoffs[binIndex];
                int end = BinCutoffs[binIndex + 1];
                float sum = 0f;

                for (int i = start; i < end; i++)
                {
                    float real = _complexBuffer[i].X;
                    float imag = _complexBuffer[i].Y;
                    float magnitude = (float)Math.Sqrt((real * real) + (imag * imag));
                    sum += magnitude;
                }

                int binWidth = Math.Max(1, end - start);
                float average = sum / binWidth;

                // Logarithmic scaling with saturation clamping between 0.0 and 35.0 (UI bar height limit)
                float scaled = (float)(Math.Log10(1.0 + (average * 30.0)) * 25.0);

                if (scaled < 0f) scaled = 0f;
                if (scaled > MaxOutputValue) scaled = MaxOutputValue;

                _outputBins[binIndex] = scaled;
            }

            return _outputBins;
        }
    }
}

