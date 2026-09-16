using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Lop co so truu tuong trien khai giao dien INotifyPropertyChanged cho toan bo cac ViewModel trong he thong MVVM.
    /// 
    /// Tac dung:
    /// - Cung cap co che thong bao bien dong du lieu tu tang logic ViewModel len tang giao dien WPF XAML.
    /// 
    /// Van de giai quyet:
    /// - Dong bo hoa tu dong hai chieu (TwoWay DataBinding) giua ViewModel va View.
    /// - Triet tieu cac thong bao PropertyChanged du thua khi gia tri thuoc tinh khong thay doi, giup tiet kiem chu ky CPU cua luong UI.
    /// 
    /// Cach thuc van hanh:
    /// - Su dung [CallerMemberName] de tu dong xac dinh ten thuoc tinh ma khong can truyen chuoi thu cong.
    /// - Phuong thuc SetProperty so sanh gia tri hien tai va gia tri moi bang EqualityComparer; chi khi gia tri thuc su thay doi moi cap nhat bo nho dem va phat su kien.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Kiem tra va cap nhat gia tri thuoc tinh, ngan chan phat sinh su kien du thua neu gia tri khong doi.
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
