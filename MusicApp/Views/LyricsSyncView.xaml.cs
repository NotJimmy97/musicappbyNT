using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MusicApp.ViewModels;

namespace MusicApp.Views
{
    /// <summary>
    /// Code-behind cho UserControl LyricsSyncView - Giao dien dong bo loi bai hat thoi gian thuc (Karaoke/Synced Lyrics).
    /// 
    /// - Tac dung: Dieu khien visual tree de tu dong cuon muot ma (auto-scroll) dong loi bai hat dang phat
    ///   vao chinh giua man hinh (vertical center alignment) theo nhip thoi gian cua Audio Engine.
    /// 
    /// - Van de giai quyet:
    ///   1. Tinh toan toa do hinh hoc tren Visual Tree: Trong WPF, viec can giua mot Item trong ListBox vao tam
    ///      ScrollViewer khong the bieu dien thuan tuy qua XAML Binding ma can ma tran bien doi khong gian
    ///      (TransformToAncestor) de xac dinh vi tri vat ly chinh xac.
    ///   2. Quan ly Memory Leak phong thu: Dang ky va huy dang ky su kien ActiveLineChanged chat che theo
    ///      vong doi DataContextChanged va Unloaded, dam bao khong giu tham chieu song toi ViewModel khi view bi huy.
    ///   3. Tranh giat lag giao dien (UI Stuttering): Su dung DispatcherPriority.Background de thuc thi thao tac
    ///      cuon sau khi he thong layout da hoan tat do dac kich thuoc cua phan tu.
    /// 
    /// - Cach thuc van hanh:
    ///   1. Lang nghe DataContextChanged de lien ket su kien ActiveLineChanged tu LyricsViewModel.
    ///   2. Khi chi muc dong dang phat (activeIndex) thay doi, day tac vu cuon vao Dispatcher voi muc uu tien Background.
    ///   3. Lay Container tuong ung cua dong loi thong qua ItemContainerGenerator.
    ///   4. Tinh toan targetOffset = currentOffset + itemY - (viewerHeight / 2) + (itemHeight / 2).
    ///   5. Cuon ScrollViewer den targetOffset sau khi da kep gia tri (clamp) an toan trong pham vi [0, ScrollableHeight].
    /// </summary>
    public partial class LyricsSyncView : UserControl
    {
        private LyricsViewModel _subscribedViewModel;

        /// <summary>
        /// Khoi tao mot instance moi cua LyricsSyncView va dang ky cac hook quan ly vong doi DataContext va Unload.
        /// </summary>
        public LyricsSyncView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// Xu ly su kien thay doi DataContext cua View.
        /// Huy dang ky su kien tu ViewModel cu (neu co) va dang ky lang nghe tu ViewModel moi de tranh ro ri bo nho.
        /// </summary>
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

        /// <summary>
        /// Xu ly su kien khi View bi go khoi Visual Tree WPF.
        /// Dam bao huy dang ky su kien ActiveLineChanged triet de, cho phep Garbage Collector thu hoi ViewModel.
        /// </summary>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.ActiveLineChanged -= OnActiveLineChanged;
                _subscribedViewModel = null;
            }
        }

        /// <summary>
        /// Xu ly dong bo cuon dong loi bai hat dang active vao chinh giua tam man hinh cua ScrollViewer.
        /// </summary>
        /// <param name="sender">Nguon phat su kien (LyricsViewModel).</param>
        /// <param name="activeIndex">Chi so dong loi bai hat dang duoc phat.</param>
        private void OnActiveLineChanged(object sender, int activeIndex)
        {
            if (activeIndex < 0 || LyricsListBox == null || LyricsScrollViewer == null)
            {
                return;
            }

            // Day tac vu tinh toan xuong Dispatcher muc Background de doi Visual Tree hoan tat qua trinh Layout Pass
            Dispatcher.InvokeAsync(() =>
            {
                if (activeIndex >= LyricsListBox.Items.Count) return;

                var item = LyricsListBox.Items[activeIndex];
                LyricsListBox.ScrollIntoView(item);

                // Tinh toan toa do hinh hoc de can giua phan tu theo chieu doc
                var container = LyricsListBox.ItemContainerGenerator.ContainerFromIndex(activeIndex) as FrameworkElement;
                if (container != null && LyricsScrollViewer.ActualHeight > 0)
                {
                    try
                    {
                        var transform = container.TransformToAncestor(LyricsScrollViewer);
                        var position = transform.Transform(new Point(0, 0));
                        double currentOffset = LyricsScrollViewer.VerticalOffset;
                        double targetOffset = currentOffset + position.Y - (LyricsScrollViewer.ActualHeight / 2.0) + (container.ActualHeight / 2.0);

                        // Kiem tra va kep gioi han cuon an toan
                        if (targetOffset < 0) targetOffset = 0;
                        if (targetOffset > LyricsScrollViewer.ScrollableHeight) targetOffset = LyricsScrollViewer.ScrollableHeight;

                        LyricsScrollViewer.ScrollToVerticalOffset(targetOffset);
                    }
                    catch (Exception)
                    {
                        // Bo qua ngoai le an toan neu Visual Tree dang trong giai doan sap xep bo cuc
                    }
                }
            }, DispatcherPriority.Background);
        }
    }
}


