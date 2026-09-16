using System;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao dien truu tuong quan ly bo can bang am thanh ky thuat so 10 bang tan (10-Band DSP Equalizer).
    /// 
    /// Tac dung:
    /// - Cung cap cac thao tac dieu khien do loi (Gain dB) cho 10 dai tan so tieu chuan ISO (31Hz den 16kHz).
    /// - Ho tro bat/tat bo loc (Bypass Mode) va thong bao su thay doi cau hinh cho cac thanh phan lien quan.
    /// 
    /// Van de giai quyet:
    /// - Ngan ngua su can thiep truc tiep cua tang ViewModel vao cac he so tinh toan bo loc IIR Bi-quad ben duoi DSP Engine.
    /// - Cho phep nguoi dung tuy bien am sac (Bass Boost, Treble, Vocal, Classical...) ma khong lam gian doan luong audio.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc thuc thi boi DspEqualizerSampleProvider trong MusicApp.AudioEngine.
    /// - Khi SetBandGain hoac SetAllBands duoc goi, bo loc cap nhat he so noi bo ma khong cap phat bo nho moi (Zero Heap Allocation),
    ///   dam bao luong am thanh (Audio Render Thread) chay muot ma khong gap hien tuong khuc xa bo nho hay do tre (Glitch/Pop).
    /// </summary>
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
