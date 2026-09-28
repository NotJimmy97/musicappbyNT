using System;
using MusicApp.Core.Common;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel quản lý một băng tần số trong bộ cân bằng âm thanh.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Quản lý BandIndex, Frequency, GainDb và phát tín hiệu cho DspEqualizerViewModel.
    /// 2. Không chịu trách nhiệm: Quản lý toàn bộ 10 băng tần hoặc xử lý âm thanh thực tế.
    /// 3. Vòng đời: Tồn tại cùng DspEqualizerViewModel.
    /// 4. Đa luồng: Phương thức SetGainSilent dùng để cập nhật UI từ Preset mà không gây loop event.
    /// State transitions:
    /// - Khi GainDb thay đổi từ UI: Kích hoạt _onGainChanged để báo cho ViewModel cha cập nhật Service.
    /// </remarks>
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
