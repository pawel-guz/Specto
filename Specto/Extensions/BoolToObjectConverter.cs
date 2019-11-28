using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Specto
{
    public class BoolToObjectConverter : IValueConverter
    {
        public object TrueObject { get; set; }
        public object FalseObject { get; set; }
        public object NullObject { get; set; }

        public BoolToObjectConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return NullObject;
            bool boolValue = true;
            bool isBool = true;
            try
            {
                boolValue = (bool)value;
            }
            catch
            {
                isBool = false;
            }

            if (!isBool) return NullObject;
            return boolValue ? TrueObject : FalseObject;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
