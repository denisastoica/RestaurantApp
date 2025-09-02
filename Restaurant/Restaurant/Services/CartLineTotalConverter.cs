using System;
using System.Globalization;
using System.Windows.Data;
using Restaurant.ViewModels;

namespace Restaurant.Services
{
    public class CartLineTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CartItemViewModel item)
            {
                return $"{item.Subtotal:F2} lei";
            }
            return "0.00 lei";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
