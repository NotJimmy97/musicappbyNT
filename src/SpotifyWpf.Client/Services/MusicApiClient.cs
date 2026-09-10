using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyWpf.Core.Dtos;
using SpotifyWpf.Core.Interfaces;

namespace SpotifyWpf.Client.Services
{
    public class MusicApiClient : IMusicApiClient
    {
        private static readonly HttpClient HttpClientInstance;
        private readonly string _baseUrl;

        static MusicApiClient()
        {
            // Enforce process-wide TLS 1.2 on .NET Framework 4.6.1 before any outbound socket request
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            ServicePointManager.DefaultConnectionLimit = 64;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            HttpClientInstance = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            HttpClientInstance.DefaultRequestHeaders.Accept.Clear();
            HttpClientInstance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClientInstance.DefaultRequestHeaders.Add("User-Agent", "SpotifyWpfClient/1.0 (.NET Framework 4.6.1)");
        }

        public MusicApiClient(string baseUrl = "http://localhost:5245/api/v1")
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<SearchResponseDto> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken)
        {
            string rawJson = await SearchTracksRawAsync(query, limit, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return new SearchResponseDto();
            }

            return JsonConvert.DeserializeObject<SearchResponseDto>(rawJson);
        }

        public async Task<string> SearchTracksRawAsync(string query, int limit, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            string requestUri = $"{_baseUrl}/search?query={Uri.EscapeDataString(query)}&limit={limit}";

            try
            {
                using (HttpResponseMessage response = await HttpClientInstance.GetAsync(
                    requestUri,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                ).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            // Retain static HttpClientInstance to avoid socket exhaustion
        }
    }
}
