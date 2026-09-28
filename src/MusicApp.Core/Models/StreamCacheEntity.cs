namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thuc the quan ly bo nho dem luong am thanh truc tuyen (Spotify-style Content-Addressable Storage Cache).
    /// </summary>
    public class StreamCacheEntity
    {
        public string TrackHash { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string LastAccessedAt { get; set; }
        public bool IsFullyCached { get; set; }
    }
}
