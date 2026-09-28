using System;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao diện Client HTTP kết nối với BFF.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Giao tiếp qua HTTP lấy dữ liệu bài hát từ xa.
    /// KHÔNG chịu trách nhiệm: Quản lý trạng thái cache hay logic phát nhạc.
    /// Vòng đời: Cần Dispose (thường quản lý bởi IHttpClientFactory hoặc DI container).
    /// Luồng: Thực hiện I/O mạng qua Task bất đồng bộ (Network thread).
    /// </remarks>
    public interface IMusicApiClient : IDisposable
    {
        /// <summary>
        /// Gui yeu cau tim kiem danh sach bai hat den may chu BFF va tu dong parse thanh SearchResponseDto.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem (ten bai hat, ten ca si hoac album).</param>
        /// <param name="limit">So luong ban ghi toi da can lay ve tren mot trang.</param>
        /// <param name="cancellationToken">Token cho phep huy bo yeu cau mang khi can thiet.</param>
        /// <returns>Doi tuong SearchResponseDto chua tap hop TrackDto tim thay.</returns>
        Task<SearchResponseDto> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken);

        /// <summary>
        /// Gui yeu cau tim kiem va tra ve chuoi van ban JSON nguyen ban chua qua deserialize.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem.</param>
        /// <param name="limit">So luong ban ghi toi da.</param>
        /// <param name="cancellationToken">Token cho phep huy bo yeu cau mang.</param>
        /// <returns>Chuoi JSON goc do may chu tra ve.</returns>
        Task<string> SearchTracksRawAsync(string query, int limit, CancellationToken cancellationToken);
    }
}
