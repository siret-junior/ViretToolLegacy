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
    /// Interaction logic for SingleTimeFrame.xaml
    /// </summary>
    public partial class SingleTimeFrame : UserControl {

        public enum Position { Left, Right };
        public DisplayFrame DisplayedFrame;

        public SingleTimeFrame(DisplayFrame displayedFrame, int count, Position pos) {
            InitializeComponent();
            DisplayedFrame = displayedFrame;

            if (count > 0) {
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(1, GridUnitType.Star);
                ColumnDefinition c2 = new ColumnDefinition();
                c2.Width = new GridLength(13, GridUnitType.Pixel);
                if (pos == Position.Left) {
                    contentHolder.ColumnDefinitions.Add(c1);
                    contentHolder.ColumnDefinitions.Add(c2);
                } else {
                    contentHolder.ColumnDefinitions.Add(c2);
                    contentHolder.ColumnDefinitions.Add(c1);
                }

                NewLine(0, pos);
                if (count >= 10) {
                    NewLine(-5, pos);
                    NewLine(5, pos);
                    if (count >= 100) {
                        NewLine(-10, pos);
                        NewLine(10, pos);
                    }
                }

                var b = new TextBlock();
                b.Text = count.ToString();
                b.VerticalAlignment = VerticalAlignment.Center;
                b.Background = Brushes.White;
                b.FontSize = 8;
                b.HorizontalAlignment = HorizontalAlignment.Center;

                contentHolder.Children.Add(b);
                Grid.SetColumn(b, pos == Position.Left ? 1 : 0);

                contentHolder.Children.Add(displayedFrame);
                Grid.SetColumn(displayedFrame, pos == Position.Left ? 0 : 1);
            } else {
                ColumnDefinition c1 = new ColumnDefinition();
                c1.Width = new GridLength(1, GridUnitType.Star);
                contentHolder.ColumnDefinitions.Add(c1);

                contentHolder.Children.Add(displayedFrame);
                Grid.SetColumn(displayedFrame, 0);
            }
        }

        private void NewLine(int position, Position pos) {
            var l = new Line();
            l.Margin = new Thickness(Math.Max(position, 0), 0, -Math.Min(position, 0), 0);
            l.Y2 = 1;
            l.Stretch = Stretch.Fill;
            l.Stroke = Brushes.Black;
            l.StrokeThickness = 1;

            contentHolder.Children.Add(l);
            Grid.SetColumn(l, pos == Position.Left ? 1 : 0);
        }
    }
}
