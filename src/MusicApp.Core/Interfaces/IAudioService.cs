using System;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao diện trừu tượng hóa hệ thống âm thanh (Audio Engine Abstraction Layer).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Cung cấp API phát âm thanh và phát sự kiện trạng thái/FFT.
    /// KHÔNG chịu trách nhiệm: Thao tác trực tiếp với phần cứng hay thư viện NAudio.
    /// Vòng đời: Scoped hoặc Singleton. Cần Dispose để giải phóng tài nguyên.
    /// Luồng: Các method điều khiển chạy trên UI thread, event có thể chạy trên background thread.
    /// </remarks>
    public interface IAudioService : IDisposable
    {
        /// <summary>
        /// Trang thai hoat dong hien tai cua he thong am thanh (Stopped, Buffering, Playing, Paused, Faulted).
        /// </summary>
        PlaybackState CurrentState { get; }

        /// <summary>
        /// Vi tri moc thoi gian dang phat hien tai cua ban nhac.
        /// </summary>
        TimeSpan CurrentTime { get; }

        /// <summary>
        /// Tong thoi luong cua ban nhac dang phat.
        /// </summary>
        TimeSpan TotalTime { get; }

        /// <summary>
        /// Muc am luong hien tai cua he thong am thanh (gia tri tu 0.0f den 1.0f).
        /// </summary>
        float Volume { get; }

        /// <summary>
        /// Dich vu dieu chinh can bang tan so DSP Equalizer 10 bang tan.
        /// </summary>
        IDspEqualizerService Equalizer { get; }

        /// <summary>
        /// Su kien phat du lieu pho tan so FFT (Fast Fourier Transform) theo chu ky de hien thi Audio Visualizer tren UI.
        /// </summary>
        event EventHandler<float[]> SpectrumDataReady;

        /// <summary>
        /// Su kien thong bao khi trang thai van hanh cua he thong am thanh thay doi.
        /// </summary>
        event EventHandler<PlaybackState> StateChanged;

        /// <summary>
        /// Khoi tao luong am thanh bat dong bo tu mot URL HTTP mang hoac mot file cuc bo tren o cung.
        /// </summary>
        /// <param name="streamUrl">Duong dan HTTP stream hoac duong dan tuyet doi toi file am thanh.</param>
        Task InitializeAsync(string streamUrl);

        /// <summary>
        /// Bat dau phat hoac tiep tuc phat tu vi tri dang tam dung.
        /// </summary>
        void Play();

        /// <summary>
        /// Tam dung phat tai vi tri thoi gian hien tai.
        /// </summary>
        void Pause();

        /// <summary>
        /// Dung hoan toan qua trinh phat va dua con tro vi tri ve 0.
        /// </summary>
        void Stop();

        /// <summary>
        /// Tua con tro phat den mot moc thoi gian tuy y trong ban nhac.
        /// </summary>
        /// <param name="position">Vi tri thoi gian can tua den.</param>
        void Seek(TimeSpan position);

        /// <summary>
        /// Thiet lap am luong dau ra cua he thong am thanh.
        /// </summary>
        /// <param name="volume">Gia tri am luong tu 0.0f (tat tieng) den 1.0f (lon nhat).</param>
        void SetVolume(float volume);
    }
}
