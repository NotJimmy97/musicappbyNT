using System;

namespace MusicApp.Core.Models
{
    public class LyricLine
    {
        public TimeSpan Timestamp { get; }
        public string Text { get; }
        public int Index { get; }

        public LyricLine(TimeSpan timestamp, string text, int index)
        {
            Timestamp = timestamp;
            Text = text ?? string.Empty;
            Index = index;
        }

        public override string ToString()
        {
            return string.Format("[{0:mm\\:ss\\.ff}] {1}", Timestamp, Text);
        }
    }
}

