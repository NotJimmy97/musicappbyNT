using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicApp.Core.Common
{
    /// <summary>
    /// Lớp cơ sở trừu tượng triển khai INotifyPropertyChanged cho các ViewModel.
    /// </summary>
    /// <remarks>
    /// Chịu trách nhiệm: Cung cấp cơ chế thông báo biến động dữ liệu lên giao diện WPF XAML.
    /// KHÔNG chịu trách nhiệm: Thực thi logic nghiệp vụ.
    /// Vòng đời: Kế thừa bởi ViewModel, tồn tại cùng với vòng đời của View.
    /// Luồng: Việc kích hoạt sự kiện nên được thực hiện hoặc đồng bộ về UI thread (Dispatcher).
    /// </remarks>
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
