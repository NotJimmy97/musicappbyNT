using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ quản lý và phân tích lời bài hát đồng bộ.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Phân tích file LRC và tải lời bài hát.
    /// KHÔNG chịu trách nhiệm: Quản lý việc hiển thị UI hay trạng thái phát nhạc.
    /// Vòng đời: Scoped hoặc Singleton.
    /// Luồng: Thao tác file/I-O bất đồng bộ chạy trên background thread, có hỗ trợ CancellationToken.
    /// </remarks>
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
