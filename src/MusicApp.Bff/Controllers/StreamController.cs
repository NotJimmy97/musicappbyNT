using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using MusicApp.Bff.Providers;

namespace MusicApp.Bff.Controllers
{
    /// <summary>
    /// Dieu khien API truyen phat am thanh truc tiep (Audio Streaming Reverse Proxy Controller).
    /// 
    /// Tac dung:
    /// - Tiep nhan yeu cau stream HTTP GET tai endpoint: /api/v1/stream/{id}.
    /// - Dong vai tro la may chu proxy chuyen tiep am thanh (Reverse Proxy) giua Audio Engine cua Client va may chu CDN goc.
    /// - Ho tro day du tieu chuan HTTP 206 Partial Content thong qua chuyen tiep header pham vi byte (Range Requests).
    /// 
    /// Van de giai quyet:
    /// - An toan & On dinh: Tranh lo dia chi goc (Direct CDN URL) cua cac nha cung cap am nhac, phong ngua viec bi loi chan
    ///   hoac het han lien ket, dong thoi giai quyet van de chan CORS tren trinh duyet hoac ung dung desktop.
    /// - Ho tro tinh nang tua nhac muot ma (Smooth Scrubbing/Seeking): Khi nguoi dung keo thanh thoi gian, MediaFoundationReader
    ///   gui yeu cau voi header "Range: bytes=start-end". StreamController se chuyen tiep header nay toi CDN goc, giup chi tai
    ///   dung phan du lieu can thiet ma khong phai tai lai toan bo tap tin am thanh tu dau.
    /// - Tiet kiem bo nho RAM: Su dung HttpCompletionOption.ResponseHeadersRead va StreamContent de doc va ghi du lieu
    ///   theo tung chunk nho (Streaming Pipeline), tuyet doi khong nap toan bo file MP3 vao RAM gay ton hao tai nguyen.
    /// 
    /// Cach thuc van hanh:
    /// - Phan giai ma ID thanh URL audio goc thong qua MusicSourceRouter.
    /// - Tao yeu cau upstream gui toi CDN goc voi Range Header neu co.
    /// - Sao chep cac header quan trong nhu Content-Type, Content-Range, Content-Length va thiet lap Accept-Ranges: bytes.
    /// </summary>
    public class StreamController : ApiController
    {
        private static readonly MusicSourceRouter Router = new MusicSourceRouter();
        private static readonly HttpClient ProxyClient;

        /// <summary>
        /// Khoi tao cau hinh HttpClient chia se dung chung de tranh hien tuong Socket Exhaustion.
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
        }

        /// <summary>
        /// Endpoint chuyen tiep luong am thanh nhi phan ho tro Range Request.
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

            // Phan giai dia chi stream goc dua tren quy tac Router
            string targetUrl = Router.ResolveAudioUrl(id);

            var upstreamRequest = new HttpRequestMessage(HttpMethod.Get, targetUrl);

            // Chuyen tiep header Range de ho tro tua vi tri trong Audio Engine
            if (Request.Headers.Range != null)
            {
                upstreamRequest.Headers.Range = Request.Headers.Range;
            }

            try
            {
                // Gui yeu cau toi CDN va chi doc phan Header truoc de phan hoi ngay lap tuc
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

                // Xac nhan may chu ho tro pham vi byte cho cac trinh phat am thanh
                response.Headers.AcceptRanges.Add("bytes");
                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadGateway, "Loi chuyen tiep proxy stream am thanh: " + ex.Message);
            }
        }
    }
}
