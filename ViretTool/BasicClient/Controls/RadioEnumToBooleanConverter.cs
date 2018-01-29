using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ViretTool.BasicClient {
    [ValueConversion(typeof(FilterControl.FilterState), typeof(bool))]
    public sealed class RadioEnumToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string current = value.ToString();
            string button = (string)parameter;

            return button == current;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string button = (string)parameter;

            return Enum.Parse(targetType, button);
        }
    }
}
