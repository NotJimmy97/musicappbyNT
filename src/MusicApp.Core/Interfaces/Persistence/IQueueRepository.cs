using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien luu tru ben vung hang doi phat nhac hien tai (Play Queue Persistence Repository).
    /// </summary>
    public interface IQueueRepository
    {
        Task SaveQueueAsync(IEnumerable<int> trackIds);
        Task<IEnumerable<TrackEntity>> LoadQueueAsync();
        Task ClearQueueAsync();
    }
}
