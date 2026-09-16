using System.Windows.Controls;

namespace MusicApp.Views
{
    /// <summary>
    /// Code-behind cho UserControl PlayQueueView - Giao dien quan ly hang doi phat nhac.
    /// 
    /// - Tac dung: Khoi tao va quan ly vong doi cua UserControl PlayQueueView tren Visual Tree WPF.
    ///   Hien thi danh sach cac bai hat dang cho phat, bai hat dang phat hien tai va ho tro tuong tac
    ///   keo-tha doi thu tu bai hat.
    /// 
    /// - Van de giai quyet: Tuan thu triet de mau thiet ke MVVM (Model-View-ViewModel). Code-behind
    ///   duoc giu tinh gon toi da (Zero-Code-Behind pattern), khong chua bat ky logic nghiep vu nao
    ///   ve hang doi hay phat nhac. Toan bo thao tac keo-tha (drag-and-drop) duoc thuc hien thong qua
    ///   GongSolutions.Wpf.DragDrop gan truc tiep tai tang XAML vao PlayQueueViewModel.
    /// 
    /// - Cach thuc van hanh:
    ///   1. Khoi tao thanh phan do hoa thong qua phuong thuc InitializeComponent() do WPF sinh ra.
    ///   2. Nhap DataContext tu MainViewModel (PlayQueueViewModel).
    ///   3. Cac tuong tac nhan dup chuot (double-click de phat), xoa bai hat khoi hang doi, hoac
    ///      keo tha sap xep deu duoc binding truc tiep qua ICommand va IDropTarget trong ViewModel.
    /// </summary>
    public partial class PlayQueueView : UserControl
    {
        /// <summary>
        /// Khoi tao mot instance moi cua PlayQueueView va nap cay phan tu XAML.
        /// </summary>
        public PlayQueueView()
        {
            InitializeComponent();
        }
    }
}


