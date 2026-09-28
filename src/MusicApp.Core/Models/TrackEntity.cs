using System;

namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the dai dien cho mot ban nhac trong he thong luu tru hop nhat (Unified Track Catalog).
    /// Tuong thich ca nguon Local Files va Streaming CDN (Jamendo, Zing, Archive.org).
    /// </summary>
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
