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
    /// <summary>
    /// Lớp HTTP Client thực hiện gọi API tới BFF (Backend-for-Frontend).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Thực hiện request HTTP GET để lấy dữ liệu tìm kiếm, deserialize JSON.
    /// 2. Không chịu trách nhiệm: Quản lý logic UI hoặc xử lý tín hiệu.
    /// 3. Vòng đời: Sử dụng HttpClient tĩnh (Singleton) để tránh lỗi cạn kiệt Socket (Socket Exhaustion).
    /// 4. Đa luồng: Hoạt động bất đồng bộ (async), hỗ trợ CancellationToken cho phép hủy giữa chừng.
    /// </remarks>
    public class MusicApiClient : IMusicApiClient
    {
        private static readonly HttpClient HttpClientInstance;
        private readonly string _baseUrl;

        /// <summary>
        /// Thiet lap cau hinh mang toan cuc cho HttpClient singleton.
        /// </summary>
        static MusicApiClient()
        {
            // Bat buoc su dung TLS 1.2 tren .NET Framework 4.6.1 truoc khi thuc hien ket noi socket
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

        /// <summary>
        /// Khoi tao client voi dia chi goc cua may chu BFF.
        /// </summary>
        /// <param name="baseUrl">Dia chi goc cua BFF API (mac dinh: http://localhost:5245/api/v1).</param>
        public MusicApiClient(string baseUrl = "http://localhost:5245/api/v1")
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Gui yeu cau tim kiem den BFF va chuyen doi JSON phan hoi thanh SearchResponseDto.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem.</param>
        /// <param name="limit">So luong bai hat toi da can lay.</param>
        /// <param name="cancellationToken">Token huy yeu cau khi nguoi dung go tiep.</param>
        /// <returns>Doi tuong SearchResponseDto chua danh sach ket qua.</returns>
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

        /// <summary>
        /// Gui yeu cau tim kiem den BFF va tra ve chuoi van ban JSON goc chua parse.
        /// </summary>
        /// <param name="query">Tu khoa tim kiem.</param>
        /// <param name="limit">So luong bai hat toi da.</param>
        /// <param name="cancellationToken">Token huy yeu cau.</param>
        /// <returns>Chuoi JSON do may chu tra ve.</returns>
        public async Task<string> SearchTracksRawAsync(string query, int limit, CancellationToken cancellationToken)
        {
            string url = $"{_baseUrl}/search?query={Uri.EscapeDataString(query ?? string.Empty)}&limit={limit}";

            using (var response = await HttpClientInstance.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Giai phong tai nguyen client (HttpClient dung chung duoc giu ton tai theo tien trinh).
        /// </summary>
        public void Dispose()
        {
            // HttpClient singleton duoc duy tri trong suot vong doi ung dung de toi uu pooling
        }
    }
}
