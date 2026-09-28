namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể quản lý các thư mục đã được người dùng quét trên máy.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Theo dõi lịch sử quét để tối ưu lần quét sau.
    /// KHÔNG chịu trách nhiệm: Thực hiện logic quét thư mục.
    /// Vòng đời: Tồn tại trong bộ nhớ khi load từ DB.
    /// Ràng buộc: FolderPath là unique key.
    /// </remarks>
    public class ScannedFolderEntity
    {
        public string FolderPath { get; set; }
        public string LastScannedAt { get; set; }
        public int TotalFiles { get; set; }
    }
}
