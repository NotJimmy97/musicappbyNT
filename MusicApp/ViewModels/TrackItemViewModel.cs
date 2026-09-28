using System;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel đại diện cho một bài hát trong các danh sách UI.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Đóng gói TrackModel, định dạng hiển thị FormattedDuration và cung cấp PlayCommand.
    /// 2. Không chịu trách nhiệm: Trực tiếp phát nhạc (chỉ gọi delegate truyền từ ViewModel cha).
    /// 3. Vòng đời: Tạo mới liên tục trong các thao tác tìm kiếm, danh sách. Dọn dẹp nhờ Garbage Collector.
    /// 4. Đa luồng: Hoạt động hoàn toàn trên UI thread.
    /// </remarks>
    public class TrackItemViewModel : ObservableObject
    {
        /// <summary>
        /// Thuc the ban nhac goc trong mien nghiep vu.
        /// </summary>
        public TrackModel Track { get; }

        /// <summary>
        /// Dinh danh duy nhat cua bai hat.
        /// </summary>
        public string Id => Track.Id;

        /// <summary>
        /// Tieu de bai hat.
        /// </summary>
        public string Title => Track.Title;

        /// <summary>
        /// Ten ca si hoac ban nhac.
        /// </summary>
        public string Artist => Track.Artist;

        /// <summary>
        /// Ten album phat hanh.
        /// </summary>
        public string Album => Track.Album;

        /// <summary>
        /// Tong thoi luong tinh theo giay.
        /// </summary>
        public int DurationSeconds => Track.DurationSeconds;

        /// <summary>
        /// Duong dan anh bia (URL web hoac Base64 Data URI).
        /// </summary>
        public string CoverImageUrl => Track.CoverImageUrl;

        /// <summary>
        /// Duong dan phat am thanh (URL stream HTTP hoac file o cung cuc bo).
        /// </summary>
        public string StreamUrl => Track.StreamUrl;

        /// <summary>
        /// Giay phep ban quyen am nhac.
        /// </summary>
        public string License => Track.License;

        /// <summary>
        /// The loai am nhac.
        /// </summary>
        public string Genre => Track.Genre ?? "Electronic";

        /// <summary>
        /// Chuoi van ban bieu dien thoi luong bai hat da dinh dang (mm:ss hoac hh:mm:ss).
        /// </summary>
        public string FormattedDuration
        {
            get
            {
                var time = TimeSpan.FromSeconds(DurationSeconds);
                return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// Lenh phat ban nhac nay khi nguoi dung nhan nut Play hoac double click tren giao dien.
        /// </summary>
        public RelayCommand PlayCommand { get; }

        /// <summary>
        /// Khoi tao TrackItemViewModel bao boc lay TrackModel kem callback phat nhac.
        /// </summary>
        /// <param name="track">Thuc the bai hat goc.</param>
        /// <param name="onPlay">Callback thuc thi khi bai hat duoc chon phat.</param>
        public TrackItemViewModel(TrackModel track, Action<TrackModel> onPlay)
        {
            Track = track ?? throw new ArgumentNullException(nameof(track));
            PlayCommand = new RelayCommand(_ => onPlay?.Invoke(Track));
        }
    }
}
