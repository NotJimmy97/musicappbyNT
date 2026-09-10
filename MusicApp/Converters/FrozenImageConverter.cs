using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusicApp.Converters
{
    public class FrozenImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string url) || string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                using (var webClient = new WebClient())
                {
                    byte[] imageBytes = webClient.DownloadData(url);
                    using (var memoryStream = new MemoryStream(imageBytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();

                        // Mandatory Freeze() call: prevents unmanaged WPF bitmap memory leaks by making it immutable and thread-safe
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

