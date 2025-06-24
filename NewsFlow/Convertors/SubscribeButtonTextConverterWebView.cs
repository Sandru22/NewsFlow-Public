using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFlow.Convertors
{
    class SubscribeButtonTextConverterWebView : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasSubscribed)
            {
                return hasSubscribed ? "🔕Dezabonează-te" : "🔔 Abonează-te";
            }
            return "🔔 Abonează-te";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
