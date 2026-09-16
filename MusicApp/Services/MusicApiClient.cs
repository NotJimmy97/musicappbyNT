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
    /// Lop Client HTTP thuc thi giao tiep giua WPF va dich vu BFF (Presentation Music API Client).
    /// 
    /// Tac dung:
    /// - Hien thuc hoa giao dien IMusicApiClient, thuc hien cac cuoc goi API bat dong bo toi endpoint /api/v1/search cua BFF.
    /// - Cung cap phuong thuc SearchTracksAsync tra ve SearchResponseDto da duoc deserialize tu dong.
    /// - Cung cap phuong thuc SearchTracksRawAsync tra ve chuoi JSON nguyen ban.
    /// 
    /// Van de giai quyet:
    /// - Quan ly vong doi ket noi TCP (Socket Lifecycle): Su dung the hien HttpClientInstance dang Singleton tinh
    ///   nham loai bo triet de loi Socket Exhaustion khi nguoi dung thuc hien tim kiem lien tuc.
    /// - Thiet lap Tls12 va Tls11 cho tien trinh ung dung tren .NET Framework 4.6.1.
    /// - Gioi han thoi gian cho (Timeout 4 giay) de tranh treo giao dien khi may chu backend bi nghen.
    /// - Ho tro CancellationToken: cho phep huy yeu cau mang cu khi nguoi dung go tiep ky tu moi vao o tim kiem.
    /// 
    /// Cach thuc van hanh:
    /// - Gui yeu cau HTTP GET toi BFF Server Host (http://localhost:5245/api/v1/search?query=...&amp;limit=...).
    /// - Doc stream phan hoi bang HttpCompletionOption.ResponseHeadersRead va deserialize bang JsonConvert.
    /// </summary>
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
