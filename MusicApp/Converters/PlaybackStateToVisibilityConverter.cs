using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MusicApp.Core.Models;

namespace MusicApp.Converters
{
    /// <summary>
    /// Bo chuyen doi trang thai phat nhac sang do hien thi Visibility trong XAML (Playback State To Visibility Converter).
    /// 
    /// Tac dung:
    /// - So sanh trang thai PlaybackState hien tai cua he thong voi tham so ConverterParameter duoc khai bao trong XAML.
    /// - Tra ve Visibility.Visible neu trang thai trung khop, nguoc lai tra ve Visibility.Collapsed.
    /// 
    /// Van de giai quyet:
    /// - Don gian hoa logic hien thi giao dien: Cho phep hien/an cac bieu tuong Play, Pause hoac con quay Buffering
    ///   hoan toan thong qua DataBinding trong XAML ma khong can viet logic if/else trong ViewModel hay code-behind.
    /// 
    /// Cach thuc van hanh:
    /// - Doc gia tri enum PlaybackState tu value va chuoi so sanh tu parameter (vi du: ConverterParameter='Playing').
    /// - Su dung Enum.TryParse de kiem tra hop le va so sanh gia tri.
    /// </summary>
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
