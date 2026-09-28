using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using MusicApp.Bff.Providers;
using MusicApp.Core.Persistence;
using MusicApp.Core.Persistence.Repositories;
using MusicApp.Core.Services;

namespace MusicApp.Bff.Controllers
{
    /// <summary>
    /// Dieu khien API truyen phat am thanh truc tiep (Audio Streaming Reverse Proxy Controller).
    /// 
    /// Tac dung:
    /// - Tiep nhan yeu cau stream HTTP GET tai endpoint: /api/v1/stream/{id}.
    /// - Dong vai tro la may chu proxy chuyen tiep am thanh (Reverse Proxy) giua Audio Engine cua Client va may chu CDN goc.
    /// - Tich hop bo nho dem Spotify CAS (Content-Addressable Storage) tren o dia cuc bo (%LOCALAPPDATA%\MusicApp\Cache):
    ///   + Neu tep am thanh da co tren dia cuc bo: Phuc vu truc tiep tu o dia (X-Cache: HIT) voi do tre < 50ms, tiet kiem 100% bang thong.
    ///   + Neu tep chua co: Chuyen tiep stream tu CDN goc ve client (X-Cache: MISS), dong thoi kich hoat tai ngam de luu cache cho lan phat sau.
    /// - Ho tro day du tieu chuan HTTP 206 Partial Content thong qua header pham vi byte (Range Requests) giup tua nhac tuc thi (Scrubbing/Seeking).
    /// - Tiet kiem bo nho RAM: Su dung Streaming Pipeline va BoundedStream, tuyet doi khong nap toan bo tep am thanh vao RAM.
    /// </summary>
    public class StreamController : ApiController
    {
        private static readonly MusicSourceRouter Router = new MusicSourceRouter();
        private static readonly HttpClient ProxyClient;
        private static readonly LocalAudioCacheService CacheService;
        private static readonly ConcurrentDictionary<string, bool> ActiveDownloads = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Khoi tao cau hinh HttpClient chia se dung chung va he thong LocalAudioCacheService.
        /// </summary>
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

            try
            {
                DatabaseInitializer.Initialize();
                var cacheRepo = new StreamCacheRepository();
                CacheService = new LocalAudioCacheService(cacheRepo);
            }
            catch
            {
                // Fallback neu khoi tao database that bai trong moi truong mock/isolated test
                CacheService = null;
            }
        }

        /// <summary>
        /// Endpoint chuyen tiep luong am thanh nhi phan ho tro Range Request va Spotify CAS Cache.
        /// </summary>
        /// <param name="id">Dinh danh duy nhat cua ban nhac can phat.</param>
        /// <returns>HttpResponseMessage chua stream am thanh va cac header phan hoi HTTP 206/200 phu hop.</returns>
        [HttpGet]
        [Route("api/v1/stream/{id}")]
        public async Task<HttpResponseMessage> Stream(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Dinh danh bai hat (Track ID) khong duoc de trong.");
            }

