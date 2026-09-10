using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MusicApp.Core.Dtos;
using MusicApp.Core.Interfaces;

namespace MusicApp.Bff.Providers
{
    public class VietnameseMusicSourceProvider : IMusicSourceProvider
    {
        private static readonly HttpClient HttpClientInstance;

        private static readonly Dictionary<string, string> AudioUrlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "vn_track_01", "https://archive.org/download/goi-ten-bon-mua/Diem%20xua.mp3" },
            { "vn_track_02", "https://archive.org/download/goi-ten-bon-mua/Ha%20trang.mp3" },
            { "vn_track_03", "https://archive.org/download/goi-ten-bon-mua/Con%20tuoi%20nao%20cho%20em.mp3" },
            { "vn_track_04", "https://archive.org/download/goi-ten-bon-mua/Mua%20hong.mp3" },
            { "vn_track_05", "https://archive.org/download/goi-ten-bon-mua/Nang%20thuy%20tinh.mp3" },
            { "vn_track_06", "https://archive.org/download/goi-ten-bon-mua/Bien%20nho.mp3" },
            { "vn_track_07", "https://archive.org/download/goi-ten-bon-mua/Cat%20bui.mp3" },
            { "vn_track_08", "https://archive.org/download/goi-ten-bon-mua/Goi%20ten%20bon%20mua.mp3" },
            { "vn_track_09", "https://archive.org/download/goi-ten-bon-mua/Mot%20coi%20di%20ve.mp3" },
            { "vn_track_10", "https://archive.org/download/goi-ten-bon-mua/Nhun%20canh%20vac%20bay.mp3" },
            { "vn_track_11", "https://archive.org/download/chieu-tay-do/Chieu%20Tay%20Do.mp3" },
            { "vn_track_12", "https://archive.org/download/cd-trinh-cong-son-dac-biet-1/01.%20Uot%20mi.mp3" }
        };

        private static readonly List<TrackDto> CuratedCatalog = new List<TrackDto>
        {
            new TrackDto
            {
                Id = "vn_track_01",
                Title = "Diễm Xưa",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 251,
                CoverImageUrl = "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_01",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_02",
                Title = "Hạ Trắng",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 326,
                CoverImageUrl = "https://images.unsplash.com/photo-1445985543470-41fdd5c31447?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_02",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_03",
                Title = "Còn Tuổi Nào Cho Em",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 324,
                CoverImageUrl = "https://images.unsplash.com/photo-1465847899084-d164df4dedc6?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_03",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_04",
                Title = "Mưa Hồng",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 297,
                CoverImageUrl = "https://images.unsplash.com/photo-1515694346937-94d85e41e6f0?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_04",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_05",
                Title = "Nắng Thủy Tinh",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 318,
                CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_05",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_06",
                Title = "Biển Nhớ",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 316,
                CoverImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_06",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_07",
                Title = "Cát Bụi",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 305,
                CoverImageUrl = "https://images.unsplash.com/photo-1511192336575-5a79af67a629?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_07",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_08",
                Title = "Gọi Tên Bốn Mùa",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 398,
                CoverImageUrl = "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_08",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_09",
                Title = "Một Cõi Đi Về",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 278,
                CoverImageUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_09",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_10",
                Title = "Như Cánh Vạc Bay",
                Artist = "Kim Tuấn (Hòa Tấu Guitar)",
                Album = "Gọi Tên Bốn Mùa",
                DurationSeconds = 368,
                CoverImageUrl = "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_10",
                License = "CC BY-ND 4.0",
                Genre = "Acoustic Việt"
            },
            new TrackDto
            {
                Id = "vn_track_11",
                Title = "Chiều Tây Đô",
                Artist = "Hương Lan",
                Album = "Tình Ca Quê Hương",
                DurationSeconds = 275,
                CoverImageUrl = "https://images.unsplash.com/photo-1528127269322-539801943592?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_11",
                License = "Public Domain",
                Genre = "V-Pop"
            },
            new TrackDto
            {
                Id = "vn_track_12",
                Title = "Ướt Mi",
                Artist = "Hà Thanh",
                Album = "Trịnh Công Sơn Tuyển Chọn",
                DurationSeconds = 300,
                CoverImageUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=300&q=80",
                StreamEndpoint = "/api/v1/stream/vn_track_12",
                License = "Public Domain",
                Genre = "Nhạc Trịnh"
            }
        };

        static VietnameseMusicSourceProvider()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            HttpClientInstance = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        public string ProviderName => "VietnameseMusic";

        public Task<List<TrackDto>> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken)
        {
            var matched = GetTracksMatching(query);
            int count = Math.Min(matched.Count, limit > 0 ? limit : 20);
            return Task.FromResult(matched.GetRange(0, count));
        }

        public async Task<Stream> GetAudioStreamAsync(string trackId, long? startByte, long? endByte, CancellationToken cancellationToken)
        {
            string audioUrl = ResolveTrackAudioUrl(trackId);
            if (string.IsNullOrEmpty(audioUrl))
            {
                throw new FileNotFoundException("Track not found: " + trackId);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, audioUrl);
            if (startByte.HasValue)
            {
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte.Value, endByte);
            }

            var response = await HttpClientInstance.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public string ResolveTrackAudioUrl(string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId))
            {
                return string.Empty;
            }

            if (AudioUrlMap.TryGetValue(trackId, out string url))
            {
                return url;
            }

            return string.Empty;
        }

        public List<TrackDto> GetAllTracks()
        {
            return new List<TrackDto>(CuratedCatalog);
        }

        public List<TrackDto> GetTracksMatching(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<TrackDto>(CuratedCatalog);
            }

            string q = query.Trim();
            string qNormalized = RemoveDiacritics(q).ToLowerInvariant();

            if (qNormalized.Contains("viet") || qNormalized == "vpop" || qNormalized == "v-pop")
            {
                return new List<TrackDto>(CuratedCatalog);
            }

            var matched = new List<TrackDto>();

            foreach (var track in CuratedCatalog)
            {
                string titleNorm = RemoveDiacritics(track.Title ?? string.Empty).ToLowerInvariant();
                string artistNorm = RemoveDiacritics(track.Artist ?? string.Empty).ToLowerInvariant();
                string albumNorm = RemoveDiacritics(track.Album ?? string.Empty).ToLowerInvariant();
                string genreNorm = RemoveDiacritics(track.Genre ?? string.Empty).ToLowerInvariant();

                if (titleNorm.Contains(qNormalized) ||
                    artistNorm.Contains(qNormalized) ||
                    albumNorm.Contains(qNormalized) ||
                    genreNorm.Contains(qNormalized))
                {
                    matched.Add(track);
                }
            }

            return matched;
        }

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }
    }
}
