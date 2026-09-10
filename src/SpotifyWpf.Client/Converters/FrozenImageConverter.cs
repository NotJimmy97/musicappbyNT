using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SpotifyWpf.Client.Converters
{
    public class FrozenImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string url = value.ToString();
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                // Architectural Rule: Freezing the BitmapImage makes it thread-safe, immutable,
                // and releases unmanaged GDI/WIC handles to prevent cumulative WPF memory leaks.
                if (bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                }

                return bitmap;
            }
            catch
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
