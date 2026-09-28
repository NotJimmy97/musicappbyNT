using MusicApp.Core.Common;

namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel đại diện cho một cột hiển thị sóng nhạc (Spectrum Visualizer Bar).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Lưu trữ Value (biên độ) và ColorHex cho UI binding.
    /// 2. Không chịu trách nhiệm: Xử lý biến đổi FFT (Fast Fourier Transform).
    /// 3. Vòng đời: Tồn tại cùng NowPlayingViewModel.
    /// 4. Đa luồng: Thuộc tính Value được cập nhật rất nhanh từ UI thread Dispatcher thông qua sự kiện âm thanh.
    /// </remarks>
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
