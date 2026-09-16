using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
    public partial class LyricsSyncView : UserControl
    {
        private LyricsViewModel _subscribedViewModel;

        public LyricsSyncView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.ActiveLineChanged -= OnActiveLineChanged;
                _subscribedViewModel = null;
            }

            if (e.NewValue is LyricsViewModel vm)
            {
                _subscribedViewModel = vm;
                _subscribedViewModel.ActiveLineChanged += OnActiveLineChanged;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.ActiveLineChanged -= OnActiveLineChanged;
                _subscribedViewModel = null;
            }
        }

        private void OnActiveLineChanged(object sender, int activeIndex)
        {
            if (activeIndex < 0 || LyricsListBox == null || LyricsScrollViewer == null)
            {
                return;
            }

            Dispatcher.InvokeAsync(() =>
            {
                if (activeIndex >= LyricsListBox.Items.Count) return;

                var item = LyricsListBox.Items[activeIndex];
                LyricsListBox.ScrollIntoView(item);

                // Calculate vertical center alignment
                var container = LyricsListBox.ItemContainerGenerator.ContainerFromIndex(activeIndex) as FrameworkElement;
                if (container != null && LyricsScrollViewer.ActualHeight > 0)
                {
                    try
                    {
                        var transform = container.TransformToAncestor(LyricsScrollViewer);
                        var position = transform.Transform(new Point(0, 0));
                        double currentOffset = LyricsScrollViewer.VerticalOffset;
                        double targetOffset = currentOffset + position.Y - (LyricsScrollViewer.ActualHeight / 2.0) + (container.ActualHeight / 2.0);

                        if (targetOffset < 0) targetOffset = 0;
                        if (targetOffset > LyricsScrollViewer.ScrollableHeight) targetOffset = LyricsScrollViewer.ScrollableHeight;

                        LyricsScrollViewer.ScrollToVerticalOffset(targetOffset);
                    }
                    catch (Exception)
                    {
                        // Fallback if visual tree is during layout pass
                    }
                }
            }, DispatcherPriority.Background);
        }
    }
}

