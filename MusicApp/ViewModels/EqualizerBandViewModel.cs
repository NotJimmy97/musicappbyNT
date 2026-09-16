using System;
using MusicApp.Core.Common;

namespace MusicApp.ViewModels
{
    public class EqualizerBandViewModel : ObservableObject
    {
        private readonly Action<int, float> _onGainChanged;
        private float _gainDb;

        public int BandIndex { get; }
        public float Frequency { get; }
        public string Label { get; }

        public float GainDb
        {
            get => _gainDb;
            set
            {
                float clamped = (float)Math.Round(Math.Max(-12.0f, Math.Min(12.0f, value)), 1);
                if (SetProperty(ref _gainDb, clamped))
                {
                    OnPropertyChanged(nameof(FormattedGain));
                    _onGainChanged?.Invoke(BandIndex, clamped);
                }
            }
        }

        public string FormattedGain => _gainDb >= 0.05f ? string.Format("+{0:0.0} dB", _gainDb) : string.Format("{0:0.0} dB", _gainDb);

        public EqualizerBandViewModel(int bandIndex, float frequency, string label, float initialGainDb, Action<int, float> onGainChanged)
        {
            BandIndex = bandIndex;
            Frequency = frequency;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            _gainDb = (float)Math.Round(initialGainDb, 1);
            _onGainChanged = onGainChanged;
        }

        public void SetGainSilent(float gainDb)
        {
            float clamped = (float)Math.Round(Math.Max(-12.0f, Math.Min(12.0f, gainDb)), 1);
            if (SetProperty(ref _gainDb, clamped))
            {
                OnPropertyChanged(nameof(FormattedGain));
            }
        }
    }
}

