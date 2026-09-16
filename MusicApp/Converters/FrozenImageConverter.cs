using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusicApp.Converters
{
    /// <summary>
    /// Bo chuyen doi hinh anh chong ro ri bo nho WPF (Memory-Leak Safe Image Converter).
    /// 
    /// Tac dung:
    /// - Chuyen doi da dang cac dinh dang anh dau vao thanh doi tuong BitmapImage hien thi tren Image Control cua WPF.
    /// - Ho tro: Chuoi Base64 Data URI (data:image/jpeg;base64,...), duong dan file o cung cuc bo, URL HTTP/HTTPS, va mang byte[].
    /// - Goi phuong thuc bitmap.Freeze() de dong bang trang thai doi tuong hinh anh.
    /// 
    /// Van de giai quyet:
    /// - Phong ngua ro ri bo nho unmanaged nghiem trong trong WPF: Cac doi tuong BitmapSource mac dinh luu tru tham chieu
    ///   toi luong UI va bo nho unmanaged. Khi danh sach bai hat cuon lien tuc va nap hang tram anh bia, viec khong goi Freeze()
    ///   se khien bo nho RAM tang vot (Out of Memory - OOM) do Garbage Collector khong the tu do thu hoi.
    /// -bitmap.Freeze() bien BitmapImage thanh doi tuong bat bien (Immutable) va cho phep chia se da luong (Cross-thread accessible),
    ///   giup giao dien render muot ma ma khong ton hao bo nho.
    /// 
    /// Cach thuc van hanh:
    /// - Nhan gia tri value tu DataBinding XAML.
    /// - Phan loai kieu du lieu: neu la base64 thi giai ma ve byte[], neu la file thi doc truc tiep, neu la URL thi download.
    /// - Khoi tao BitmapImage voi BitmapCacheOption.OnLoad va goi Freeze() truoc khi tra ve cho WPF Visual Tree.
    /// </summary>
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
