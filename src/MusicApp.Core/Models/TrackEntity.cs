using System;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể đại diện cho một bản nhạc trong hệ thống lưu trữ hợp nhất (Unified Track Catalog).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Chứa thông tin metadata của bản nhạc.
    /// KHÔNG chịu trách nhiệm: Thực thi logic truy xuất DB hay phát nhạc.
    /// Vòng đời: Tồn tại trong bộ nhớ khi được query hoặc khởi tạo.
    /// Ràng buộc: TrackKey phải duy nhất (Fuzzy Fingerprint) chống trùng lặp. SourceType phân biệt nguồn (local/jamendo/vn).
    /// </remarks>
    public class TrackEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Ma van tay chuan hoa de chong trung lap (Fuzzy Fingerprint Key: "title::artist").
        /// </summary>
        public string TrackKey { get; set; }

        /// <summary>
        /// Nguon ban nhac: 'local' | 'jamendo' | 'vn'
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// Duong dan tep tren o cung (Local FilePath) hoac ID luong truc tuyen.
        /// </summary>
        public string SourceId { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public int DurationSeconds { get; set; }
        public int Bitrate { get; set; }

        /// <summary>
        /// Duong dan tep anh thumbnail 120x120 tren dia (%LOCALAPPDATA%\MusicApp\Covers\{hash}.jpg).
        /// </summary>
        public string CoverUri { get; set; }

        /// <summary>
        /// Thoi diem sua doi tep tren dia (ISO 8601 UTC) phuc vu co che Quet gia tang (Incremental Scan).
        /// </summary>
        public string FileMTime { get; set; }

        public int PlayCount { get; set; }
        public int SkipCount { get; set; }
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Diem so quan he voi nguoi dung phuc vu thuat toan goi y (Affinity Recommendation Engine).
        /// </summary>
        public double AffinityScore { get; set; }

        public string LastPlayedAt { get; set; }
        public string CreatedAt { get; set; }
    }
}
