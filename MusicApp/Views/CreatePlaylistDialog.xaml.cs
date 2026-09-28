using System.Windows;

namespace MusicApp.Views
{
    public partial class CreatePlaylistDialog : Window
    {
        public string PlaylistName => TxtName.Text?.Trim();
        public string PlaylistDescription => TxtDescription.Text?.Trim();

        public CreatePlaylistDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => TxtName.Focus();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlaylistName))
            {
                MessageBox.Show("Vui lòng nhập tên danh sách phát.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
