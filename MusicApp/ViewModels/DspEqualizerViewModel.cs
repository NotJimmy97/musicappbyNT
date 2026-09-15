using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;

namespace MusicApp.ViewModels
{
    public class DspEqualizerViewModel : ObservableObject
    {
        private readonly IDspEqualizerService _equalizerService;
        private bool _isUpdatingInternally;

        private static readonly Dictionary<string, float[]> PresetCurves = new Dictionary<string, float[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Flat", new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f } },
            { "Rock", new float[] { 4.5f, 3.5f, 2.0f, -1.0f, -1.5f, 1.0f, 2.5f, 3.5f, 4.5f, 4.5f } },
            { "Pop", new float[] { -1.5f, -1.0f, 1.0f, 2.5f, 3.5f, 3.0f, 1.5f, 0.5f, -0.5f, -1.0f } },
            { "Jazz", new float[] { 3.0f, 2.5f, 1.0f, 1.5f, -1.0f, -1.0f, 0.0f, 1.5f, 2.5f, 3.0f } },
            { "Classical", new float[] { 4.0f, 3.5f, 3.0f, 2.0f, -1.5f, -1.5f, 0.0f, 2.0f, 3.0f, 3.5f } },
            { "Bass Boost", new float[] { 7.0f, 6.0f, 5.0f, 3.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f } },
            { "Vocal Boost", new float[] { -2.0f, -2.0f, -1.0f, 1.5f, 3.5f, 3.5f, 2.5f, 1.0f, 0.0f, -1.0f } }
        };

        private static readonly string[] BandLabels = new string[]
        {
            "32Hz", "64Hz", "125Hz", "250Hz", "500Hz", "1kHz", "2kHz", "4kHz", "8kHz", "16kHz"
        };

        public ObservableCollection<EqualizerBandViewModel> Bands { get; } = new ObservableCollection<EqualizerBandViewModel>();
        public ObservableCollection<string> Presets { get; } = new ObservableCollection<string>();

        private string _selectedPreset = "Flat";
        public string SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (SetProperty(ref _selectedPreset, value))
                {
                    if (!_isUpdatingInternally && value != null && value != "Custom")
                    {
                        ApplyPreset(value);
                    }
                }
            }
        }

        public bool IsEnabled
        {
            get => _equalizerService != null && _equalizerService.IsEnabled;
            set
            {
                if (_equalizerService != null && _equalizerService.IsEnabled != value)
                {
                    _equalizerService.IsEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string StatusText => IsEnabled ? "BẬT (ACTIVE)" : "TẮT (BYPASS)";

        public RelayCommand ResetCommand { get; }
        public RelayCommand ApplyPresetCommand { get; }
        public RelayCommand ToggleBypassCommand { get; }

        public DspEqualizerViewModel(IDspEqualizerService equalizerService)
        {
            _equalizerService = equalizerService ?? throw new ArgumentNullException(nameof(equalizerService));

            foreach (var presetName in PresetCurves.Keys)
            {
                Presets.Add(presetName);
            }
            Presets.Add("Custom");

            float[] freqs = _equalizerService.BandFrequencies;
            float[] gains = _equalizerService.BandGains;

            for (int i = 0; i < 10; i++)
            {
                float freq = (freqs != null && i < freqs.Length) ? freqs[i] : 1000f;
                float gain = (gains != null && i < gains.Length) ? gains[i] : 0f;
                string label = i < BandLabels.Length ? BandLabels[i] : string.Format("{0}Hz", freq);

                var bandVm = new EqualizerBandViewModel(i, freq, label, gain, OnBandGainChanged);
                Bands.Add(bandVm);
            }

            ResetCommand = new RelayCommand(_ => ResetToFlat());
            ToggleBypassCommand = new RelayCommand(_ => IsEnabled = !IsEnabled);
            ApplyPresetCommand = new RelayCommand(p =>
            {
                if (p is string name && PresetCurves.ContainsKey(name))
                {
                    SelectedPreset = name;
                }
            });

            DetectMatchingPreset();
        }

        private void OnBandGainChanged(int bandIndex, float gainDb)
        {
            if (_isUpdatingInternally) return;

            _equalizerService.SetBandGain(bandIndex, gainDb);
            DetectMatchingPreset();
        }

        public void ResetToFlat()
        {
            ApplyPreset("Flat");
        }

        public void ApplyPreset(string presetName)
        {
            if (!PresetCurves.TryGetValue(presetName, out float[] targetCurve))
            {
                return;
            }

            _isUpdatingInternally = true;
            try
            {
                for (int i = 0; i < Bands.Count && i < targetCurve.Length; i++)
                {
                    Bands[i].SetGainSilent(targetCurve[i]);
                }

                _equalizerService.SetAllBands(targetCurve);
                SetProperty(ref _selectedPreset, presetName, nameof(SelectedPreset));
            }
            finally
            {
                _isUpdatingInternally = false;
            }
        }

        private void DetectMatchingPreset()
        {
            _isUpdatingInternally = true;
            try
            {
                string matchedPreset = "Custom";

                foreach (var kvp in PresetCurves)
                {
                    bool match = true;
                    float[] curve = kvp.Value;

                    for (int i = 0; i < Bands.Count && i < curve.Length; i++)
                    {
                        if (Math.Abs(Bands[i].GainDb - curve[i]) > 0.15f)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        matchedPreset = kvp.Key;
                        break;
                    }
                }

                SetProperty(ref _selectedPreset, matchedPreset, nameof(SelectedPreset));
            }
            finally
            {
                _isUpdatingInternally = false;
            }
        }
    }
}
