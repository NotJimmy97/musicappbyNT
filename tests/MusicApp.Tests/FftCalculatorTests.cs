using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.AudioEngine.Dsp;

namespace MusicApp.Tests
{
    /// <summary>
    /// Bo kiem thu don vi cho bo tinh toan bien doi Fourier nhanh (FftCalculator).
    /// 
    /// - Tac dung: Kiem thu do chinh xac va tinh on dinh cua thuat toan FFT Cooley-Tukey,
    ///   ham cua so Hann Window va thuat toan phan bo 16 dai am pho (Spectrum Bins).
    /// 
    /// - Van de giai quyet:
    ///   1. Dam bao dau ra luon co dung 16 bins am pho de dong bo hoan hao voi giao dien Spectrum Visualizer.
    ///   2. Kiem thu bien do dau ra: Chieu cao cot am pho luon nam trong pham vi an toan [0.0, 35.0] pixel,
    ///      khong bi vuot nguong gay tran UI ngay ca khi tin hieu dau vao la nhieu trang (White Noise) bien do cuc dai.
    ///   3. Kiem thu trang thai im lang (Silence): Tin hieu 0 cho ra tat ca cac cot bang 0.
    ///   4. Kiem thu phong thu bien dau vao (ArgumentNullException): Chan loi null ngay lap tuc.
    /// 
    /// - Cach thuc van hanh:
    ///   Truyen cac mang mau tin hieu (song sin 440Hz, tieng on trang, tin hieu 0, mang null) vao phuong thuc Calculate().
    /// </summary>
    [TestClass]
    public class FftCalculatorTests
    {
        [TestMethod]
        public void Calculate_ReturnsExact16Bins()
        {
            var calculator = new FftCalculator();
            var dummySamples = new float[1024];

            // Populate synthetic 440Hz sine wave (A4 note) at 44.1kHz sampling rate
            for (int i = 0; i < dummySamples.Length; i++)
            {
                dummySamples[i] = (float)Math.Sin(2.0 * Math.PI * 440.0 * i / 44100.0);
            }

            float[] bins = calculator.Calculate(dummySamples);

            Assert.IsNotNull(bins);
            Assert.AreEqual(16, bins.Length);
        }

        [TestMethod]
        public void Calculate_AllBinsWithinZeroToThirtyFiveRange()
        {
            var calculator = new FftCalculator();
            var random = new Random(42);
            var whiteNoise = new float[1024];

            for (int i = 0; i < whiteNoise.Length; i++)
            {
                whiteNoise[i] = (float)((random.NextDouble() * 2.0) - 1.0);
            }

            float[] bins = calculator.Calculate(whiteNoise);

            for (int b = 0; b < bins.Length; b++)
            {
                Assert.IsTrue(bins[b] >= 0.0f, $"Bin {b} was below 0.0: {bins[b]}");
                Assert.IsTrue(bins[b] <= 35.0f, $"Bin {b} exceeded max height of 35.0: {bins[b]}");
            }
        }

        [TestMethod]
        public void Calculate_SilenceYieldsZeroBins()
        {
            var calculator = new FftCalculator();
            var silence = new float[1024];

            float[] bins = calculator.Calculate(silence);

            for (int b = 0; b < bins.Length; b++)
            {
                Assert.AreEqual(0.0f, bins[b], 0.001f, $"Bin {b} was non-zero for pure silence.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Calculate_NullInput_ThrowsArgumentNullException()
        {
            var calculator = new FftCalculator();
            calculator.Calculate(null);
        }
    }
}

