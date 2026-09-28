using System.Windows.Controls;
using System.Windows.Input;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
    public partial class PlaylistDetailView : UserControl
    {
        public PlaylistDetailView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PlaylistDetailViewModel vm && vm.SelectedTrack != null)
            {
                vm.SelectedTrack.PlayCommand?.Execute(null);
            }
        }
    }
}
