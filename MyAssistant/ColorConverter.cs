using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace MyAssistant
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string newColor = "";

            Color c = (Color)System.Windows.Media.ColorConverter.ConvertFromString("SkyBlue");

            var color = value.ToString();
            if (color == c.ToString())
            {
                newColor = "Orange";
            }
            return newColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
