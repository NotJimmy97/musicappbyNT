namespace MusicApp.ViewModels
{
    /// <summary>
    /// ViewModel đại diện cho một mục điều hướng trên thanh Sidebar.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Lưu trữ Title, ViewKey, IconSymbol, Category để binding lên UI.
    /// 2. Không chịu trách nhiệm: Quản lý logic điều hướng thực sự (thuộc MainViewModel).
    /// 3. Vòng đời: Khởi tạo một lần tại MainViewModel và tồn tại cùng ứng dụng.
    /// 4. Đa luồng: Hoạt động thuần trên UI thread.
    /// </remarks>
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