            // 1. Kiem tra xem ban nhac da co trong Local Disk Cache chua
            string trackHash = CacheService?.ComputeTrackHash(id);
            if (!string.IsNullOrEmpty(trackHash) && CacheService != null)
            {
                try
                {
                    string cachedFilePath = await CacheService.TryGetCachedAudioFileAsync(trackHash).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(cachedFilePath) && File.Exists(cachedFilePath))
                    {
                        return ServeFromLocalCache(cachedFilePath);
                    }
                }
                catch
                {
                    // Fallback to upstream proxy neu truy xuat cache gap su co
                }
            }

            // 2. Neu chua co trong cache, phan giai URL goc va proxy stream truc tiep tu CDN
            string targetUrl = Router.ResolveAudioUrl(id);
            var upstreamRequest = new HttpRequestMessage(HttpMethod.Get, targetUrl);

            // Chuyen tiep header Range de ho tro tua vi tri trong Audio Engine
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

                // Sao chep cac header can thiet cho bo giai ma am thanh phia client
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
                response.Headers.Add("X-Cache", "MISS");

                // Kich hoat tai ngam luu cache tren o dia neu request thanh cong
                if (upstreamResponse.IsSuccessStatusCode && !string.IsNullOrEmpty(trackHash))
                {
                    TriggerBackgroundCaching(trackHash, targetUrl);
                }

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadGateway, "Loi chuyen tiep proxy stream am thanh: " + ex.Message);
            }
        }

        /// <summary>
        /// Phuc vu stream am thanh truc tiep tu tep cache cuc bo voi do tre cuc thap (< 50ms).
        /// </summary>
        private HttpResponseMessage ServeFromLocalCache(string cachedFilePath)
        {
            var fileInfo = new FileInfo(cachedFilePath);
            long totalLength = fileInfo.Length;

            if (Request.Headers.Range != null && Request.Headers.Range.Ranges.Count > 0)
            {
                var range = Request.Headers.Range.Ranges.FirstOrDefault();
                long from = range?.From ?? 0;
                long to = range?.To ?? (totalLength - 1);

                if (from >= totalLength)
                {
                    var invalidResponse = Request.CreateResponse((HttpStatusCode)416); // RequestedRangeNotSatisfiable
                    invalidResponse.Content = new StringContent("Pham vi byte yeu cau vuot qua tong kich thuoc tep.");
                    invalidResponse.Content.Headers.ContentRange = new ContentRangeHeaderValue(totalLength);
                    return invalidResponse;
                }

                if (to >= totalLength)
                {
                    to = totalLength - 1;
                }

                long length = to - from + 1;
                var fs = new FileStream(cachedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
                fs.Seek(from, SeekOrigin.Begin);
                var boundedStream = new BoundedStream(fs, length);

                var partialResponse = Request.CreateResponse(HttpStatusCode.PartialContent);
                partialResponse.Content = new StreamContent(boundedStream);
                partialResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
                partialResponse.Content.Headers.ContentRange = new ContentRangeHeaderValue(from, to, totalLength);
                partialResponse.Content.Headers.ContentLength = length;
                partialResponse.Headers.AcceptRanges.Add("bytes");
                partialResponse.Headers.Add("X-Cache", "HIT");
                return partialResponse;
            }
            else
            {
                var fs = new FileStream(cachedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
                var fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                fullResponse.Content = new StreamContent(fs);
                fullResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
                fullResponse.Content.Headers.ContentLength = totalLength;
                fullResponse.Headers.AcceptRanges.Add("bytes");
                fullResponse.Headers.Add("X-Cache", "HIT");
                return fullResponse;
            }
        }

        /// <summary>
        /// Kich hoat tac vu ngam tai toan bo ban nhac ve luu vao Content-Addressable Storage (CAS) Cache.
        /// </summary>
        private static void TriggerBackgroundCaching(string trackHash, string audioUrl)
        {
            if (CacheService == null || string.IsNullOrWhiteSpace(trackHash) || string.IsNullOrWhiteSpace(audioUrl)) return;
            if (!ActiveDownloads.TryAdd(trackHash, true)) return;

            Task.Run(async () =>
            {
                try
                {
                    using (var downloadClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) })
                    using (var response = await downloadClient.GetAsync(audioUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                await CacheService.SaveStreamToCacheAsync(trackHash, stream).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch
                {
                    // Bo qua ngoai le ngam de khong anh huong den ung dung
                }
                finally
                {
                    ActiveDownloads.TryRemove(trackHash, out _);
                }
            });
        }

        /// <summary>
        /// Stream gioi han pham vi byte phuc vu chuan xac HTTP 206 Partial Content Range.
        /// </summary>
        private class BoundedStream : Stream
        {
            private readonly Stream _inner;
            private long _bytesRemaining;

            public BoundedStream(Stream inner, long length)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _bytesRemaining = Math.Max(0, length);
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _bytesRemaining;
            public override long Position
            {
                get => 0;
                set => throw new NotSupportedException();
            }

            public override void Flush() => _inner.Flush();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_bytesRemaining <= 0) return 0;
                int toRead = (int)Math.Min(count, _bytesRemaining);
                int read = _inner.Read(buffer, offset, toRead);
                _bytesRemaining -= read;
                return read;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            {
                if (_bytesRemaining <= 0) return 0;
                int toRead = (int)Math.Min(count, _bytesRemaining);
                int read = await _inner.ReadAsync(buffer, offset, toRead, cancellationToken).ConfigureAwait(false);
                _bytesRemaining -= read;
                return read;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
