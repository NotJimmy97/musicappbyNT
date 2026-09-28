using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MusicApp.Core.Models;

namespace MusicApp.Core.Services
{
    /// <summary>
    /// Bộ phân tích cú pháp tập tin lời bài hát chuẩn LRC.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Parse text định dạng LRC thành LyricLine list.
    /// KHÔNG chịu trách nhiệm: Đọc file từ ổ cứng hay mạng.
    /// Vòng đời: Lớp tĩnh hoặc Transient.
    /// Luồng: Hàm thuần túy, an toàn luồng khi dùng Regex đã biên dịch.
    /// </remarks>
    public class LrcParser
    {
        // Regex nhan dien timestamp tieu chuan dang [mm:ss] hoac [mm:ss.xx] hoac [mm:ss.xxx]
        private static readonly Regex TimestampRegex = new Regex(@"\[(\d{1,2}):(\d{2})(?:[.:](\d{1,3}))?\]", RegexOptions.Compiled);

        // Regex nhan dien tag dieu chinh do lech thoi gian toan cuc dang [offset:+/-ms]
        private static readonly Regex OffsetRegex = new Regex(@"\[offset:\s*([+-]?\d+)\s*\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Phan tich toan bo noi dung chuoi LRC va tra ve danh sach dong loi da duoc sap xep theo trinh tu thoi gian.
        /// </summary>
        /// <param name="lrcContent">Chuoi ky tu chua toan bo noi dung tap tin .lrc.</param>
        /// <returns>Danh sach IReadOnlyList chua cac LyricLine hop le.</returns>
        public IReadOnlyList<LyricLine> Parse(string lrcContent)
        {
            if (string.IsNullOrWhiteSpace(lrcContent))
            {
                return new List<LyricLine>();
            }

            // Doc the offset toan cuc (neu co) de bu tru do lech thoi gian cho toan bo bai hat
            int offsetMs = 0;
            var offsetMatch = OffsetRegex.Match(lrcContent);
            if (offsetMatch.Success && int.TryParse(offsetMatch.Groups[1].Value, out int parsedOffset))
            {
                offsetMs = parsedOffset;
            }

            var parsedEntries = new List<Tuple<TimeSpan, string>>();

            using (var reader = new StringReader(lrcContent))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();

                    // Bo qua dong trong va cac the metadata khong phai loi bai hat
                    if (string.IsNullOrEmpty(trimmed) || 
                        trimmed.StartsWith("[ti:", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("[ar:", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("[al:", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("[by:", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("[offset:", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var matches = TimestampRegex.Matches(trimmed);
                    if (matches.Count == 0)
                    {
                        continue;
                    }

                    // Noi dung loi nam o phan sau cung cua timestamp cuoi cung tren dong
                    var lastMatch = matches[matches.Count - 1];
                    string lyricText = trimmed.Substring(lastMatch.Index + lastMatch.Length).Trim();

                    // Xu ly truong hop mot dong co nhieu timestamp (vi du: [00:12.00][01:30.00] Doan diep khuc lap lai)
                    for (int i = 0; i < matches.Count; i++)
                    {
                        var m = matches[i];
                        if (int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes) &&
                            int.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int seconds))
                        {
                            int millis = 0;
                            if (m.Groups[3].Success)
                            {
                                string fracStr = m.Groups[3].Value;
                                // Dinh dang 2 chu so la centiseconds (1/100s -> nhan 10 thanh ms)
                                if (fracStr.Length == 2 && int.TryParse(fracStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int centis))
                                {
                                    millis = centis * 10;
                                }
                                // Dinh dang 3 chu so la milliseconds (1/1000s)
                                else if (fracStr.Length == 3 && int.TryParse(fracStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ms))
                                {
                                    millis = ms;
                                }
                                else if (int.TryParse(fracStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int generalMs))
                                {
                                    millis = generalMs;
                                }
                            }

                            // Tinh tong thoi gian theo mili-giay va cong them do lech offset
                            long totalMs = (minutes * 60L + seconds) * 1000L + millis + offsetMs;
                            if (totalMs < 0) totalMs = 0;

                            parsedEntries.Add(Tuple.Create(TimeSpan.FromMilliseconds(totalMs), lyricText));
                        }
                    }
                }
            }

            if (parsedEntries.Count == 0)
            {
                return new List<LyricLine>();
            }

            // Sap xep cac cau hat tang dan theo trinh tu thoi gian phat
            var sorted = parsedEntries.OrderBy(e => e.Item1).ToList();
            var result = new List<LyricLine>(sorted.Count);

            for (int i = 0; i < sorted.Count; i++)
            {
                result.Add(new LyricLine(sorted[i].Item1, sorted[i].Item2, i));
            }

            return result;
        }
    }
}
