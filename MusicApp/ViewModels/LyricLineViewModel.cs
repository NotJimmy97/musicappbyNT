using System;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel đại diện cho một dòng lời bài hát trên giao diện Lyrics.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Đóng gói LyricLine, cung cấp trạng thái IsActive để UI highlight, và cung cấp SeekCommand.
    /// 2. Không chịu trách nhiệm: Phân tích cú pháp LRC (thuộc ILyricsService).
    /// 3. Vòng đời: Nằm trong danh sách của LyricsViewModel, được tạo lại khi bài hát thay đổi.
    /// 4. Đa luồng: IsActive được cập nhật nhanh từ UI Dispatcher.
    /// </remarks>
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
