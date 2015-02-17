using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using System.ComponentModel;
namespace CorySynthUI.Converters
{
    public class ByteTypeConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var orig = (byte)value;
            return System.Convert.ToDouble(orig);
                
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var orig = (double)value;
            return System.Convert.ToByte(orig);
        }
    }
}
