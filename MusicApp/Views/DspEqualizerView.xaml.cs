using System.Windows.Controls;

namespace MusicApp.Views
{
    /// <summary>
    /// Code-behind cho UserControl DspEqualizerView - Giao dien bo loc am sac (Equalizer) va truc quan hoa am pho (Spectrum Analyzer).
    /// 
    /// - Tac dung: Khoi tao va quan ly vong doi cua UserControl DspEqualizerView tren Visual Tree WPF.
    ///   Hien thi he thong 10 dai tan so (31Hz - 16kHz), danh sach Preset cai dat san va thanh pho tan so 16 kenh.
    /// 
    /// - Van de giai quyet: Tuan thu tuyet doi nguyen tac tach biet giao dien MVVM (Zero-Code-Behind pattern).
    ///   Toan bo thao tac dieu chinh Gain cua dai tan, lua chon Preset, bat/tat Equalizer va cap nhat
    ///   do cao cot pho (Spectrum Bar height) duoc thuc hien thuan tuy qua XAML Data Binding vao DspEqualizerViewModel.
    ///   Tranh viec can thiep truc tiep vao control UI tu code C#, giup tang kha nang kiem thu (unit testable) va bao tri.
    /// 
    /// - Cach thuc van hanh:
    ///   1. Khoi tao cac thanh phan do hoa thong qua InitializeComponent() do WPF framework sinh ra tu XAML.
    ///   2. Nhap DataContext tu MainViewModel (DspEqualizerViewModel).
    ///   3. Cac Slider dải tần liên kết Two-Way Binding voi thuoc tinh Gain cua tung EqualizerBandViewModel;
    ///      khi nguoi dung keo Slider, ViewModel lap tuc cap nhat he so khuyech dai sang DspEqualizerSampleProvider
    ///      trong Audio Engine ma khong gay tre hay giat am thanh.
    /// </summary>
    public partial class DspEqualizerView : UserControl
    {
        /// <summary>
        /// Khoi tao mot instance moi cua DspEqualizerView va nap cay phan tu XAML.
        /// </summary>
        public DspEqualizerView()
        {
            InitializeComponent();
        }
    }
}


