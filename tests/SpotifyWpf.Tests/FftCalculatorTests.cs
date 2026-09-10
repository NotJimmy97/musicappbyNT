using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpotifyWpf.AudioEngine.Dsp;

namespace SpotifyWpf.Tests
{
    [TestClass]
    public class FftCalculatorTests
    {
        private FftCalculator _calculator;

        [TestInitialize]
        public void Setup()
        {
            _calculator = new FftCalculator();
        }

        [TestMethod]
        public void Calculate_ReturnsExact16Bins()
        {
            // Arrange: 1024 samples of a synthetic 440Hz test sine wave at 44.1kHz sample rate
            float[] samples = CreateSineWaveSamples(frequency: 440.0, sampleRate: 44100, count: 1024, amplitude: 0.8f);

            // Act
            float[] bins = _calculator.Calculate(samples);

            // Assert
            Assert.IsNotNull(bins);
            Assert.AreEqual(16, bins.Length);
        }

        [TestMethod]
        public void Calculate_AllBinsWithinZeroToThirtyFiveRange()
        {
            // Arrange: Generate multiple signal variations (sine, loud impulse, noise, silence)
            var testSignals = new[]
            {
                new float[1024], // Pure silence
                CreateSineWaveSamples(60.0, 44100, 1024, 1.0f), // Loud Bass
                CreateSineWaveSamples(1000.0, 44100, 1024, 1.0f), // Loud Midrange
                CreateSineWaveSamples(8000.0, 44100, 1024, 1.0f), // Loud Treble
                CreateNoiseSamples(1024, 1.0f) // Loud White Noise
            };

            // Act & Assert: All bins across all signals must strictly fall within [0.0, 35.0]
            foreach (float[] signal in testSignals)
            {
                float[] bins = _calculator.Calculate(signal);

                for (int i = 0; i < bins.Length; i++)
                {
                    Assert.IsTrue(bins[i] >= 0.0f, $"Bin [{i}] value {bins[i]} was less than 0.0");
                    Assert.IsTrue(bins[i] <= 35.0f, $"Bin [{i}] value {bins[i]} exceeded maximum 35.0");
                }
            }
        }

        [TestMethod]
        public void Calculate_SilenceYieldsZeroBins()
        {
            // Arrange
            float[] silence = new float[1024];

            // Act
            float[] bins = _calculator.Calculate(silence);

            // Assert
            for (int i = 0; i < bins.Length; i++)
            {
                Assert.AreEqual(0.0f, bins[i], 0.0001f, $"Bin [{i}] should be zero for pure silence");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Calculate_NullInput_ThrowsArgumentNullException()
        {
            // Act
            _calculator.Calculate(null);
        }

        private static float[] CreateSineWaveSamples(double frequency, int sampleRate, int count, float amplitude)
        {
            float[] samples = new float[count];
            double step = 2.0 * Math.PI * frequency / sampleRate;

            for (int i = 0; i < count; i++)
            {
                samples[i] = (float)(Math.Sin(i * step) * amplitude);
            }

            return samples;
        }

        private static float[] CreateNoiseSamples(int count, float amplitude)
        {
            float[] samples = new float[count];
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                samples[i] = (float)((random.NextDouble() * 2.0 - 1.0) * amplitude);
            }

            return samples;
        }
    }
}
