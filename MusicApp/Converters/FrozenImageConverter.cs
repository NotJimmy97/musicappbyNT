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
                    if (inputStr.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                    {
                        int commaIdx = inputStr.IndexOf(',');
                        if (commaIdx >= 0 && commaIdx < inputStr.Length - 1)
                        {
                            string base64 = inputStr.Substring(commaIdx + 1);
                            imageBytes = System.Convert.FromBase64String(base64);
                        }
                    }
                    else if (File.Exists(inputStr))
                    {
                        imageBytes = File.ReadAllBytes(inputStr);
                    }
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

                    // Mandatory Freeze() call: prevents unmanaged WPF bitmap memory leaks by making it immutable and thread-safe
                    bitmap.Freeze();
                    return bitmap;
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

