using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Bo tien ich tao ma van tay chuan hoa giup hop nhat va khu trung lap bai hat giua cac nguon
    /// (Track Identity & Deduplication Helper).
    /// </summary>
    public static class TrackIdentityHelper
    {
        /// <summary>
        /// Sinh khoa van tay duy nhat tu Title va Artist.
        /// </summary>
        public static string GenerateTrackKey(string title, string artist)
        {
            string cleanTitle = Normalize(title);
            string cleanArtist = Normalize(artist);

            if (string.IsNullOrEmpty(cleanTitle))
            {
                cleanTitle = "unknown";
            }
            if (string.IsNullOrEmpty(cleanArtist))
            {
                cleanArtist = "unknown";
            }

            return $"{cleanTitle}::{cleanArtist}";
        }

        /// <summary>
        /// Chuan hoa chuoi: bo dau tieng Viet, bo tag [Official MV], (Remix)..., dua ve chu thuong khong dau.
        /// </summary>
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string text = input.Trim().ToLowerInvariant();

            // Loai bo cac the ngoac thuong gap tren mang nhu [Official Music Video], (Audio), (Lyric Video)...
            text = Regex.Replace(text, @"\[.*?\]|\(.*?\)", "");

            // Loai bo cac hau to ft., feat., prod.
            text = Regex.Replace(text, @"\b(ft\.|feat\.|featuring|prod\.)\b.*", "");

            // Loai bo dau tieng Viet (Unicode Normalization Form D)
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

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Thay the cac ky tu d dac biet
            result = result.Replace('đ', 'd').Replace('Đ', 'd');

            // Loai bo toan bo ky tu dac biet, chi giu chu cai, chu so va dau cach
            result = Regex.Replace(result, @"[^a-z0-9\s]", " ");

            // Rut gon nhieu dau cach lien tiep thanh 1 dau cach
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }
    }
}
