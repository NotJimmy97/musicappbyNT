using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SpotifyWpf.Bff;
using SpotifyWpf.Core.Dtos;

namespace SpotifyWpf.Tests
{
    [TestClass]
    public class BffEndpointTests
    {
        private const string BaseAddress = "http://localhost:5245";
        private static IDisposable _serverHost;
        private static HttpClient _client;

        [ClassInitialize]
        public static void SetupClass(TestContext context)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            _serverHost = BffServerHost.Start(BaseAddress);
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress),
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        [ClassCleanup]
        public static void CleanupClass()
        {
            _client?.Dispose();
            _serverHost?.Dispose();
        }

        [TestMethod]
        public async Task SearchEndpoint_WithValidQuery_Returns200OkWithStreamEndpoint()
        {
            // Act: Request 3 electronic tracks matching Step 2 verification contract
            HttpResponseMessage response = await _client.GetAsync("/api/v1/search?query=electronic&limit=3");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SearchResponseDto>(json);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Items.Count > 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Items[0].StreamEndpoint));
            Assert.IsTrue(result.Items[0].StreamEndpoint.StartsWith("/api/v1/stream/"));
        }

        [TestMethod]
        public async Task SearchEndpoint_WithEmptyQuery_Returns400BadRequest()
        {
            // Act: Empty query must trigger early return guard clause
            HttpResponseMessage response = await _client.GetAsync("/api/v1/search?query=&limit=3");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamEndpoint_WithRangeHeader_ProxiesAudioContent()
        {
            // Arrange: Request 1KB partial byte range
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/stream/jamendo_track_1849201");
            request.Headers.Range = new RangeHeaderValue(0, 1024);

            // Act
            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            // Assert: Must return audio content (PartialContent 206 or OK 200)
            bool isExpectedStatus = response.StatusCode == HttpStatusCode.PartialContent || response.StatusCode == HttpStatusCode.OK;
            Assert.IsTrue(isExpectedStatus, $"Unexpected status code: {response.StatusCode}");
            Assert.AreEqual("audio/mpeg", response.Content.Headers.ContentType.MediaType);
        }
    }
}
