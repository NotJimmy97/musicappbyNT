using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MusicApp.Core.Models;

namespace MusicApp.Converters
{
    public class PlaybackStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlaybackState state && parameter is string targetStateStr)
            {
                if (Enum.TryParse(targetStateStr, out PlaybackState targetState))
                {
                    return state == targetState ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
