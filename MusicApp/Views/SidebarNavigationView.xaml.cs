using System.Windows.Controls;

namespace MusicApp.Views
{
    /// <summary>
    /// Code-behind cua thanh dieu huong Sidebar Navigation (Sidebar Navigation Code-Behind).
    /// 
    /// Tac dung:
    /// - Khoi tao thanh phan giao dien menu doc ben trai ung dung tu SidebarNavigationView.xaml.
    /// - Hien thi logo thuong hieu "Jun Music", danh sach cac muc menu (Kham pha, Nhac Viet, Thu vien, Hang doi, Loi bai hat, EQ),
    ///   va bang thong bao trang thai he thong (BFF Online port 5245 &amp; NAudio Engine).
    /// 
    /// Van de giai quyet:
    /// - Tuan thu nguyen ly MVVM: Khong viet ma xu ly su kien trong code-behind.
    /// - DataContext cua UserControl nay duoc thua huong truc tiep tu MainWindow (MainViewModel).
    /// - Thao tac click chon muc menu duoc binding hai chieu (TwoWay Binding) thong qua SelectedItem="{Binding SelectedNavigationItem, Mode=TwoWay}".
    /// </summary>
    public partial class SidebarNavigationView : UserControl
    {
        /// <summary>
        /// Khoi tao UserControl SidebarNavigationView.
        /// </summary>
        public SidebarNavigationView()
        {
            InitializeComponent();
        }
    }
}
