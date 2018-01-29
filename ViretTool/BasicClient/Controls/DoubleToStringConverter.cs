using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ViretTool.BasicClient {

    [ValueConversion(typeof(double), typeof(string))]
    public sealed class DoubleToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double doubleType = ((double)value)*100;
            return doubleType.ToString("0.0") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}