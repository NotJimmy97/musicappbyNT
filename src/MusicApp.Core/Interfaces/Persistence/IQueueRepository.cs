using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện lưu trữ hàng đợi phát nhạc hiện tại.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu và phục hồi danh sách bài hát đang chờ phát.
    /// KHÔNG chịu trách nhiệm: Quản lý bài đang phát hoặc điều khiển player.
    /// Vòng đời: Transient/Scoped tùy DI.
    /// Luồng/DB: Thao tác I/O lưu trữ (DB/File) chạy bất đồng bộ trên background thread.
    /// </remarks>
    public interface IQueueRepository
    {
        Task SaveQueueAsync(IEnumerable<int> trackIds);
        Task<IEnumerable<TrackEntity>> LoadQueueAsync();
        Task ClearQueueAsync();
    }
}
