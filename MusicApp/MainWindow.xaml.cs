using System.Windows;

namespace MusicApp
{
    /// <summary>
    /// Code-behind cua cua so chinh ung dung (Main Window Shell Code-Behind).
    /// 
    /// Tac dung:
    /// - Nap va khoi tao cay giao dien Visual Tree tu tap tin MainWindow.xaml thong qua InitializeComponent().
    /// - Dong vai tro la khung chua tong the (Shell Host) chua hai khu vuc chinh:
    ///   1. Hang 0 (Work Area): Chia 2 cot gom Sidebar Navigation ben trai (220px) va Dynamic Content Area ben phai.
    ///   2. Hang 1 (Player Bar): The NowPlayingCardView co dinh o goc duoi cung.
    /// 
    /// Van de giai quyet:
    /// - Tuan thu tuyet doi quy tac MVVM nghiem ngat: Lop code-behind nay HOAN TOAN KHONG chua bat ky logic nghiep vu nao
    ///   (Zero Business Logic in Code-Behind). Toan bo viec xu ly su kien, chuyen trang, phat am thanh, hoac go tim kiem
    ///   deu duoc uy thac thong qua DataBinding va ICommand toi MainViewModel gan tren DataContext.
    /// 
    /// Cach thuc van hanh:
    /// - DataContext duoc thiet lap tu lop App.xaml.cs tai thoi diem khoi dong.
    /// - MainWindow.xaml su dung ContentControl kem theo cac DataTrigger dua tren thuoc tinh CurrentViewName
    ///   de tu dong thay doi DataTemplate hien thi View tuong ung.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Khoi tao cua so giao dien chinh MainWindow.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
