using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao diện quản lý các preset DSP Equalizer 10 băng tần.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Lưu trữ, truy xuất cấu hình Equalizer.
    /// KHÔNG chịu trách nhiệm: Áp dụng dải tần số lên Audio Engine.
    /// Vòng đời: Transient/Scoped tùy DI.
    /// Luồng/DB: Thao tác qua Task bất đồng bộ, lưu ở DB.
    /// </remarks>
    public interface IPresetRepository
    {
        Task<IEnumerable<EqPresetEntity>> GetAllAsync();
        Task SaveCustomPresetAsync(string name, float[] gains);
        Task DeleteCustomPresetAsync(string name);
    }
}
