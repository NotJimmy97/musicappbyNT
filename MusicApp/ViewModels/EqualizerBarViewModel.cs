using MusicApp.Core.Common;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel dai dien cho mot cot hien thi song nhac FFT (Audio Spectrum Visualizer Bar).
    /// 
    /// Tac dung:
    /// - Quan ly gia tri bien do song (Value) va mau sac hien thi (ColorHex) cua mot cot trong dai 16 cot Visualizer.
    /// - Ke thua ObservableObject de phat tin hieu INotifyPropertyChanged khi gia tri bien do duoc cap nhat tu Audio Engine.
    /// 
    /// Van de giai quyet:
    /// - Cho phep ItemsControl trong XAML tu dong co dan chieu cao cot song nhac (Height) theo thoi gian thuc.
    /// - Cung cap mau sac tuy bien theo dai tan so (mau tim neon cho am tram Bass, mau xanh ngoc cyan cho am trung Mid, mau hong cho am cao Treble).
    /// 
    /// Cach thuc van hanh:
    /// - DspEqualizerViewModel nhan du lieu SpectrumDataReady (30fps) tu IAudioService va cap nhat Value cho tung cot.
    /// </summary>
    public class EqualizerBarViewModel : ObservableObject
    {
        private double _value;

        /// <summary>
        /// Gia tri bien do hien tai cua cot song nhac (dao dong tu 0.0 den 35.0 don vi chieu cao UI).
        /// </summary>
        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private string _colorHex;

        /// <summary>
        /// Ma mau sac HEX dai dien cho cot song nhac (vi du: #1DB954, #8A2BE2, #00FFFF).
        /// </summary>
        public string ColorHex
        {
            get => _colorHex;
            set => SetProperty(ref _colorHex, value);
        }

        /// <summary>
        /// Khoi tao mot cot visualizer voi bien do ban dau va mau sac chu dao.
        /// </summary>
        /// <param name="initialValue">Gia tri bien do ban dau.</param>
        /// <param name="colorHex">Ma mau sac HEX.</param>
        public EqualizerBarViewModel(double initialValue, string colorHex)
        {
            _value = initialValue;
            _colorHex = colorHex;
        }
    }
}
