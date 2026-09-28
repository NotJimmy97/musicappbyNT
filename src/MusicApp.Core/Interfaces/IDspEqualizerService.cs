using System;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao diện quản lý bộ cân bằng âm thanh kỹ thuật số 10 băng tần.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Điều khiển độ lợi (Gain dB) cho 10 dải tần số.
    /// KHÔNG chịu trách nhiệm: Xử lý IIR Bi-quad trực tiếp.
    /// Vòng đời: Tồn tại cùng với AudioEngine.
    /// Luồng: Phải thread-safe, không cấp phát bộ nhớ (Zero Allocation) khi gọi trên Audio Render Thread.
    /// </remarks>
    public interface IDspEqualizerService
    {
        /// <summary>
        /// Trang thai kich hoat bo Equalizer. Khi false, tin hieu am thanh se di qua nguyen ban (Bypass).
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Mang cac tan so trung tam cua 10 bang tan tinh theo Hz (31Hz, 62Hz, 125Hz, 250Hz, 500Hz, 1kHz, 2kHz, 4kHz, 8kHz, 16kHz).
        /// </summary>
        float[] BandFrequencies { get; }

        /// <summary>
        /// Mang cac gia tri do loi hien tai cua tung bang tan tinh theo decibel (dB), thong thuong trong khoang [-12dB, +12dB].
        /// </summary>
        float[] BandGains { get; }

        /// <summary>
        /// Su kien kich hoat khi mot hoac nhieu bang tan duoc thay doi gia tri do loi hoac khi bat/tat bo loc.
        /// </summary>
        event EventHandler EqualizerChanged;

        /// <summary>
        /// Thiet lap gia tri do loi cho mot bang tan cu the.
        /// </summary>
        /// <param name="bandIndex">Chi so bang tan tu 0 den 9.</param>
        /// <param name="gainDb">Muc do loi tinh theo dB (-12dB den +12dB).</param>
        void SetBandGain(int bandIndex, float gainDb);

        /// <summary>
        /// Thiet lap dong thoi do loi cho toan bo 10 bang tan (dung khi ap dung cac cau hinh cai dat san - Presets).
        /// </summary>
        /// <param name="gains">Mang chua 10 gia tri do loi tuong ung voi 10 bang tan.</param>
        void SetAllBands(float[] gains);
    }
}
