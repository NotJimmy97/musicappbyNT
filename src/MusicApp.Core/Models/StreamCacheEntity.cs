namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể quản lý bộ nhớ đệm luồng âm thanh trực tuyến (Stream Cache).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Ánh xạ File đệm với chuỗi Hash của bài hát trực tuyến.
    /// KHÔNG chịu trách nhiệm: Xóa file vật lý.
    /// Vòng đời: Tồn tại trong bộ nhớ khi load từ DB.
    /// Ràng buộc: TrackHash duy nhất, FilePath phải hợp lệ trên đĩa.
    /// </remarks>
    public class StreamCacheEntity
    {
        public string TrackHash { get; set; }
        public string FilePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string LastAccessedAt { get; set; }
        public bool IsFullyCached { get; set; }
    }
}
