using System;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;

namespace MusicApp.Core.Interfaces
{
    /// <summary>
    /// Giao dien Client HTTP ket noi giua ung dung WPF va may chu Backend For Frontend (BFF).
    /// 
    /// Tac dung:
    /// - Cung cap cac phuong thuc goi API tim kiem bai hat bat dong bo tu xa.
    /// - Ho tro ca phuong thuc tra ve DTO da deserialize va phuong thuc tra ve chuoi JSON goc (Raw JSON).
    /// 
    /// Van de giai quyet:
    /// - Dong goi logic su dung HttpClient, quan ly vong doi ket noi TCP (Connection Pooling) va tranh hien tuong Socket Exhaustion.
    /// - Cho phep huy yeu cau mang thoi gian thuc bang CancellationToken khi nguoi dung go tiep tu khoa tim kiem (Debounce).
    /// 
    /// Cach thuc van hanh:
    /// - Duoc thuc thi boi lop MusicApiClient trong du an MusicApp (Presentation Layer).
    /// - Giao tiep voi may chu ASP.NET OWIN Host chay ngam tren cong cuc bo (vi du: http://127.0.0.1:5005).
    /// </summary>
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
