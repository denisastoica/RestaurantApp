using System;
using System.Globalization;
using System.Windows.Data;

namespace Restaurant.Services
{
    public class StatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            if (string.IsNullOrEmpty(status))
                return false;

            status = status.Trim().ToLower();
            return status == "inregistrata" || status == "se pregateste" || status == "a plecat la client";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
