using System.Windows.Controls;

namespace MusicApp.Views
{
    /// <summary>
    /// Code-behind cua man hinh quet thu vien am nhac cuc bo (Local Library Scanner View Code-Behind).
    /// 
    /// Tac dung:
    /// - Khoi tao giao dien quet tap tin am thanh tren may tinh tu LocalLibraryScannerView.xaml.
    /// - Cung cap cac thanh phan UI: O chon duong dan, nut duyet thu muc (Browse), nut quet (Scan),
    ///   thanh trang thai tien do (ProgressBar &amp; StatusMessage), o loc tim kiem nhanh va bang danh sach DataGrid.
    /// 
    /// Van de giai quyet:
    /// - Tuan thu nguyen ly MVVM: Khong viet logic quet I/O trong lop nay.
    /// - Moi hanh dong cua nguoi dung deu binding truc tiep sang LocalLibraryViewModel
    ///   (BrowseCommand, ScanCommand, CancelScanCommand, PlayTrackCommand).
    /// </summary>
    public partial class LocalLibraryScannerView : UserControl
    {
        /// <summary>
        /// Khoi tao UserControl LocalLibraryScannerView.
        /// </summary>
        public LocalLibraryScannerView()
        {
            InitializeComponent();
        }
    }
}
