using System;
using System.Windows.Input;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Triển khai mẫu ICommand tiêu chuẩn cho kiến trúc MVVM.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Đóng gói hành vi thực thi đồng bộ và điều kiện vào Command.
    /// KHÔNG chịu trách nhiệm: Chạy các tác vụ bất đồng bộ (dùng AsyncRelayCommand).
    /// Vòng đời: Tồn tại cùng vòng đời của ViewModel.
    /// Luồng: Thường liên kết với UI thread qua CommandManager.RequerySuggested.
    /// </remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Ket noi vao he thong dieu huong Command cua WPF de tu dong danh gia lai dieu kien thuc thi khi UI thay doi trang thai.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Yeu cau he thong WPF danh gia lai ngay lap tuc trang thai CanExecute cua lenh.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
