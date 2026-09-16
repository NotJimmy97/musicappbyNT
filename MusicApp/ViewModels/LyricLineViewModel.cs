using System;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel dai dien cho mot dong loi bai hat tren giao dien Lyrics (Synchronized Lyric Line Item).
    /// 
    /// Tac dung:
    /// - Bao boc thuc the LyricLine de cung cap du lieu cho ListBox trong LyricsSyncView.xaml.
    /// - Cung cap thuoc tinh IsActive de Visual Tree ap dung DataTrigger thay doi font chu, mau sac,
    ///   va phong to dong loi hat hien tai (Karaoke Highlight Effect).
    /// - Cung cap lenh SeekCommand cho phep nguoi dung click truc tiep vao dong loi de tua am thanh den dung giay do.
    /// 
    /// Van de giai quyet:
    /// - Nguoi dung nghe nhac thuong co nhu cau nghe lai mot cau hat cu the. Viec ho tro SeekCommand ngay tren dong loi
    ///   mang lai trai nghiem tuong tac cao cap nhu Spotify hoac Apple Music.
    /// - Ke thua ObservableObject de cap nhat IsActive tuc thi khi luong nhac chuyen dong loi moi.
    /// 
    /// Cach thuc van hanh:
    /// - Timestamp, Text va Index duoc lay tu doi tuong bat bien LyricLine.
    /// - SeekCommand thuc thi callback onSeek(Timestamp), delegate nay se goi truc tiep xuong IAudioService.Seek.
    /// </summary>
    public class LyricLineViewModel : ObservableObject
    {
        /// <summary>
        /// Thuc the dong loi bai hat goc ben duoi mien nghiep vu (Core Model).
        /// </summary>
        public LyricLine Line { get; }

        /// <summary>
        /// Moc thoi gian cua cau hat trong ban nhac.
        /// </summary>
        public TimeSpan Timestamp => Line.Timestamp;

        /// <summary>
        /// Noi dung van ban cau hat.
        /// </summary>
        public string Text => Line.Text;

        /// <summary>
        /// Thu tu chi so dong (bat dau tu 0).
        /// </summary>
        public int Index => Line.Index;

        private bool _isActive;

        /// <summary>
        /// Trang thai xac dinh cau hat nay co dang duoc phat tai thoi diem hien tai hay khong.
        /// Khi true, giao dien XAML se to sang chu va cuon man hinh den vi tri cau hat nay.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        /// <summary>
        /// Lenh cho phep nguoi dung click vao cau hat tren giao dien de tua thoi gian phat den moc nay.
        /// </summary>
        public RelayCommand SeekCommand { get; }

        /// <summary>
        /// Khoi tao ViewModel dong loi kem theo callback tua thoi gian.
        /// </summary>
        /// <param name="line">Thuc the LyricLine goc.</param>
        /// <param name="onSeek">Hanh dong callback tua am thanh den thoi diem chi dinh.</param>
        public LyricLineViewModel(LyricLine line, Action<TimeSpan> onSeek)
        {
            Line = line ?? throw new ArgumentNullException(nameof(line));
            SeekCommand = new RelayCommand(_ => onSeek?.Invoke(Timestamp));
        }
    }
}
