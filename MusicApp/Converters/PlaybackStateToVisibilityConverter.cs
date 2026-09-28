using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MusicApp.Core.Models;

namespace MusicApp.Converters
{
    /// <summary>
    /// Bộ chuyển đổi trạng thái phát nhạc sang hiển thị Visibility trong XAML.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: So sánh PlaybackState với Parameter và trả về Visible/Collapsed.
    /// 2. Không chịu trách nhiệm: Điều khiển AudioEngine.
    /// 3. Vòng đời: Đối tượng tĩnh không trạng thái.
    /// 4. Đa luồng: Chỉ thực thi trên UI thread.
    /// </remarks>
    public class PlaybackStateToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Chuyen doi PlaybackState sang Visibility dua tren ConverterParameter.
        /// </summary>
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

        /// <summary>
        /// Khong ho tro chuyen doi nguoc tu Visibility ve trang thai phat nhac.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
