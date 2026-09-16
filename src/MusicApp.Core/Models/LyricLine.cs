using System;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the dai dien cho mot dong loi bai hat dong bo theo thoi gian (Synchronized Lyric Line).
    /// 
    /// Tac dung:
    /// - Luu tru thoi diem bat dau (Timestamp), noi dung loi (Text) va thu tu hien thi (Index) cua cau hat.
    /// - Cung cap cau truc du lieu bat bien (Immutable Object) giup dam bao an toan tuyet doi khi truy cap da luong.
    /// 
    /// Van de giai quyet:
    /// - Phu vu tinh nang cuon loi bai hat tu dong (Karaoke-style Auto-scroll) theo vi tri phat am thanh hien tai.
    /// - Tranh tinh trang sai lech moc thoi gian hoac bi sua doi gia tri trong qua trinh phat.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc tao ra boi LrcParser sau khi phan tich noi dung dinh dang .lrc.
    /// - LyricsViewModel se so sanh Timestamp cua tung LyricLine voi CurrentTime cua IAudioService 
    ///   de tim ra dong loi hat hien tai bang thuat toan Binary Search O(log N).
    /// </summary>
    public class LyricLine
    {
        /// <summary>
        /// Moc thoi gian bat dau cau hat trong ban nhac.
        /// </summary>
        public TimeSpan Timestamp { get; }

        /// <summary>
        /// Noi dung van ban cua cau hat.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Chi so dong trong danh sach toan bo loi bai hat (bat dau tu 0).
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Khoi tao mot dong loi bai hat bat bien.
        /// </summary>
        /// <param name="timestamp">Moc thoi gian cua cau hat.</param>
        /// <param name="text">Noi dung van ban cau hat.</param>
        /// <param name="index">Thu tu chi so dong.</param>
        public LyricLine(TimeSpan timestamp, string text, int index)
        {
            Timestamp = timestamp;
            Text = text ?? string.Empty;
            Index = index;
        }

        /// <summary>
        /// Dinh dang hien thi dong loi kem timestamp phuc vu debug va logging.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[{0:mm\\:ss\\.ff}] {1}", Timestamp, Text);
        }
    }
}
