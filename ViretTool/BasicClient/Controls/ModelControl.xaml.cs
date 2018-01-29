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
    /// Interaction logic for ModelControl.xaml
    /// </summary>
    public partial class ModelControl : UserControl {
        public ModelControl() {
            InitializeComponent();
        }

        public delegate void ModelSettingChangedHandler(double value, bool useForSorting);
        public event ModelSettingChangedHandler ModelSettingChangedEvent;

        public delegate void ModelClearedHandler();
        public event ModelClearedHandler ModelClearedEvent;

        public static readonly DependencyProperty ModelNameProperty = DependencyProperty.Register("ModelName", typeof(string), typeof(ModelControl), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty UseForSortingProperty = DependencyProperty.Register("UseForSorting", typeof(bool), typeof(ModelControl), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(ModelControl), new FrameworkPropertyMetadata(0.0d));
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(double), typeof(ModelControl), new FrameworkPropertyMetadata(0.0d));

        public string ModelName {
            get { return (string)GetValue(ModelNameProperty); }
            set { SetValue(ModelNameProperty, value); }
        }

        public bool UseForSorting {
            get { return (bool)GetValue(UseForSortingProperty); }
            set { SetValue(UseForSortingProperty, value); }
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

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e) {
            ModelSettingChangedEvent?.Invoke(Value, UseForSorting);
        }

        public void Clear() {
            Value = DefaultValue;
            if (UseForSorting) {
                UseForSorting = false;
            } else {
                ModelSettingChangedEvent?.Invoke(Value, UseForSorting);
            }
            ModelClearedEvent?.Invoke();
        }

        private void Clear(object sender, RoutedEventArgs e) {
            Clear();
        }

        public void CheckMe() {
            if (!UseForSorting) {
                UseForSorting = true;
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e) {
            ModelSettingChangedEvent?.Invoke(Value, UseForSorting);
        }
    }
}
