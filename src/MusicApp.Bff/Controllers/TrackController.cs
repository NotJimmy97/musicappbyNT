using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MusicApp.Bff.Providers;
using MusicApp.Bff.Services;
using MusicApp.Core.Dtos;

namespace MusicApp.Bff.Controllers
{
    /// <summary>
    /// Dieu khien API xu ly truy van va tim kiem thong tin bai hat (Track Search Web API Controller).
    /// 
    /// Tac dung:
    /// - Tiep nhan yeu cau tim kiem HTTP GET tai endpoint: /api/v1/search?query={query}&amp;limit={limit}.
    /// - Kiem tra tinh hop le cua tham so dau vao, gioi han so luong ban ghi an toan (Clamping [1..50]).
    /// - Tra ve doi tuong SearchResponseDto chua danh sach TrackDto da duoc tong hop tu nhieu nguon.
    /// 
    /// Van de giai quyet:
    /// - Bao ve backend khoi cac cuoc tan cong DoS hoac query qua tai bang cach ap dung Clamped Limit va MemoryCache.
    /// - Ket hop MemoryCacheService voi thoi gian luu dem 30 phut giup phan hoi ngay lap tuc cho cac tu khoa trung lap.
    /// - Che dau su phuc tap cua viec dinh tuyen da nguon (Vietnamese vs Jamendo) thong qua lop MusicSourceRouter.
    /// 
    /// Cach thuc van hanh:
    /// - Tao cacheKey theo dinh dang "search:{query}:{limit}".
    /// - Goi Cache.GetOrCreateAsync: neu cache hit, tra ve ket qua ngay lap tuc; neu cache miss, goi Router.SearchAsync.
    /// </summary>
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
