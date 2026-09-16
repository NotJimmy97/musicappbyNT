using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao dien nha cung cap nguon am nhac (Music Source Provider Strategy).
    /// 
    /// Tac dung:
    /// - Dinh nghia mau thiet ke Chien luoc (Strategy Pattern) cho cac nguon du lieu am nhac khac nhau trong he thong.
    /// - Cung cap phuong thuc tim kiem danh sach bai hat va phuong thuc lay Stream am thanh nhi phan theo dai byte (Range Request).
    /// 
    /// Van de giai quyet:
    /// - Cho phep mo rong he thong voi bat ky nguon am nhac nao moi (Jamendo, Nhac Viet Nam, SoundCloud, Spotify, Zing MP3,...)
    ///   ma khong can sua doi ma nguon cua Controller hay Client (tuan thu Open/Closed Principle).
    /// - Ho tro HTTP 206 Partial Content: giup stream am thanh tiet kiem bang thong va cho phep client tua vi tri tuy y.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc trien khai boi JamendoMusicSourceProvider va VietnameseMusicSourceProvider.
    /// - Duoc dieu phoi tap trung boi MusicSourceRouter trong du an MusicApp.Bff.
    /// </summary>
    public interface IMusicSourceProvider
    {
        /// <summary>
        /// Ten nhan dien cua nha cung cap am nhac (vi du: "Jamendo", "VietnameseCatalog").
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Tim kiem danh sach bai hat tu nguon tuong ung theo tu khoa.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem.</param>
        /// <param name="limit">So luong bai hat toi da can lay.</param>
        /// <param name="cancellationToken">Token huy tac vu mang.</param>
        /// <returns>Danh sach cac doi tuong TrackDto da duoc chuan hoa.</returns>
        Task<List<TrackDto>> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken);

        /// <summary>
        /// Lay luong du lieu am thanh nhi phan ho tro pham vi byte (Byte-range streaming).
        /// </summary>
        /// <param name="trackId">Dinh danh bai hat can lay stream.</param>
        /// <param name="startByte">Vi tri byte bat dau (neu null se bat dau tu dau file).</param>
        /// <param name="endByte">Vi tri byte ket thuc (neu null se lay den het file).</param>
        /// <param name="cancellationToken">Token huy ket noi mang.</param>
        /// <returns>Doi tuong Stream chua du lieu am thanh nhi phan de tra ve cho client.</returns>
        Task<Stream> GetAudioStreamAsync(string trackId, long? startByte, long? endByte, CancellationToken cancellationToken);
    }
}
