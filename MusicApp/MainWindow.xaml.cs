using System.Windows;

namespace MusicApp
{
    /// <summary>
    /// Lớp code-behind của cửa sổ chính ứng dụng.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Gọi InitializeComponent để nạp Visual Tree.
    /// 2. Không chịu trách nhiệm: Thực thi logic nghiệp vụ (thuộc tính MVVM). Không chứa logic nào trong code-behind.
    /// 3. Vòng đời: Khởi tạo bởi App.xaml và tồn tại đến khi đóng ứng dụng.
    /// 4. Đa luồng: Chỉ thao tác trên UI thread.
    /// </remarks>
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
