namespace MusicApp.AudioEngine.Dsp
{
    /// <summary>
    /// Cau truc du lieu bieu dien mot cot pho tan so (Frequency Spectrum Bin).
    /// 
    /// Tac dung:
    /// - Luu tru chi so cot (Index), tan so dai dien tinh theo Hz (FrequencyHz) va bien do nang luong (Value).
    /// - Duoc su dung lam kieu du lieu gia tri (Value Type Struct) nhe nhang phuc vu bo tinh toan FFT va ve do thi song.
    /// 
    /// Van de giai quyet:
    /// - Khi phan tich pho am thanh voi toc do 30 den 60 khung hinh moi giay, viec khoi tao cac Class doi tuong tren Heap
    ///   se lien tuc kich hoat bo don rac (Garbage Collection GC) gay ra hien tuong khuc xa am thanh (Audio Glitches).
    /// - Dinh nghia duoi dang Struct giup cap phat bo nho truc tiep tren Stack hoac mang lien tuc, loai bo hoan toan ap luc GC.
    /// 
    /// Cach thuc van hanh:
    /// - FftCalculator tinh toan bien do tu cac he so Complex FFT va nap vao tung SpectrumBin tuong ung.
    /// </summary>
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
