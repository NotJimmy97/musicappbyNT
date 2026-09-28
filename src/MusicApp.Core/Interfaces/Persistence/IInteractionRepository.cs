using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien ghi nhan nhat ky tuong tac phuc vu thuat toan goi y (Interaction Logger).
    /// </summary>
    public interface IInteractionRepository
    {
        Task LogInteractionAsync(int trackId, string actionType, int durationPlayedSeconds);
        Task<IEnumerable<UserInteractionEntity>> GetRecentInteractionsAsync(int limit = 100);
    }
}
