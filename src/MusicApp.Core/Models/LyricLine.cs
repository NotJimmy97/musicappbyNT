using System;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể đại diện cho một dòng lời bài hát đồng bộ thời gian (Synchronized Lyric Line).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ dữ liệu dòng text và timestamp từ file LRC.
    /// KHÔNG chịu trách nhiệm: Cuộn lời bài hát trên UI.
    /// Vòng đời: Tạo mới khi đọc file LRC và xóa khi chuyển bài.
    /// Ràng buộc: Immutable state, thuộc tính Timestamp phải không âm.
    /// </remarks>
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
