namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel dai dien cho mot muc dieu huong tren thanh Sidebar Navigation (Sidebar Navigation Item).
    /// 
    /// Tac dung:
    /// - Luu tru thong tin tieu de (Title), bieu tuong Segoe MDL2 Assets (IconSymbol),
    ///   khoa phan biet View (ViewKey: "Explore", "Queue", "LocalLibrary", "Lyrics", "Equalizer"), va nhom phan loai (Category).
    /// - Duoc su dung trong danh sach ObservableCollection&lt;NavigationItemViewModel&gt; tren SidebarNavigationView.
    /// 
    /// Van de giai quyet:
    /// - Cho phep cau hinh dong danh muc dieu huong ma khong can hardcode cac nut bam vao file XAML.
    /// - Khi nguoi dung click vao mot muc, MainViewModel lang nghe su kien SelectionChanged va thay doi
    ///   CurrentContentView phu hop theo mau ViewModel-First Navigation.
    /// 
    /// Cach thuc van hanh:
    /// - Binding truc tiep vao ListBoxItem / RadioButton trong SidebarNavigationView.xaml.
    /// </summary>
    public class NavigationItemViewModel
    {
        /// <summary>
        /// Tieu de hien thi cua muc dieu huong (vi du: "Kham pha", "Hang doi", "Thu vien",...).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Khoa dinh danh View de MainViewModel quyet dinh chuyen doi ContentControl.
        /// </summary>
        public string ViewKey { get; set; }

        /// <summary>
        /// Ma ky tu bieu tuong phông Segoe MDL2 Assets hoac chuoi bieu tuong text.
        /// </summary>
        public string IconSymbol { get; set; }

        /// <summary>
        /// Danh muc phan nhom tren menu (vi du: "MENU", "THU VIEN").
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Tra ve tieu de de ho tro debug va hien thi co ban.
        /// </summary>
        public override string ToString()
        {
            return Title;
        }
    }
}
