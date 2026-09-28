namespace MusicApp.Core.Models
{
    /// <summary>
    /// Định nghĩa tập hợp các trạng thái vận hành của bộ phát nhạc (Audio Engine).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Cung cấp State Machine cho toàn bộ ứng dụng.
    /// KHÔNG chịu trách nhiệm: Phản ánh trạng thái thư viện mạng.
    /// Vòng đời: Dùng như kiểu giá trị Enum.
    /// Ràng buộc: Giá trị Enum được dùng trên UI và Audio Thread để binding.
    /// </remarks>
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
