using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MusicApp.Bff.Providers;
using MusicApp.Bff.Services;
using MusicApp.Core.Dtos;

// OWNS: API xử lý tìm kiếm bài hát (HTTP GET /api/v1/search).
// DOES NOT OWN: Nguồn cung cấp dữ liệu thực tế (dùng MusicSourceRouter) hoặc cơ chế Cache.
// CONSTRAINTS: Phải clamp limit [1..50] để chống overload. Cache sử dụng chỉ lưu RAM.

namespace MusicApp.Bff.Controllers
{
    /// <summary>
    /// Điều khiển API xử lý truy vấn và tìm kiếm thông tin bài hát (Track Search).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Validate query/limit, kiểm tra cache và trả về dữ liệu bài hát dạng JSON.
    /// 2. Không chịu trách nhiệm: Gọi trực tiếp sang các API ngoài như Jamendo.
    /// 3. Vòng đời trạng thái: Stateless Controller (mỗi request 1 instance).
    /// 4. Yêu cầu đặc biệt: Static Router và Cache tồn tại ở process-level. Cache chỉ lưu trên RAM (mất khi restart).
    /// </remarks>
    public class TrackController : ApiController
    {
        private static readonly MusicSourceRouter Router = new MusicSourceRouter();
        private static readonly MemoryCacheService Cache = new MemoryCacheService();

        /// <summary>
        /// Endpoint tim kiem bai hat da nguon dua tren tu khoa.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem do nguoi dung nhap vao tren giao dien.</param>
        /// <param name="limit">So luong bai hat toi da can lay (mac dinh: 20, toi da: 50).</param>
        /// <returns>SearchResponseDto chua ket qua tim kiem va tong so ban ghi.</returns>
        [HttpGet]
        [Route("api/v1/search")]
        public async Task<IHttpActionResult> Search([FromUri] string query, [FromUri] int limit = 20)
        {
            // Kiem tra tham so bat buoc (Guard Clause)
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Tu khoa tim kiem (query) khong duoc de trong.");
            }

            // Gioi han an toan so luong ket qua tra ve trong khoang tu 1 den 50
            int clampedLimit = Math.Min(Math.Max(limit, 1), 50);
            string cacheKey = $"search:{query.ToLowerInvariant().Trim()}:{clampedLimit}";

            // Lay tu bo nho dem hoac thuc thi tim kiem qua Router neu chua co
            var response = await Cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(30),
                async () =>
                {
                    var items = await Router.SearchAsync(query, clampedLimit, CancellationToken.None).ConfigureAwait(false);
                    return new SearchResponseDto
                    {
                        Total = items.Count,
                        Items = items
                    };
                }
            ).ConfigureAwait(false);

            return Ok(response);
        }
    }
}
