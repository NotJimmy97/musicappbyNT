using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpotifyWpf.Core.Common
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            // Suppress redundant PropertyChanged notifications to prevent unnecessary WPF layout/render passes
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Null-conditional invoke maintains thread-safe notification dispatch to event subscribers
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

