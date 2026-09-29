using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;

namespace MusicApp.Bff.Providers
{
    // OWNS: Phân giải intent tìm kiếm và điều phối kết quả giữa các nhà cung cấp.
    // DOES NOT OWN: Tương tác mạng thực tế HTTP hoặc caching metadata.
    // CONSTRAINTS: Phải kết hợp song song (Task.WhenAll) và tự động loại bỏ bản ghi trùng (Deduplication).

    /// <summary>
    /// Bộ định tuyến và điều phối các nhà cung cấp nguồn âm nhạc.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Phân tích keyword để chọn provider ưu tiên, gửi request song song và gộp kết quả.
    /// 2. Không chịu trách nhiệm: Quản lý cache RAM/Disk hoặc tải file trực tiếp.
    /// 3. Vòng đời trạng thái: Stateless coordinator (thường dùng static cho vòng đời process-level).
    /// 4. Yêu cầu đặc biệt: Logic fallback thông minh (Curated -> Jamendo) và chuẩn hoá fallback chain.
    /// </remarks>
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
        /// Khoi tao bo dinh tuyen nguon nhac va cac provider thanh phan (ho tro dependency injection).
        /// </summary>
        public MusicSourceRouter(JamendoSourceProvider jamendo = null, VietnameseMusicSourceProvider vietnamese = null)
        {
            Jamendo = jamendo ?? new JamendoSourceProvider();
            Vietnamese = vietnamese ?? new VietnameseMusicSourceProvider();
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
