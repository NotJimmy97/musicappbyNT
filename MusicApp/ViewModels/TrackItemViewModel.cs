using System;
using MusicApp.Core.Common;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel dai dien cho mot ban nhac trong danh sach hien thi (Track Item Presentation Model).
    /// 
    /// Tac dung:
    /// - Dong goi thuc the TrackModel va bo sung cac thuoc tinh phuc vu trinh dien giao dien nhu FormattedDuration (mm:ss),
    ///   anh bia da Freeze, va lenh phat PlayCommand.
    /// - Duoc su dung trong danh sach ket qua tim kiem (MainViewModel), danh sach phat (PlayQueueViewModel),
    ///   va danh sach thu vien offline (LocalLibraryViewModel).
    /// 
    /// Van de giai quyet:
    /// - Tranh viec code logic dinh dang thoi gian hoac xu ly su kien Click trong code-behind cua tung View.
    /// - Gan nut Play truc tiep vao tung dong bai hat thong qua PlayCommand nhan tham so la TrackModel,
    ///   giup nguoi dung chi can click mot lan la co the phat ngay ban nhac mong muon.
    /// 
    /// Cach thuc van hanh:
    /// - Khi PlayCommand duoc goi, no kich hoat callback onPlay(Track) do ViewModel cha (MainViewModel hoac PlayQueueViewModel) truyen vao.
    /// </summary>
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
