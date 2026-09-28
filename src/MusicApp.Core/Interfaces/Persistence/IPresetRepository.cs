using System.Collections.Generic;
using System.Threading.Tasks;
using MusicApp.Core.Models;

namespace MusicApp.Core.Interfaces.Persistence
{
    /// <summary>
    /// Giao dien quan ly cac preset DSP Equalizer 10 bang tan (Preset Repository).
    /// </summary>
    public interface IPresetRepository
    {
        Task<IEnumerable<EqPresetEntity>> GetAllAsync();
        Task SaveCustomPresetAsync(string name, float[] gains);
        Task DeleteCustomPresetAsync(string name);
    }
}
