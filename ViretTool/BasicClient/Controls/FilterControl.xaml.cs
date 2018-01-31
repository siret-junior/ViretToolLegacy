using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ViretTool.BasicClient {
    /// <summary>
    /// Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl : UserControl {
        public FilterControl() {
            InitializeComponent();
        }

        public enum FilterState { Y, N, Off }

        public delegate void FilterChangedHandler(FilterState state, double value);

        public event FilterChangedHandler FilterChangedEvent;


        public static readonly DependencyProperty FilterNameProperty = DependencyProperty.Register("FilterName", typeof(string), typeof(FilterControl), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(FilterControl), new FrameworkPropertyMetadata(0.0d));
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(double), typeof(FilterControl), new FrameworkPropertyMetadata(0.0d));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(FilterState), typeof(FilterControl), new FrameworkPropertyMetadata(FilterState.Off));

        public string FilterName {
            get { return (string)GetValue(FilterNameProperty); }
            set { SetValue(FilterNameProperty, value); }
        }

        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double DefaultValue {
            get { return (double)GetValue(DefaultValueProperty); }
            set {
                SetValue(DefaultValueProperty, value);
                Value = value;
            }
        }

        public FilterState State {
            get { return (FilterState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public void Reset() {
            Value = DefaultValue;
            State = FilterState.Off;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            FilterChangedEvent?.Invoke(State, Value/100d);
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e) {
            if (State == FilterState.Off) {
                State = FilterState.Y;
            }
            FilterChangedEvent?.Invoke(State, Value/100d);
        }
    }
}
