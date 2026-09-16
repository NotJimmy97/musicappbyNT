using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Lop lenh bat dong bo (Asynchronous Command) danh cho mo hinh MVVM trong WPF.
    /// 
    /// Tac dung:
    /// - Dong goi cac tac vu bat dong bo (Func&lt;object, Task&gt;) vao giao dien ICommand tieu chuan cua WPF.
    /// - Cho phep DataBinding truc tiep tu Button hoac CommandBinding trong XAML den cac phuong thuc async trong ViewModel.
    /// 
    /// Van de giai quyet:
    /// - Giao dien ICommand mac dinh chi ho tro thuc thi dong bo (void Execute), de dan den hien tuong khoa UI (UI Freeze)
    ///   neu goi tac vu I/O nang, hoac gay loi khong the bat duoc Exception (Unobserved Task Exception) khi dung async void tuy tien.
    /// - Ngan ngua tinh trang nguoi dung nhan nut lien tiep (Re-entrancy / Double Click) gay xung dot luong hoac goi API trung lap.
    /// 
    /// Cach thuc van hanh:
    /// - Quan ly bien co _isExecuting. Khi tac vu dang chay, CanExecute tu dong tra ve false de vo hieu hoa control tren UI.
    /// - Su dung khoi try-finally de dam bao trang thai thuc thi luon duoc khoi phuc ve false ngay ca khi xay ra ngoai le.
    /// - Tich hop CommandManager.RequerySuggested de tu dong thong bao cho WPF cap nhat trang thai Enabled/Disabled cua nut bam.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Khoi tao mot doi tuong AsyncRelayCommand moi.
        /// </summary>
        /// <param name="executeAsync">Delegate thuc thi bat dong bo nhan tham so va tra ve Task.</param>
        /// <param name="canExecute">Dieu kien tien quyet xac dinh lenh co the thuc thi hay khong.</param>
        public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Kiem tra xem lenh co the duoc thuc thi tai thoi diem hien tai hay khong.
        /// Tra ve false neu tac vu truoc do van dang trong tien trinh xu ly (_isExecuting = true).
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        /// <summary>
        /// Thuc thi tac vu bat dong bo duoc dong goi.
        /// Tu dong kich hoat cap nhat CanExecute truoc va sau khi hoan tat tac vu.
        /// </summary>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeAsync(parameter).ConfigureAwait(false);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Su kien thong bao khi trang thai kha thi cua lenh thay doi.
        /// Duoc cau hinh lien ket voi co che RequerySuggested cua WPF CommandManager.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Yeu cau he thong WPF danh gia lai trang thai CanExecute cho tat ca cac thanh phan lien ket lenh.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
