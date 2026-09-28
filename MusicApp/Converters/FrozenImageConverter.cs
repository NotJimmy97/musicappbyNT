using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusicApp.Converters
{
    /// <summary>
    /// Bộ chuyển đổi hình ảnh WPF chống rò rỉ bộ nhớ.
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Chuyển đổi đường dẫn, base64 thành BitmapImage an toàn.
    /// 2. Không chịu trách nhiệm: Quản lý lưu trữ/cache file vật lý.
    /// 3. Vòng đời: Đối tượng tĩnh không trạng thái. Gọi Freeze() để biến BitmapImage thành đối tượng bất biến.
    /// 4. Đa luồng: Nhờ hàm Freeze(), ảnh có thể chia sẻ đa luồng an toàn.
    /// </remarks>
    public class FrozenImageConverter : IValueConverter
    {
        /// <summary>
        /// Chuyen doi du lieu nguon thanh BitmapImage da duoc dong bang an toan bo nho.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is System.Windows.Media.ImageSource imageSource)
            {
                return imageSource;
            }

            byte[] imageBytes = null;

            if (value is byte[] rawBytes && rawBytes.Length > 0)
            {
                imageBytes = rawBytes;
            }
            else if (value is string inputStr && !string.IsNullOrWhiteSpace(inputStr))
            {
                try
                {
                    // 1. Xu ly chuoi Base64 Data URI (thuong gap khi doc tu the ID3 cua file cuc bo)
                    if (inputStr.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                    {
                        int commaIdx = inputStr.IndexOf(',');
                        if (commaIdx >= 0 && commaIdx < inputStr.Length - 1)
                        {
                            string base64 = inputStr.Substring(commaIdx + 1);
                            imageBytes = System.Convert.FromBase64String(base64);
                        }
                    }
                    // 2. Xu ly duong dan tap tin tren o dia
                    else if (File.Exists(inputStr))
                    {
                        imageBytes = File.ReadAllBytes(inputStr);
                    }
                    // 3. Xu ly dia chi URL web tu xa
                    else if (inputStr.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             inputStr.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var webClient = new WebClient())
                        {
                            imageBytes = webClient.DownloadData(inputStr);
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            try
            {
                using (var memoryStream = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    // Bat buoc goi Freeze(): bien bitmap thanh doi tuong bat bien chong ro ri bo nho WPF
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Khong ho tro chuyen doi nguoc tu Image ve chuoi du lieu goc.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
