using System.Threading.Tasks;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện lưu trữ cài đặt cấu hình ứng dụng.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Get/Set cấu hình dạng Key-Value (chủ đề, âm lượng,...).
    /// KHÔNG chịu trách nhiệm: Validate giá trị cấu hình theo nghiệp vụ.
    /// Vòng đời: Transient/Scoped tùy DI.
    /// Luồng/DB: Thao tác I/O chạy bất đồng bộ trên background thread.
    /// </remarks>
    public interface ISettingsRepository
    {
        Task<T> GetAsync<T>(string key, T defaultValue = default(T));
        Task SetAsync<T>(string key, T value);
    }
}
