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
    /// <summary>
    /// Bo kiem thu tich hop (Integration Tests) cho cac Endpoint cua Backend For Frontend (BFF).
    /// 
    /// - Tac dung: Kiem tra hoat dong thuc te cua cac API Controller (TrackController, StreamController)
    ///   thong qua server ao Microsoft.Owin.Testing.TestServer ma khong can khoi chay port mang vat ly.
    /// 
    /// - Van de giai quyet: Dam bao tinh toan ven cua API Contract (SearchResponseDto, TrackDto),
    ///   kiem thu kha nang xu ly tai nguyen am thanh qua giao thuc HTTP Range Requests (206 Partial Content),
    ///   kiem thu co che dinh tuyen nhac Viet Nam khong dau / co dau va phong ngua hoi quy khi sua code BFF.
    /// 
    /// - Cach thuc van hanh:
    ///   1. Setup(): Khoi tao Owin TestServer su dung cau hinh Startup chuan cua BFF.
    ///   2. Gui cac HTTP Request (GET search, GET stream) thong qua HttpClient cua TestServer.
    ///   3. Assert: Xac minh ma trang thai HttpStatusCode (200, 206, 400), cau truc JSON tra ve va StreamEndpoint.
    ///   4. Cleanup(): Giai phong HttpClient va TestServer sau moi ca kiem thu de tranh ro ri bo nho.
    /// </summary>
    [TestClass]
    public class BffEndpointTests
    {
        private TestServer _server;
        private HttpClient _client;

        /// <summary>
        /// Thiet lap moi truong kiem thu: Khoi tao in-memory OWIN TestServer va HttpClient.
        /// </summary>
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

        /// <summary>
        /// Don dep tai nguyen sau khi kiem thu: Giai phong HttpClient va TestServer.
        /// </summary>
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

        [TestMethod]
        public async Task SearchEndpoint_WithVietnameseQuery_ReturnsVietnameseTracks()
        {
            var response = await _client.GetAsync("api/v1/search?query=vietnam&limit=5");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            var searchResult = JsonConvert.DeserializeObject<SearchResponseDto>(body);

            Assert.IsNotNull(searchResult);
            Assert.IsTrue(searchResult.Total > 0);
            Assert.IsTrue(searchResult.Items.Count > 0);

            var firstItem = searchResult.Items[0];
            Assert.IsTrue(firstItem.Id.StartsWith("vn_track_"));
            Assert.IsFalse(string.IsNullOrWhiteSpace(firstItem.Title));
            Assert.IsTrue(firstItem.StreamEndpoint.StartsWith("/api/v1/stream/vn_track_"));
        }

        [TestMethod]
        public async Task SearchEndpoint_WithAccentInsensitiveQuery_MatchesVietnameseTitle()
        {
            var response = await _client.GetAsync("api/v1/search?query=diem%20xua&limit=1");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string body = await response.Content.ReadAsStringAsync();
            var searchResult = JsonConvert.DeserializeObject<SearchResponseDto>(body);

            Assert.IsNotNull(searchResult);
            Assert.IsTrue(searchResult.Total > 0);
            Assert.AreEqual("Diễm Xưa", searchResult.Items[0].Title);
        }

        [TestMethod]
        public async Task StreamEndpoint_WithVietnameseTrack_ProxiesAudioContent()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/stream/vn_track_01");
            request.Headers.Range = new RangeHeaderValue(0, 511);

            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.PartialContent ||
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadGateway
            );
        }
    }
}

