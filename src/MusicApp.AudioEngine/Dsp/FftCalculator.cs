using System;
using NAudio.Dsp;

namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Bộ tính toán biến đổi Fourier nhanh (Fast Fourier Transform - FFT Calculator).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Chuyển đổi mảng mẫu âm thanh (miền thời gian) sang phổ (miền tần số) bằng thuật toán Radix-2 FFT, áp dụng cửa sổ Hann giảm nhiễu rò rỉ phổ (Spectral Leakage).
    /// 2. Không chịu trách nhiệm: Thu thập luồng âm thanh hoặc giao tiếp trực tiếp với giao diện đồ hoạ.
    /// 3. Thời gian sống: Tồn tại cùng vòng đời của đồ thị âm thanh cho mỗi bài hát.
    /// 4. Đa luồng/Vòng đời: Thường được gọi bởi Audio Thread. Các bộ đệm được cấp phát cố định 1 lần duy nhất (Zero Allocation) để ngăn ngừa giật lag do Garbage Collection.
    /// </remarks>
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
        /// Thực hiện biến đổi FFT và trả về mảng 16 giá trị biên độ phổ. Hàm này chạy trên Audio Thread, không cấp phát heap.
        /// </summary>
        /// <param name="inputSamples">Mảng các mẫu âm thanh đầu vào ở miền thời gian.</param>
        /// <returns>Mảng cố định gồm 16 giá trị biên độ dải tần.</returns>
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
