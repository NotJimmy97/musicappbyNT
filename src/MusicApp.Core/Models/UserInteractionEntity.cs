namespace MusicApp.Core.Models
{
    /// <summary>
    /// Nhật ký ghi nhận hành vi tương tác của người dùng (User Interaction).
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Log dữ liệu thô phục vụ Recommendation.
    /// KHÔNG chịu trách nhiệm: Tính toán điểm ưu tiên (Affinity score).
    /// Vòng đời: Tồn tại trong quá trình query từ DB.
    /// Ràng buộc: ActionType phải thuộc tập hợp hợp lệ (click, play_start, etc.).
    /// </remarks>
    public class UserInteractionEntity
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string ActionType { get; set; }
        public int DurationPlayedSeconds { get; set; }
        public string CreatedAt { get; set; }
    }
}
