using System;

namespace MusicApp.Core.Interfaces
{
    public interface IDspEqualizerService
    {
        bool IsEnabled { get; set; }
        float[] BandFrequencies { get; }
        float[] BandGains { get; }

        event EventHandler EqualizerChanged;

        void SetBandGain(int bandIndex, float gainDb);
        void SetAllBands(float[] gains);
    }
}
