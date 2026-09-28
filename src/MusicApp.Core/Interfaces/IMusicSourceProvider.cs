using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao diện nhà cung cấp nguồn âm nhạc (Strategy Pattern).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Cung cấp API tìm kiếm bài hát và lấy Stream âm thanh nhị phân.
    /// KHÔNG chịu trách nhiệm: Quản lý thư viện cục bộ hay trạng thái phát.
    /// Vòng đời: Singleton, đăng ký qua MusicSourceRouter.
    /// Luồng: Stream/Range request bất đồng bộ trên mạng.
    /// </remarks>
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
