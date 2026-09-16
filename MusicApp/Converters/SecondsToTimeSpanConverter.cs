using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicApp.Converters
{
    /// <summary>
    /// Bo chuyen doi thoi gian tinh bang giay sang chuoi dinh dang gio phut giay (Seconds to TimeSpan String Converter).
    /// 
    /// Tac dung:
    /// - Chuyen doi gia tri thoi luong (double, int, float) tinh theo tong so giay thanh chuoi van ban hien thi tren giao dien.
    /// - Dinh dang dau ra: "mm:ss" neu thoi luong duoi 1 gio, hoac "hh:mm:ss" neu thoi luong tu 1 gio tro len.
    /// 
    /// Van de giai quyet:
    /// - Giup nguoi dung doc hieu thoi gian bai hat mot cach truc quan tren thanh tien do (Timeline Slider)
    ///   va trong danh sach hang doi/thu vien offline.
    /// - Xu ly an toan cac gia tri am hoac null, luon dam bao tra ve dinh dang chuan ma khong bi loi runtime.
    /// 
    /// Cach thuc van hanh:
    /// - Doc so giay tu gia tri value, chuyen ve TimeSpan bang TimeSpan.FromSeconds(Math.Max(0, seconds)).
    /// - Kiem tra TotalHours de quyet dinh mau format chuoi phu hop.
    /// </summary>
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
