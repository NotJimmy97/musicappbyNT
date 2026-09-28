using System;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Bộ thu thập và gom mẫu âm thanh (Sample Aggregator Decorator Pattern).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Trích xuất và tích luỹ dữ liệu mẫu PCM 32-bit float từ luồng âm thanh để tính toán FFT.
    /// 2. Không chịu trách nhiệm: Phát nhạc ra loa hoặc hiển thị đồ hoạ UI.
    /// 3. Thời gian sống: Tồn tại cùng đồ thị âm thanh (Audio Graph) cho mỗi bài hát.
    /// 4. Đa luồng/Vòng đời: Hàm Read chạy trên Audio Thread, cấm cấp phát đối tượng mới (Zero Allocation). Sự kiện FftCalculated có thể phát ở Audio Thread.
    /// </remarks>
    public class SampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly FftCalculator _fftCalculator = new FftCalculator();

        // Bo dem vong cap phat tinh de tich luy du lieu cua so FFT ma khong can cap phat GC
        private readonly float[] _sampleRingBuffer = new float[FftCalculator.FftSize];
        private int _ringBufferIndex = 0;

        /// <summary>
        /// Su kien phat du lieu pho FFT khi bo dem vong tich luy du 1024 mau am thanh.
        /// </summary>
        public event EventHandler<float[]> FftCalculated;

        /// <summary>
        /// Dinh dang am thanh (Sample Rate, Channels) duoc ke thua nguyen ven tu nguon am thanh goc.
        /// </summary>
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Khoi tao bo gom mau am thanh bao boc lay nguon ISampleProvider.
        /// </summary>
        /// <param name="source">Nguon am thanh dau vao trong do thi DSP.</param>
        public SampleAggregator(ISampleProvider source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Doc mau am thanh vao bo dem dau ra, dong thoi tich luy mau vao bo dem vong phuc vu FFT.
        /// </summary>
        /// <param name="buffer">Bo dem chua mau am thanh dau ra.</param>
        /// <param name="offset">Vi tri bat dau ghi trong bo dem.</param>
        /// <param name="count">So luong mau yeu cau doc.</param>
        /// <returns>So luong mau thuc te doc duoc.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            // Thu thập mẫu, tránh cấp phát bộ nhớ (Zero Allocation) để tối ưu Audio Thread.
            for (int i = 0; i < samplesRead; i++)
            {
                _sampleRingBuffer[_ringBufferIndex] = buffer[offset + i];
                _ringBufferIndex++;

                // Khi tich luy du cua so 1024 mau, thuc hien FFT va reset chi so bo dem vong ve 0
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
