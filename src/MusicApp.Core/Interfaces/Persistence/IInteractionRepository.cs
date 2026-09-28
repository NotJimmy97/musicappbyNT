using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện ghi nhận nhật ký tương tác phục vụ gợi ý.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Log hành vi nghe nhạc của người dùng.
    /// KHÔNG chịu trách nhiệm: Tính toán gợi ý (Recommendation logic).
    /// Vòng đời: Transient/Scoped tùy DI, vòng đời gắn với yêu cầu truy xuất DB.
    /// Luồng/DB: Thao tác qua Task bất đồng bộ, sử dụng DB Connection string mặc định.
    /// </remarks>
    public interface IInteractionRepository
    {
        Task LogInteractionAsync(int trackId, string actionType, int durationPlayedSeconds);
        Task<IEnumerable<UserInteractionEntity>> GetRecentInteractionsAsync(int limit = 100);
    }
}
