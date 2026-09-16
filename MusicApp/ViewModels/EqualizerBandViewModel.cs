using System;
using MusicApp.Core.Common;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quan ly mot bang can bang tan so cu the (Equalizer Frequency Band Slider).
    /// 
    /// Tac dung:
    /// - Dong goi cac thuoc tinh: Chi so bang tan (BandIndex), Tan so trung tam (Frequency),
    ///   Nhan hien thi (Label: "32Hz", "1kHz",...), va Muc do loi (GainDb [-12.0dB..+12.0dB]).
    /// - Cung cap chuoi dinh dang do loi co dau cong tru (FormattedGain: "+3.5 dB", "-2.0 dB").
    /// - Phat delegate _onGainChanged khi nguoi dung thao tac keo Slider tren giao dien.
    /// 
    /// Van de giai quyet:
    /// - Cho phep phan tach ro rang giua thao tac nguoi dung (User Gesture) va viec nap gia tri tu Preset (Programmatic Update).
    /// - Phuong thuc SetGainSilent cho phep cap nhat giao dien khi chon Preset ma KHONG kich hoat lai su kien _onGainChanged,
    ///   tranh gay ra vong lap vo tan (Event Loop Ping-pong) giua ViewModel va DSP Engine.
    /// 
    /// Cach thuc van hanh:
    /// - Slider trong DspEqualizerView.xaml TwoWay Binding voi GainDb.
    /// - Khi GainDb thay doi, kiem tra SetProperty va kich hoat _onGainChanged(BandIndex, clampedGain).
    /// </summary>
    public class EqualizerBandViewModel : ObservableObject
    {
        private readonly Action<int, float> _onGainChanged;
        private float _gainDb;

        /// <summary>
        /// Chi so thu tu cua bang tan (0 den 9).
        /// </summary>
        public int BandIndex { get; }

        /// <summary>
        /// Tan so trung tam cua bang tan tinh bang Hertz (Hz).
        /// </summary>
        public float Frequency { get; }

        /// <summary>
        /// Nhan van ban dai dien cho tan so hien thi tren giao dien (vi du: "32Hz", "125Hz", "1kHz",...).
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Do loi hien tai cua bang tan tinh theo dB (gioi han trong khoang [-12.0dB, +12.0dB]).
        /// </summary>
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

        /// <summary>
        /// Chuoi van ban bieu dien do loi da duoc dinh dang (vi du: "+4.0 dB", "0.0 dB", "-3.5 dB").
        /// </summary>
        public string FormattedGain => _gainDb >= 0.05f ? string.Format("+{0:0.0} dB", _gainDb) : string.Format("{0:0.0} dB", _gainDb);

        /// <summary>
        /// Khoi tao bang tan voi day du thong so va delegate lang nghe su thay doi.
        /// </summary>
        /// <param name="bandIndex">Chi so bang tan tu 0 den 9.</param>
        /// <param name="frequency">Tan so trung tam (Hz).</param>
        /// <param name="label">Nhan hien thi.</param>
        /// <param name="initialGainDb">Muc do loi ban dau (dB).</param>
        /// <param name="onGainChanged">Hanh dong callback duoc goi khi do loi thay doi tu UI.</param>
        public EqualizerBandViewModel(int bandIndex, float frequency, string label, float initialGainDb, Action<int, float> onGainChanged)
        {
            BandIndex = bandIndex;
            Frequency = frequency;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            _gainDb = (float)Math.Round(initialGainDb, 1);
            _onGainChanged = onGainChanged;
        }

        /// <summary>
        /// Thiet lap do loi moi trong che do im lang (khong kich hoat callback _onGainChanged).
        /// Su dung khi ap dung cac Preset cai dat san de tranh goi nguoc xuong DSP service.
        /// </summary>
        /// <param name="gainDb">Gia tri do loi moi (dB).</param>
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
