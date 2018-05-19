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
using ViretTool.BasicClient.Displays;
using ViretTool.DataModel;

namespace ViretTool.BasicClient {
    /// <summary>
    /// Interaction logic for SingleTimeFrame.xaml
    /// </summary>
    public partial class SingleTimeFrame : UserControl {

        public enum Position { Left, Right };

        DisplayFrame DisplayedFrame;
        TextBlock Text;
        Position SplitterPosition;
        ColumnDefinition Splitter;
        List<Line> Lines = new List<Line>();

        public SingleTimeFrame(IDisplayControl disp, Position pos) {
            InitializeComponent();
            SplitterPosition = pos;
            DisplayedFrame = new DisplayFrame(disp);

            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(1, GridUnitType.Star);
            Splitter = new ColumnDefinition();
            Splitter.Width = new GridLength(13, GridUnitType.Pixel);
            if (pos == Position.Left) {
                contentHolder.ColumnDefinitions.Add(c1);
                contentHolder.ColumnDefinitions.Add(Splitter);
            } else {
                contentHolder.ColumnDefinitions.Add(Splitter);
                contentHolder.ColumnDefinitions.Add(c1);
            }

            Text = new TextBlock();
            Text.VerticalAlignment = VerticalAlignment.Center;
            Text.Background = Brushes.White;
            Text.FontSize = 8;
            Text.HorizontalAlignment = HorizontalAlignment.Center;
            Panel.SetZIndex(Text, 2);

            contentHolder.Children.Add(Text);
            Grid.SetColumn(Text, pos == Position.Left ? 1 : 0);

            contentHolder.Children.Add(DisplayedFrame);
            Grid.SetColumn(DisplayedFrame, pos == Position.Left ? 0 : 1);
        }

        internal void Clear() {
            DisplayedFrame.Clear();
            foreach (var item in Lines) {
                contentHolder.Children.Remove(item);
            }
            Lines.Clear();
            Text.Text = "";
        }

        internal void SelectIf(List<DataModel.Frame> selectedFrames) {
            DisplayedFrame.IsSelected = selectedFrames.Contains(DisplayedFrame.Frame);
            if (GlobalItemSelector.SelectedFrame != null) {
                DisplayedFrame.IsGlobalSelectedFrame = GlobalItemSelector.SelectedFrame == DisplayedFrame.Frame;
            }
        }

        private void NewLine(int position) {
            var l = new Line();
            l.Margin = new Thickness(Math.Max(position, 0), 0, -Math.Min(position, 0), 0);
            l.Y2 = 1;
            l.Stretch = Stretch.Fill;
            l.Stroke = Brushes.Black;
            l.StrokeThickness = 1;
            Lines.Add(l);

            contentHolder.Children.Add(l);
            Grid.SetColumn(l, SplitterPosition == Position.Left ? 1 : 0);
        }

        internal void Set(Tuple<DataModel.Frame, int> tuple) {
            int count = tuple.Item2;

            if (count > 0) {
                NewLine(0);
                if (count >= 10) {
                    NewLine(-5);
                    NewLine(5);
                    if (count >= 100) {
                        NewLine(-10);
                        NewLine(10);
                    }
                }
                Text.Text = count.ToString();
                Splitter.Width = new GridLength(13, GridUnitType.Pixel);
            } else {
                Splitter.Width = new GridLength(0, GridUnitType.Pixel);
            }
            
            DisplayedFrame.Frame = tuple.Item1;
        }
    }
}
