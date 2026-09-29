using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;
using MusicApp.Core.Models;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quản lý bộ cân bằng âm thanh kỹ thuật số 10 băng tần.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Quản lý trạng thái 10 băng tần EqualizerBandViewModel, các preset cấu hình, và Toggle Bypass.
    /// 2. Không chịu trách nhiệm: Xử lý tín hiệu số DSP (Digital Signal Processing) thực tế (thuộc IDspEqualizerService).
    /// 3. Vòng đời: Singleton/Tồn tại suốt vòng đời ứng dụng.
    /// 4. Đa luồng: Hoạt động hoàn toàn trên UI thread. Sử dụng _isUpdatingInternally để tránh feedback loop khi cập nhật qua lại.
    /// State transitions:
    /// - Khi thay đổi Slider: Cập nhật gain xuống Service và gọi DetectMatchingPreset (chuyển qua Custom nếu không khớp).
    /// - Khi chọn Preset: Đặt toàn bộ băng tần xuống Service, update giá trị lên giao diện qua SetGainSilent.
    /// </remarks>
    public class DspEqualizerViewModel : ObservableObject
    {
        private readonly IDspEqualizerService _equalizerService;
        private bool _isUpdatingInternally;

        // Bang cac duong cong am thanh cai dat san (Preset Curves) doc tu EqPresetCatalog (Single Source of Truth)
        private static readonly Dictionary<string, float[]> PresetCurves =
            EqPresetCatalog.AllPresets.ToDictionary(p => p.Name, p => p.Gains, StringComparer.OrdinalIgnoreCase);

        // Nhan van ban tan so trung tam hien thi phia duoi tung Slider
        private static readonly string[] BandLabels = EqPresetCatalog.BandLabels;

        /// <summary>
        /// Tap hop 10 ViewModel dai dien cho 10 dai tan so Slider tren giao dien.
        /// </summary>
        public ObservableCollection<EqualizerBandViewModel> Bands { get; } = new ObservableCollection<EqualizerBandViewModel>();

        /// <summary>
        /// Danh sach ten cac Preset cai dat san phuc vu ComboBox lua chon.
        /// </summary>
        public ObservableCollection<string> Presets { get; } = new ObservableCollection<string>();

        private string _selectedPreset = "Flat";

        /// <summary>
        /// Ten cau hinh Preset dang duoc lua chon hien tai.
        /// </summary>
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

        /// <summary>
        /// Trang thai kich hoat bo loc Equalizer tren Audio Engine.
        /// </summary>
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

        /// <summary>
        /// Chuoi van ban bieu dien trang thai bo loc ("BẬT (ACTIVE)" hoac "TẮT (BYPASS)").
        /// </summary>
        public string StatusText => IsEnabled ? "B\u1EACT (ACTIVE)" : "T\u1EAET (BYPASS)";

        /// <summary>
        /// Lenh dat lai toan bo cac dai tan ve muc can bang Flat (0 dB).
        /// </summary>
        public RelayCommand ResetCommand { get; }

        /// <summary>
        /// Lenh ap dung mot Preset cu the theo ten.
        /// </summary>
        public RelayCommand ApplyPresetCommand { get; }

        /// <summary>
        /// Lenh bat/tat bo qua bo loc (Toggle Bypass).
        /// </summary>
        public RelayCommand ToggleBypassCommand { get; }

        /// <summary>
        /// Khoi tao DspEqualizerViewModel va nap cac thong so ban dau tu IDspEqualizerService.
        /// </summary>
        /// <param name="equalizerService">Dich vu DSP Equalizer.</param>
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

        /// <summary>
        /// Xu ly khi nguoi dung keo Slider thay doi do loi tren mot bang tan.
        /// </summary>
        private void OnBandGainChanged(int bandIndex, float gainDb)
        {
            if (_isUpdatingInternally) return;

            _equalizerService.SetBandGain(bandIndex, gainDb);
            DetectMatchingPreset();
        }

        /// <summary>
        /// Khoi phuc do loi toan bo 10 bang tan ve 0 dB.
        /// </summary>
        public void ResetToFlat()
        {
            ApplyPreset("Flat");
        }

        /// <summary>
        /// Ap dung cau hinh am thanh tu Preset sang tat ca cac bang tan.
        /// </summary>
        /// <param name="presetName">Ten preset can ap dung.</param>
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

        /// <summary>
        /// Kiem tra xem do loi cua 10 bang tan hien tai co khop voi bat ky Preset co san nao hay khong.
        /// Neu khong khop se tu dong chuyen sang che do "Custom".
        /// </summary>
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
