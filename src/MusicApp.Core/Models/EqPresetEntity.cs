namespace MusicApp.Core.Models
{
    /// <summary>
    /// Thực thể lưu trữ cấu hình cân bằng âm thanh (EQ Preset).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Cung cấp thông số JSON cho bộ Equalizer.
    /// KHÔNG chịu trách nhiệm: Xử lý âm thanh trực tiếp.
    /// Vòng đời: Tồn tại trong bộ nhớ khi load từ DB.
    /// Ràng buộc: Name phải duy nhất. GainsJson chứa chuỗi định dạng float array.
    /// </remarks>
    public class EqPresetEntity
    {
        public string Name { get; set; }
        public string GainsJson { get; set; }
        public bool IsCustom { get; set; }
    }

    /// <summary>
    /// Thực thể lưu trữ trạng thái phiên làm việc để phục hồi (Session Resume).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ thông tin phát lần cuối cùng.
    /// KHÔNG chịu trách nhiệm: Thực thi logic phục hồi.
    /// Vòng đời: Tồn tại trong bộ nhớ khi load từ DB.
    /// Ràng buộc: Chỉ có 1 bản ghi trong DB (Id = 1).
    /// </remarks>
    public class AppSessionStateEntity
    {
        public int Id { get; set; } = 1;
        public int? LastTrackId { get; set; }
        public double LastPositionSeconds { get; set; }
        public double Volume { get; set; } = 1.0;
        public bool IsMuted { get; set; }
        public int RepeatMode { get; set; }
        public bool IsShuffleEnabled { get; set; }
    }
}
