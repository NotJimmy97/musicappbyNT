using System.Threading.Tasks;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien luu tru cai dat cau hinh ung dung dang key-value (Settings Repository).
    /// </summary>
    public interface ISettingsRepository
    {
        Task<T> GetAsync<T>(string key, T defaultValue = default(T));
        Task SetAsync<T>(string key, T value);
    }
}
