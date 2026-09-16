using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MusicApp.Core.Models;

namespace MusicApp.Core.Services
{
    public class LrcParser
    {
        private static readonly Regex TimestampRegex = new Regex(@"\[(\d{1,2}):(\d{2})(?:[.:](\d{1,3}))?\]", RegexOptions.Compiled);
        private static readonly Regex OffsetRegex = new Regex(@"\[offset:\s*([+-]?\d+)\s*\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IReadOnlyList<LyricLine> Parse(string lrcContent)
        {
            if (string.IsNullOrWhiteSpace(lrcContent))
            {
                return new List<LyricLine>();
            }

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
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("[ti:", StringComparison.OrdinalIgnoreCase) ||
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

                    // Text is everything following the last timestamp match
                    var lastMatch = matches[matches.Count - 1];
                    string lyricText = trimmed.Substring(lastMatch.Index + lastMatch.Length).Trim();

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
                                if (fracStr.Length == 2 && int.TryParse(fracStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int centis))
                                {
                                    millis = centis * 10;
                                }
                                else if (fracStr.Length == 3 && int.TryParse(fracStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ms))
                                {
                                    millis = ms;
                                }
                                else if (int.TryParse(fracStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int generalMs))
                                {
                                    millis = generalMs;
                                }
                            }

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

            // Sort lines chronologically
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

