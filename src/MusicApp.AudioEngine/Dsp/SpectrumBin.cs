namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Cấu trúc dữ liệu biểu diễn một cột phổ tần số (Frequency Spectrum Bin).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Lưu trữ chỉ số, tần số (Hz) và biên độ năng lượng của một dải phổ.
    /// 2. Không chịu trách nhiệm: Tính toán FFT hay vẽ đồ hoạ UI.
    /// 3. Thời gian sống: Rất ngắn, cấp phát trên Stack (Value Type).
    /// 4. Đa luồng/Vòng đời: An toàn khi truyền qua các luồng do là value type, không gây áp lực dọn rác (Zero GC Allocation).
    /// </remarks>
    public struct SpectrumBin
    {
        /// <summary>
        /// Chi so thu tu cua cot tan so trong dai 16 cot hien thi (0 den 15).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Tan so trung tam cua dai tinh theo Hertz (Hz).
        /// </summary>
        public float FrequencyHz { get; set; }

        /// <summary>
        /// Gia tri bien do nang luong da qua chuan hoa va ty le logarit (0.0f den 35.0f).
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Khoi tao mot cot pho tan so voi day du thong so.
        /// </summary>
        /// <param name="index">Thu tu cot.</param>
        /// <param name="frequencyHz">Tan so trung tam (Hz).</param>
        /// <param name="value">Bien do nang luong.</param>
        public SpectrumBin(int index, float frequencyHz, float value)
        {
            Index = index;
            FrequencyHz = frequencyHz;
            Value = value;
        }
    }
}
