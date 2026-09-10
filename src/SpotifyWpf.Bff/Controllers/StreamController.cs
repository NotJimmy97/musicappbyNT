using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using SpotifyWpf.Bff.Providers;

namespace SpotifyWpf.Bff.Controllers
{
    public class StreamController : ApiController
    {
        private static readonly JamendoSourceProvider Provider = new JamendoSourceProvider();
        private static readonly HttpClient ProxyClient;

        static StreamController()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.None
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
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Track ID is required");
            }

            string targetStreamUrl = await Provider.GetAudioStreamUrlAsync(id).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(targetStreamUrl))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Audio stream could not be resolved");
            }

            var forwardRequest = new HttpRequestMessage(HttpMethod.Get, targetStreamUrl);

            // Forward HTTP Range header directly to enable immediate seeking and partial chunk downloads
            if (Request.Headers.Range != null)
            {
                forwardRequest.Headers.Range = Request.Headers.Range;
            }

            HttpResponseMessage upstreamResponse = await ProxyClient.SendAsync(
                forwardRequest,
                HttpCompletionOption.ResponseHeadersRead
            ).ConfigureAwait(false);

            var clientResponse = Request.CreateResponse(upstreamResponse.StatusCode);

            // Forward content stream without buffering entire file in memory
            var upstreamStream = await upstreamResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            clientResponse.Content = new StreamContent(upstreamStream);

            if (upstreamResponse.Content.Headers.ContentType != null)
            {
                clientResponse.Content.Headers.ContentType = upstreamResponse.Content.Headers.ContentType;
            }
            else
            {
                clientResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
            }

            if (upstreamResponse.Content.Headers.ContentRange != null)
            {
                clientResponse.Content.Headers.ContentRange = upstreamResponse.Content.Headers.ContentRange;
            }

            if (upstreamResponse.Content.Headers.ContentLength.HasValue)
            {
                clientResponse.Content.Headers.ContentLength = upstreamResponse.Content.Headers.ContentLength.Value;
            }

            clientResponse.Headers.AcceptRanges.Add("bytes");

            return clientResponse;
        }
    }
}
