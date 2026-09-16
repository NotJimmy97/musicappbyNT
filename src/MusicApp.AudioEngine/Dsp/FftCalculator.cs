using System;
using NAudio.Dsp;

namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Bo tinh toan bien doi Fourier nhanh (Fast Fourier Transform - FFT Calculator).
    /// 
    /// Tac dung:
    /// - Chuyen doi tin hieu am thanh tu mien thoi gian (Time Domain Samples) sang mien tan so (Frequency Domain Spectrum).
    /// - Tich hop cua so Hann (Hann Windowing) de giam thieu hien tuong ro ri pho (Spectral Leakage).
    /// - Gom nhom 512 tan so thuc te thanh 16 dai tan so cam nhan (Perceptual Bins) phuc vu hien thi Audio Visualizer tren WPF.
    /// 
    /// Van de giai quyet:
    /// - Nguyen ly Zero Heap Allocation: Toan bo cac bo dem (Complex buffer, Output bins, Window array) deu duoc cap phat
    ///   co dinh mot lan duy nhat khi khoi tao (Pre-allocated Static Buffers), loai bo hoan toan hien tuong cap phat tren
    ///   Large Object Heap (LOH) va ngan chan tinh trang nghen GC trong Audio Render Loop.
    /// - Ty le hoa phi tuyen (Logarithmic Scaling): Tai nguoi cam nhan am luong theo ham logarit, do do ham Calculate
    ///   ap dung cong thuc Log10 ket hop bo gioi han bao hoa (Saturation Clamping [0..35]) de chieu cao cot tren UI giao dong tu nhien.
    /// 
    /// Cach thuc van hanh:
    /// - Nhan mang 1024 mau am thanh tu SampleAggregator.
    /// - Nhan tung mau voi he so cua so Hann da tinh truoc de lam muot hai dau bien.
    /// - Thuc thi thuat toan Radix-2 FFT truc tiep tren mang Complex (In-place FFT).
    /// - Tinh bien do Euclid: Magnitude = Sqrt(Real^2 + Imag^2) cho tung dai tan so va tinh trung binh vao 16 cot.
    /// </summary>
    public class FftCalculator
    {
        /// <summary>
        /// Kich thuoc cua so FFT (1024 mau am thanh).
        /// </summary>
        public const int FftSize = 1024;

        /// <summary>
        /// Bac luy thua cua 2 (2^10 = 1024).
        /// </summary>
        public const int M = 10;

        /// <summary>
        /// So luong cot tan so dau ra de ve visualizer tren giao dien.
        /// </summary>
        public const int BinCount = 16;

        /// <summary>
        /// Gia tri bien do toi da duoc phep vuot qua (tuong ung voi chieu cao toi da cua Slider/ProgressBar UI).
        /// </summary>
        public const float MaxOutputValue = 35.0f;

        // Bo dem so phuc va bo dem dau ra duoc cap phat san de loai bo ap luc don rac GC
        private readonly Complex[] _complexBuffer = new Complex[FftSize];
        private readonly float[] _outputBins = new float[BinCount];
        private readonly float[] _window = new float[FftSize];

        // Cac moc chi so cat tan so phan chia 512 tan so duong thanh 16 dai tan so cam nhan
        private static readonly int[] BinCutoffs = new int[BinCount + 1]
        {
            1, 2, 4, 7, 11, 16, 23, 33, 47, 67, 95, 135, 191, 270, 381, 450, 512
        };

        /// <summary>
        /// Khoi tao bo tinh toan FFT va tinh toan truoc cac he so cua so Hann (Hann Window Coefficients).
        /// </summary>
        public FftCalculator()
        {
            // Tinh toan truoc cua so Hann mot lan duy nhat tranh tinh toan lai nhieu lan trong audio thread
            for (int i = 0; i < FftSize; i++)
            {
                _window[i] = (float)(0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (FftSize - 1))));
            }
        }

        /// <summary>
        /// Thuc hien bien doi FFT va tra ve mang 16 gia tri bien do pho da qua chuan hoa.
        /// </summary>
        /// <param name="inputSamples">Mang cac mau am thanh dau vao o mien thoi gian.</param>
        /// <returns>Mang co dinh gom 16 gia tri bien do dai tan so.</returns>
        public float[] Calculate(float[] inputSamples)
        {
            if (inputSamples == null)
            {
                throw new ArgumentNullException(nameof(inputSamples));
            }

            int count = Math.Min(inputSamples.Length, FftSize);

            // Sao chep va ap dung cua so Hann truc tiep vao bo dem so phuc
            for (int i = 0; i < count; i++)
            {
                _complexBuffer[i].X = inputSamples[i] * _window[i];
                _complexBuffer[i].Y = 0f;
            }

            // Zero-padding cho cac mau con thieu neu do dai input nho hon FftSize
            for (int i = count; i < FftSize; i++)
            {
                _complexBuffer[i].X = 0f;
                _complexBuffer[i].Y = 0f;
            }

            // Thuc thi thuat toan FFT Radix-2 in-place cua NAudio
            FastFourierTransform.FFT(true, M, _complexBuffer);

            // Gom 512 he so pho tan so duong vao 16 dai tan so cam nhan
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

                // Ap dung cong thuc bien doi logarit phi tuyen giup do nhay am thanh tu nhien hon
                float scaled = (float)(Math.Log10(1.0 + (average * 30.0)) * 25.0);

                // Gioi han bien do an toan (Clamping) tranh tran khoang gia tri [0..MaxOutputValue]
                if (scaled < 0f) scaled = 0f;
                if (scaled > MaxOutputValue) scaled = MaxOutputValue;

                _outputBins[binIndex] = scaled;
            }

            return _outputBins;
        }
    }
}
