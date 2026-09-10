using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using MusicApp.Bff.Providers;

namespace MusicApp.Bff.Controllers
{
    public class StreamController : ApiController
    {
        private static readonly JamendoSourceProvider Provider = new JamendoSourceProvider();
        private static readonly HttpClient ProxyClient;

        static StreamController()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.None,
                AllowAutoRedirect = true
            };

            ProxyClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        [HttpGet]
        [Route("api/v1/stream/{id}")]
        public async Task<HttpResponseMessage> Stream(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Track ID is required.");
            }

            string targetUrl = Provider.ResolveTrackAudioUrl(id);

            var upstreamRequest = new HttpRequestMessage(HttpMethod.Get, targetUrl);

            // Forward incoming Range Header to enable seamless seek/scrub in the Audio Engine
            if (Request.Headers.Range != null)
            {
                upstreamRequest.Headers.Range = Request.Headers.Range;
            }

            try
            {
                var upstreamResponse = await ProxyClient.SendAsync(
                    upstreamRequest,
                    HttpCompletionOption.ResponseHeadersRead
                ).ConfigureAwait(false);

                var response = Request.CreateResponse(upstreamResponse.StatusCode);
                response.Content = new StreamContent(await upstreamResponse.Content.ReadAsStreamAsync().ConfigureAwait(false));

                // Mirror critical Content headers for media decoders
                if (upstreamResponse.Content.Headers.ContentType != null)
                {
                    response.Content.Headers.ContentType = upstreamResponse.Content.Headers.ContentType;
                }
                else
                {
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
                }

                if (upstreamResponse.Content.Headers.ContentRange != null)
                {
                    response.Content.Headers.ContentRange = upstreamResponse.Content.Headers.ContentRange;
                }

                if (upstreamResponse.Content.Headers.ContentLength.HasValue)
                {
                    response.Content.Headers.ContentLength = upstreamResponse.Content.Headers.ContentLength;
                }

                response.Headers.AcceptRanges.Add("bytes");
                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadGateway, "Audio stream proxy failure: " + ex.Message);
            }
        }
    }
}
