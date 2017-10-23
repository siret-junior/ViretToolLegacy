using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViretTool.RankingModels;

namespace ViretTool.BasicClient.Controls {
    class ModelSelector : WrapPanel {
        static ModelSelector() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModelSelector), new FrameworkPropertyMetadata(typeof(ModelSelector)));
        }

        public ModelSelector() {
            Loaded += ModelSelector_Loaded;
        }

        private Dictionary<IRankingModel, CheckBox> mCheckBoxes = new Dictionary<IRankingModel, CheckBox>();

        public event Action ModelSelectionChangedEvent;
        public static readonly DependencyProperty ModelsProperty = DependencyProperty.Register("Models", typeof(IEnumerable<IRankingModel>), typeof(ModelSelector), new FrameworkPropertyMetadata(null));


        public IEnumerable<IRankingModel> Models {
            get { return (IEnumerable<IRankingModel>)GetValue(ModelsProperty); }
            set { SetValue(ModelsProperty, value); }
        }

        public IEnumerable<KeyValuePair<IRankingModel, bool>> ModelSelection {
            get {
                foreach (var item in mCheckBoxes) {
                    yield return new KeyValuePair<IRankingModel, bool>(item.Key, (bool)item.Value.IsChecked);
                }
            }
        }

        private void ModelSelector_Loaded(object sender, RoutedEventArgs e) {
            foreach (var item in Models) {
                CheckBox c = new CheckBox();
                c.Tag = item;
                c.Content = item.Name;
                c.Margin = new Thickness(0, 0, 10, 0);
                c.Click += CheckBox_Click;
                Children.Add(c);
                mCheckBoxes.Add(item, c);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            ModelSelectionChangedEvent?.Invoke();
        }

        public void Select(IRankingModel model) {
            if (IsLoaded) mCheckBoxes[model].IsChecked = true;
        }

    }
}
