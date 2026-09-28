using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicApp.Converters
{
    /// <summary>
    /// Bộ chuyển đổi số giây sang chuỗi định dạng (mm:ss hoặc hh:mm:ss).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Định dạng chuỗi hiển thị thời gian cho UI.
    /// 2. Không chịu trách nhiệm: Tính toán thời gian phát thực tế.
    /// 3. Vòng đời: Đối tượng tĩnh không trạng thái.
    /// 4. Đa luồng: Chỉ thực thi trên UI thread.
    /// </remarks>
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        /// <summary>
        /// Chuyen doi so giay thanh chuoi dinh dang thoi gian (mm:ss hoac hh:mm:ss).
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds = 0;
            if (value is double d) seconds = d;
            else if (value is int i) seconds = i;
            else if (value is float f) seconds = f;

            var time = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Khong ho tro chuyen doi nguoc tu chuoi gio phut ve so giay.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
