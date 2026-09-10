using System;
using System.Threading.Tasks;
using System.Web.Http;
using SpotifyWpf.Bff.Providers;
using SpotifyWpf.Bff.Services;
using SpotifyWpf.Core.Dtos;

namespace SpotifyWpf.Bff.Controllers
{
    public class TrackController : ApiController
    {
        private static readonly JamendoSourceProvider Provider = new JamendoSourceProvider();
        private static readonly MemoryCacheService CacheService = new MemoryCacheService();

        [HttpGet]
        [Route("api/v1/search")]
        public async Task<IHttpActionResult> Search([FromUri] string query, [FromUri] int limit = 20, [FromUri] int page = 1)
        {
            // Early return guard clause rejecting invalid search queries at the controller boundary
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required");
            }

            // Enforce defensive limits to prevent excessive memory allocation
            if (limit <= 0) limit = 20;
            if (limit > 50) limit = 50;
            if (page <= 0) page = 1;

            string cacheKey = $"search_{query.Trim().ToLowerInvariant()}_{limit}_{page}";

            SearchResponseDto response = await CacheService.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(30),
                () => Provider.SearchAsync(query, limit, page)
            ).ConfigureAwait(false);

            return Ok(response);
        }
    }
}
