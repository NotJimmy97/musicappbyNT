using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicApp.Converters
{
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds = 0;
            if (value is double d) seconds = d;
            else if (value is int i) seconds = i;
            else if (value is float f) seconds = f;

            var time = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

