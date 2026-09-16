using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;

namespace MusicApp.Bff.Providers
{
    /// <summary>
    /// Bo dinh tuyen va dieu phoi cac nha cung cap nguon am nhac (Music Source Router Coordinator).
    /// 
    /// Tac dung:
    /// - Dong vai tro la Facade tap trung ket noi hai nha cung cap: JamendoSourceProvider va VietnameseMusicSourceProvider.
    /// - Phan tich y dinh tim kiem cua nguoi dung (Intent Recognition) de uu tien ket qua Viet Nam hoac quoc te phu hop.
    /// - Thuc thi tim kiem song song (Parallel Search) thong qua Task.WhenAll va hop nhat ket qua khong trung lap (Deduplication).
    /// - Phan giai ma bai hat de tra ve dung duong dan stream audio cua nha cung cap tuong ung.
    /// 
    /// Van de giai quyet:
    /// - Giam thoi gian cho cua nguoi dung: Thay vi goi tuan tu tung provider, Router ban hai Task tim kiem song song.
    /// - Nhan dien thong minh cac tu khoa tieng Viet ("trinh", "acoustic", "guitar", "que huong", "vpop",...)
    ///   de uu tien hien thi nhac Viet truoc, neu chua du so luong moi lay them tu Jamendo.
    /// - Loai bo cac ban ghi trung lap dua tren Id bai hat thong qua GroupBy va Select First.
    /// 
    /// Cach thuc van hanh:
    /// - Neu chuoi query rong, ghep danh sach mac dinh cua ca hai provider.
    /// - Kiem tra isVietnameseIntent: neu dung se uu tien lay Vietnamese truoc, phan con lai moi goi Jamendo.
    /// - Neu la tim kiem tong quat, chay song song Task.WhenAll, ghep hai danh sach ket qua va cat theo gioi han safeLimit.
    /// - Khi nhan yeu cau phat audio, neu Id bat dau bang "vn_track_" thi chuyen cho Vietnamese, nguoc lai chuyen cho Jamendo.
    /// </summary>
    public class MusicSourceRouter
    {
        /// <summary>
        /// Nha cung cap am nhac quoc te Jamendo.
        /// </summary>
        public JamendoSourceProvider Jamendo { get; }

        /// <summary>
        /// Nha cung cap am nhac Viet Nam dac tuyen.
        /// </summary>
        public VietnameseMusicSourceProvider Vietnamese { get; }

        /// <summary>
        /// Khoi tao bo dinh tuyen nguon nhac va cac provider thanh phan.
        /// </summary>
        public MusicSourceRouter()
        {
            Jamendo = new JamendoSourceProvider();
            Vietnamese = new VietnameseMusicSourceProvider();
        }

        /// <summary>
        /// Thuc thi tim kiem thong minh ket hop giua nhac Viet Nam va nhac quoc te Jamendo.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem.</param>
        /// <param name="limit">So luong ban ghi toi da yeu cau.</param>
        /// <param name="cancellationToken">Token huy tac vu.</param>
        /// <returns>Danh sach cac doi tuong TrackDto da hop nhat va loai bo trung lap.</returns>
        public async Task<List<TrackDto>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
        {
            int safeLimit = limit > 0 ? limit : 20;

            // Truong hop nguoi dung khong nhap tu khoa (Home/Kham pha): ghep danh muc de xuat cua ca hai nguon
            if (string.IsNullOrWhiteSpace(query))
            {
                var combined = new List<TrackDto>();
                combined.AddRange(Vietnamese.GetAllTracks());
                combined.AddRange(Jamendo.GetCuratedTracksMatching(string.Empty));
                return combined.Take(safeLimit).ToList();
            }

            string q = query.Trim().ToLowerInvariant();
            string qNorm = VietnameseMusicSourceProvider.RemoveDiacritics(q);

            // Nhan dien y dinh tim kiem nhac Viet Nam
            bool isVietnameseIntent =
                qNorm.Contains("viet") ||
                qNorm.Contains("vpop") ||
                qNorm.Contains("v-pop") ||
                qNorm.Contains("trinh") ||
                qNorm.Contains("son tung") ||
                qNorm.Contains("den vau") ||
                qNorm.Contains("acoustic") ||
                qNorm.Contains("guitar") ||
                qNorm.Contains("que huong") ||
                qNorm.Contains("diem xua") ||
                qNorm.Contains("ha trang");

            if (isVietnameseIntent)
            {
                // Uu tien tim kiem nhac Viet Nam truoc
                var vnTracks = await Vietnamese.SearchTracksAsync(query, safeLimit, cancellationToken).ConfigureAwait(false);
                if (vnTracks.Count >= safeLimit)
                {
                    return vnTracks;
                }

                // Neu chua du gioi han, bo sung them ket qua quoc te tu Jamendo
                int remaining = safeLimit - vnTracks.Count;
                var internationalTracks = await Jamendo.SearchTracksAsync(query, remaining, cancellationToken).ConfigureAwait(false);

                var merged = new List<TrackDto>(vnTracks);
                merged.AddRange(internationalTracks);
                return merged;
            }
            else
            {
                // Chay tim kiem bat dong bo song song tren ca hai nha cung cap de toi uu thoi gian phan hoi
                var vnTask = Vietnamese.SearchTracksAsync(query, safeLimit, cancellationToken);
                var jamendoTask = Jamendo.SearchTracksAsync(query, safeLimit, cancellationToken);

                await Task.WhenAll(vnTask, jamendoTask).ConfigureAwait(false);

                var vnResults = vnTask.Result ?? new List<TrackDto>();
                var jamendoResults = jamendoTask.Result ?? new List<TrackDto>();

                var merged = new List<TrackDto>();
                merged.AddRange(vnResults);
                merged.AddRange(jamendoResults);

                // Loai bo cac ban ghi trung Id neu co va gioi han theo safeLimit
                return merged.GroupBy(t => t.Id).Select(g => g.First()).Take(safeLimit).ToList();
            }
        }

        /// <summary>
        /// Dinh tuyen va phan giai ma bai hat de tra ve URL am thanh phat truc tiep.
        /// </summary>
        /// <param name="trackId">Dinh danh duy nhat cua bai hat.</param>
        /// <returns>Dia chi URL goc tren CDN cua bai hat do.</returns>
        public string ResolveAudioUrl(string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId))
            {
                return string.Empty;
            }

            // Phan biet nguon nhac dua tren tien to cua Track ID
            if (trackId.StartsWith("vn_track_"))
            {
                return Vietnamese.ResolveTrackAudioUrl(trackId);
            }

            return Jamendo.ResolveTrackAudioUrl(trackId);
        }
    }
}
