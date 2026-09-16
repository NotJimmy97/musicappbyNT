using System;
using NAudio.Wave;

namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Bo thu thap va gom mau am thanh (Sample Aggregator Decorator Pattern).
    /// 
    /// Tac dung:
    /// - Nam trong chuoi xu ly do thi am thanh (DSP Graph), chen giua bo loc Equalizer va WaveOutEvent.
    /// - Trich xuat du lieu mau am thanh (Float Samples) khi chung di qua ham Read ma khong lam thay doi tin hieu am thanh goc.
    /// - Tich luy cac mau vao bo dem vong (Ring Buffer) de kich hoat tinh toan FFT khi du 1024 mau.
    /// 
    /// Van de giai quyet:
    /// - Giu cho audio pipeline tiep tuc phat lien tuc trong khi van co the trich xuat du lieu cho do hoa (Visualization).
    /// - Tuyet doi tuan thu quy tac Zero Allocation: Khong khoi tao bat ky mang byte hay float nao ben trong vong lap Read,
    ///   giup luong phat am thanh duy tri do tre thap nhat (Low Latency) va on dinh nhat.
    /// 
    /// Cach thuc van hanh:
    /// - Goi _source.Read de nap am thanh tu bo loc phia truoc.
    /// - Sao chep tung mau vao _sampleRingBuffer. Khi chi so cham moc FftCalculator.FftSize (1024),
    ///   goi FftCalculator.Calculate va phat su kien FftCalculated cho tang quan ly NAudioService.
    /// </summary>
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

            // Vong lap xu ly dong chay am thanh: Nghiem cam cap phat new float[] hoac new byte[] tai day
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
