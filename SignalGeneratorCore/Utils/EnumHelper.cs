using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorySignalGenerator.Utils
{
    /// <summary>
    /// From http://www.codeproject.com/Tips/584206/Enum-to-ComboBox-binding
    /// </summary>
    public class EnumHelper : DependencyObject
    {
        public static String GetEnum(DependencyObject obj)
        {
            return (String)obj.GetValue(EnumProperty);
        }

        public static void SetEnum(DependencyObject obj, String value)
        {
            obj.SetValue(EnumProperty, value);
        }

        // Using a DependencyProperty as the backing store for Enum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnumProperty =
            DependencyProperty.RegisterAttached("Enum", typeof(String), typeof(EnumHelper), new PropertyMetadata(null, OnEnumChanged));

        private static void OnEnumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ItemsControl;

            if (control != null)
            {
                if (e.NewValue != null)
                {
                    var type = Type.GetType((string)e.NewValue);
                    if (type != null)
                    {
                        var _enum = Enum.GetValues(type as Type);
                        control.ItemsSource = _enum;
                    }
                }
            }
        }

       
    }
}
