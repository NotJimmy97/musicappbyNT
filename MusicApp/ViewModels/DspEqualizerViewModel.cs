using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MusicApp.Core.Common;
using MusicApp.Core.Interfaces;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quan ly bo can bang am thanh ky thuat so 10 bang tan (10-Band DSP Equalizer ViewModel).
    /// 
    /// Tac dung:
    /// - Cung cap danh sach 10 bang tan EqualizerBandViewModel cho giao dien DspEqualizerView.xaml.
    /// - Cung cap cac cau hinh am thanh cai dat san (Presets: Flat, Rock, Pop, Jazz, Classical, Bass Boost, Vocal Boost).
    /// - Quan ly trang thai Bat/Tat (Bypass Mode) va chuc nang Reset ve mac dinh Flat (0 dB).
    /// - Tu dong nhan dien cau hinh dang ap dung (Preset Detection) khi nguoi dung keo chinh bat ky thanh Slider nao.
    /// 
    /// Van de giai quyet:
    /// - Tranh hien tuong xung dot vong lap phan hoi (Feedback Loop Ping-pong):
    ///   Khi nguoi dung chon mot Preset, cac Slider phai duoc cap nhat gia tri moi nhung KHONG duoc phep
    ///   goi nguoc lai ham ApplyPreset. Bien co _isUpdatingInternally va phuong thuc SetGainSilent
    ///   ngan chan triet de van de nay.
    /// - Tuan thu chat che nguyen ly phan tach trach nhiem (Separation of Concerns): Giao dien chi tuong tac voi ViewModel;
    ///   moi phep tinh toan DSP thuc su duoc uy thac hoan toan xuong IDspEqualizerService.
    /// 
    /// Cach thuc van hanh:
    /// - Khoi tao 10 doi tuong EqualizerBandViewModel voi tan so tieu chuan tu 32Hz den 16kHz.
    /// - Khi mot bang tan thay doi do loi, OnBandGainChanged goi _equalizerService.SetBandGain va goi DetectMatchingPreset.
    /// </summary>
    public class DspEqualizerViewModel : ObservableObject
    {
        private readonly IDspEqualizerService _equalizerService;
        private bool _isUpdatingInternally;

        // Bang cac duong cong am thanh cai dat san (Preset Curves) tinh theo do loi dB tren 10 bang tan
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

        // Nhan van ban tan so trung tam hien thi phia duoi tung Slider
        private static readonly string[] BandLabels = new string[]
        {
            "32Hz", "64Hz", "125Hz", "250Hz", "500Hz", "1kHz", "2kHz", "4kHz", "8kHz", "16kHz"
        };

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
