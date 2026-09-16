using System;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao dien truu tuong hoa toan bo he thong am thanh (Audio Engine Abstraction Layer).
    /// 
    /// Tac dung:
    /// - Cung cap API dieu khien phat am thanh: Khoi tao, Phat, Tam dung, Dung, Tua vi tri va Dieu chinh am luong.
    /// - Phat su kien cap nhat trang thai (StateChanged) va du lieu pho tan so FFT thoi gian thuc (SpectrumDataReady).
    /// - Dong vai tro la nut goc ket noi toi bo can bang am thanh ky thuat so (Equalizer).
    /// 
    /// Van de giai quyet:
    /// - Tuan thu chat che nguyen ly Dependency Inversion (DIP): Tang UI va ViewModel chi giao tiep qua Interface,
    ///   hoan toan khong phu thuoc truc tiep vao thu vien NAudio hay phan cung am thanh DirectSound/WaveOut.
    /// - Giup viec kiem thu don vi (Unit Test) de dang thong qua cac Mock Object ma khong can thiet bi phan cung am thanh that.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc thuc thi boi NAudioService trong project MusicApp.AudioEngine.
    /// - Su dung do thi xu ly tin hieu so (DSP Graph):
    ///   Nguon am thanh (File / HTTP Stream) -> DspEqualizerSampleProvider -> SampleAggregator -> WaveOutEvent.
    /// </summary>
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
