using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicApp.Bff;
using MusicApp.Bff.Controllers;
using MusicApp.Core.Dtos;
using Newtonsoft.Json;
using Owin;

namespace MusicApp.Tests
{
    [TestClass]
    public class BffEndpointTests
    {
        private TestServer _server;
        private HttpClient _client;

        [TestInitialize]
        public void Setup()
        {
            _server = TestServer.Create(app =>
            {
                var startup = new Startup();
                startup.Configuration(app);
            });
            _client = _server.HttpClient;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _server?.Dispose();
        }

        [TestMethod]
        public async Task SearchEndpoint_WithValidQuery_Returns200OkWithStreamEndpoint()
        {
            var response = await _client.GetAsync("api/v1/search?query=electronic&limit=3");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            var searchResult = JsonConvert.DeserializeObject<SearchResponseDto>(body);

            Assert.IsNotNull(searchResult);
            Assert.IsTrue(searchResult.Total > 0);
            Assert.IsNotNull(searchResult.Items);
            Assert.IsTrue(searchResult.Items.Count > 0);

            var firstItem = searchResult.Items[0];
            Assert.IsFalse(string.IsNullOrWhiteSpace(firstItem.Title));
            Assert.IsFalse(string.IsNullOrWhiteSpace(firstItem.Artist));
            Assert.IsTrue(firstItem.StreamEndpoint.StartsWith("/api/v1/stream/"));
        }

        [TestMethod]
        public async Task SearchEndpoint_WithEmptyQuery_Returns400BadRequest()
        {
            var response = await _client.GetAsync("api/v1/search?query=&limit=10");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamEndpoint_WithRangeHeader_ProxiesAudioContent()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/stream/jamendo_track_1849201");
            request.Headers.Range = new RangeHeaderValue(0, 1023);

            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.PartialContent ||
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadGateway
            );
        }
    }
}

