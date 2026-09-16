using System;
using System.Windows.Input;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Trien khai mau ICommand tieu chuan cho kien truc MVVM trong WPF.
    /// 
    /// Tac dung:
    /// - Dong goi cac hanh vi thuc thi (Action) va dieu kien cho phep thuc thi (Predicate) thanh doi tuong Command co the gan ket (Bind) vao XAML.
    /// 
    /// Van de giai quyet:
    /// - Tach roi hoan toan logic xu ly su kien khoi Code-Behind (.xaml.cs), dam bao tinh don trach nhiem cua ViewModel va de dang kiem thu don vi.
    /// 
    /// Cach thuc van hanh:
    /// - Uy quyen lenh qua delegate Action<object> va kiem tra trang thai qua Predicate<object>.
    /// - Mok ket noi CanExecuteChanged truc tiep voi CommandManager.RequerySuggested cua WPF de tu dong danh gia lai trang thai bat/tat (Enabled/Disabled) cua nut bam tren giao dien nguoi dung.
    /// </summary>
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
