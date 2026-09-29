using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Lớp lệnh bất đồng bộ (Asynchronous Command) dành cho mô hình MVVM.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Đóng gói các tác vụ bất đồng bộ (Task) vào giao diện ICommand.
    /// KHÔNG chịu trách nhiệm: Quản lý vòng đời luồng độc lập.
    /// Vòng đời: Scoped hoặc Transient gắn với ViewModel.
    /// Luồng: Đảm bảo tác vụ không chặn UI thread, chặn double click/re-entrancy an toàn.
    /// </remarks>
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

            var syncContext = SynchronizationContext.Current;
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeAsync(parameter).ConfigureAwait(false);
            }
            finally
            {
                _isExecuting = false;
                if (syncContext != null)
                {
                    syncContext.Post(_ => RaiseCanExecuteChanged(), null);
                }
                else
                {
                    RaiseCanExecuteChanged();
                }
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
