using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao dien dich vu quan ly va phan tich loi bai hat dong bo (Lyrics Service Interface).
    /// 
    /// Tac dung:
    /// - Cung cap phuong thuc phan tich chuoi dinh dang LRC thanh danh sach LyricLine.
    /// - Cung cap co che tu dong tim nap loi bai hat tu file .lrc di kem tren o dia hoac tu danh muc tich hop san.
    /// 
    /// Van de giai quyet:
    /// - Chuan hoa quy trinh xu ly loi bai hat cho ca nguon nhac offline (file dia phuong) va nguon nhac online (catalog san co).
    /// - Cho phep ung dung hien thi loi dong bo thoi gian thuc dang Karaoke cho nguoi dung.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc thuc thi boi LyricsService, ben trong su dung bo phan tich Regex LrcParser.
    /// - Duoc goi boi LyricsViewModel khi bai hat hien tai thay doi tren bo phat nhac.
    /// </summary>
    public interface ILyricsService
    {
        /// <summary>
        /// Phan tich noi dung chuoi van ban dinh dang LRC thanh danh sach cac dong loi LyricLine sap xep theo thoi gian.
        /// </summary>
        /// <param name="lrcContent">Chuoi van ban chua noi dung file .lrc.</param>
        /// <returns>Danh sach cac doi tuong LyricLine da duoc sap xep tang dan theo timestamp.</returns>
        IReadOnlyList<LyricLine> ParseLrc(string lrcContent);

        /// <summary>
        /// Tim kiem va tai loi bai hat phu hop cho mot bai hat duoc chi dinh.
        /// </summary>
        /// <param name="track">Doi tuong ban nhac can nap loi.</param>
        /// <param name="cancellationToken">Token huy tac vu doc bat dong bo.</param>
        /// <returns>Danh sach cac dong loi da parse, hoac danh sach rong neu khong tim thay loi phu hop.</returns>
        Task<IReadOnlyList<LyricLine>> LoadLyricsForTrackAsync(TrackModel track, CancellationToken cancellationToken = default(CancellationToken));
    }
}
