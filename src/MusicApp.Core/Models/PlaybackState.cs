namespace MusicApp.Core.Models
{
    /// <summary>
    /// Dinh nghia tap hop cac trang thai van hanh cua bo phat nhac (Audio Engine).
    /// 
    /// Tac dung:
    /// - Cung cap mo hinh may trang thai huu han (Finite State Machine) dong nhat cho toan bo ung dung.
    /// - Cho phep tang ViewModel va Presentation Layer theo doi, phan ung va binding giao dien (Play/Pause/Buffering/Faulted).
    /// 
    /// Van de giai quyet:
    /// - Co lap thu vien am thanh ben thu ba (NAudio) khoi giao dien nguoi dung, tranh phu thuoc chat che vao NAudio.PlaybackState.
    /// - Bo sung cac trang thai chuyen tiep mang tinh chat ung dung nhu Buffering (khi stream mang chua san sang) 
    ///   hoac Faulted (khi gap su co ket noi hoac loi giai ma), giup nguoi dung co phan hoi truc quan ro rang.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc phat ra tu IAudioService thong qua su kien StateChanged.
    /// - MainViewModel lang nghe su kien nay va dong bo hoa trang thai sang NowPlayingViewModel, tu do cap nhat UI WPF.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// Bo phat dang o trang thai dung hoan toan, khong chiem dung thiet bi am thanh hoac con tro vi tri o diem bat dau.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// He thong dang nap du lieu tu mang hoac giai ma buffer, chua the phat ngay lap tuc.
        /// </summary>
        Buffering = 1,

        /// <summary>
        /// Tin hieu am thanh dang duoc phat lien tuc ra thiet bi dau ra (WaveOut/DirectSound).
        /// </summary>
        Playing = 2,

        /// <summary>
        /// Audio stream dang tam dung tai vi tri hien tai, san sang tiep tuc phat ngay khi co lenh.
        /// </summary>
        Paused = 3,

        /// <summary>
        /// Xay ra loi nghiem trong trong qua trinh stream hoac giai ma (vi du: mat mang, URL 404, file hong).
        /// </summary>
        Faulted = 4
    }
}
