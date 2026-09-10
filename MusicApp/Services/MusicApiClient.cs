using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;
using Newtonsoft.Json;

namespace MusicApp.Services
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
                Timeout = TimeSpan.FromSeconds(4)
            };

            HttpClientInstance.DefaultRequestHeaders.Accept.Clear();
            HttpClientInstance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClientInstance.DefaultRequestHeaders.Add("User-Agent", "MusicAppClient/1.0 (.NET Framework 4.6.1)");
        }

        public MusicApiClient(string baseUrl = "http://localhost:5245/api/v1")
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<SearchResponseDto> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken)
        {
            string url = $"{_baseUrl}/search?query={Uri.EscapeDataString(query ?? string.Empty)}&limit={limit}";

            using (var response = await HttpClientInstance.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<SearchResponseDto>(json);
            }
        }

        public async Task<string> SearchTracksRawAsync(string query, int limit, CancellationToken cancellationToken)
        {
            string url = $"{_baseUrl}/search?query={Uri.EscapeDataString(query ?? string.Empty)}&limit={limit}";

            using (var response = await HttpClientInstance.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            // Reusable singleton HttpClient is intentionally kept alive for process lifetime
        }
    }
}

