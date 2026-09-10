using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MusicApp.Bff.Providers;
using MusicApp.Bff.Services;
using MusicApp.Core.Dtos;

namespace MusicApp.Bff.Controllers
{
    public class TrackController : ApiController
    {
        private static readonly JamendoSourceProvider Provider = new JamendoSourceProvider();
        private static readonly MemoryCacheService Cache = new MemoryCacheService();

        [HttpGet]
        [Route("api/v1/search")]
        public async Task<IHttpActionResult> Search([FromUri] string query, [FromUri] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }

            int clampedLimit = Math.Min(Math.Max(limit, 1), 50);
            string cacheKey = $"search:{query.ToLowerInvariant().Trim()}:{clampedLimit}";

            var response = await Cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(30),
                async () =>
                {
                    var items = await Provider.SearchTracksAsync(query, clampedLimit, CancellationToken.None).ConfigureAwait(false);
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
