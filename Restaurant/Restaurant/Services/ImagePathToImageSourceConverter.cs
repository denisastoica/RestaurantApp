using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Restaurant.Services
{
    public class ImagePathToImageSourceConverter : IValueConverter
    {
        private const string DefaultImagePath = "images/default_image.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relativePath = value as string;

            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = DefaultImagePath;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDir, relativePath.TrimStart('\\', '/'));

                if (!File.Exists(fullPath))
                {
                    fullPath = Path.Combine(baseDir, DefaultImagePath);
                    if (!File.Exists(fullPath))
                    {
                        return null;
                    }
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
