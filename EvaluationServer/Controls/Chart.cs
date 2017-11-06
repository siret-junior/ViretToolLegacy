using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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

namespace VitretTool.EvaluationServer.Controls {

    public class Chart : Control {

        static Chart() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Chart), new FrameworkPropertyMetadata(typeof(Chart)));
        }

        public const string PartPlot = "PART_Plot";
        public const string PartXAxis = "PART_XAxis";
        public const string PartYAxis = "PART_YAxis";

        ItemsControl mPlot;
        ItemsControl mXAxis;
        ItemsControl mYAxis;

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            mPlot = (ItemsControl)Template.FindName(PartPlot, this);
            mXAxis = (ItemsControl)Template.FindName(PartXAxis, this);
            mYAxis = (ItemsControl)Template.FindName(PartYAxis, this);

            SizeChanged += Chart_Loaded;
            Loaded += Chart_Loaded;
        }

        private void Chart_Loaded(object sender, RoutedEventArgs e) {
            DrawGraph();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IChartLine>), typeof(Chart), new FrameworkPropertyMetadata(null));

        public IEnumerable<IChartLine> ItemsSource {
            get { return (IEnumerable< IChartLine>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private void DrawYAxis(float min, float max, float spacing, List<FrameworkElement> objects) {
            var axis = new List<FrameworkElement>();
            
            while (min <= max) {
                var b = new Border();
                b.Height = 30;
                b.Margin = new Thickness(-10, 0, 0, 0);
                b.Width = 30;

                var tb = new TextBlock();
                tb.Text = min.ToString();
                tb.FontSize = 15;
                tb.TextAlignment = TextAlignment.Right;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.Foreground = Brushes.White;
                b.Child = tb;

                Canvas.SetTop(b, mYAxis.ActualHeight - (min * mYAxis.ActualHeight / max + 15));
                axis.Add(b);
                var l = new Line();
                l.X1 = 0;
                l.X2 = mPlot.ActualWidth;
                l.Y1 = mYAxis.ActualHeight - min * mYAxis.ActualHeight / max;
                l.Y2 = mYAxis.ActualHeight - min * mYAxis.ActualHeight / max;

                l.StrokeDashArray = new DoubleCollection() { 2, 2 };
                l.Stroke = Brushes.Gray;

                objects.Add(l);

                min += spacing;
            }
            mYAxis.ItemsSource = axis;
        }

        private void DrawXAxis(float min, float max, float spacing, List<FrameworkElement> objects) {
            var axis = new List<FrameworkElement>();

            while (min <= max) {
                var b = new Border();
                b.Margin = new Thickness(0, 6, 0, 0);
                b.Height = 24;
                b.Width = 30;

                var tb = new TextBlock();
                tb.Text = min.ToString();
                tb.FontSize = 15;
                tb.TextAlignment = TextAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Top;
                tb.Foreground = Brushes.White;
                b.Child = tb;

                Canvas.SetLeft(b, min * mXAxis.ActualWidth / max - 15);
                axis.Add(b);
                var l = new Line();
                l.Y1 = 0;
                l.Y2 = mPlot.ActualHeight;
                l.X1 = min * mXAxis.ActualWidth / max;
                l.X2 = min * mXAxis.ActualWidth / max;

                l.StrokeDashArray = new DoubleCollection() { 2, 2 };
                l.Stroke = Brushes.Gray;

                objects.Add(l);

                min += spacing;
            }
            mXAxis.ItemsSource = axis;
        }

        public void DrawGraph() {
            float XAxisSpacing = 1;
            float YAxisSpacing = 20;

            var canvasObjects = new List<FrameworkElement>();

            if (ItemsSource == null || !ItemsSource.Any()) {
                DrawYAxis(0, YAxisSpacing, YAxisSpacing, canvasObjects);
                DrawXAxis(0, XAxisSpacing, XAxisSpacing, canvasObjects);
                mPlot.ItemsSource = canvasObjects;
                return;
            }

            
            double minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

            foreach (IChartLine line in ItemsSource) {
                double tmp = line.Points.Max(p => p.Y);
                if (tmp > maxY) maxY = tmp;
                tmp = line.Points.Min(p => p.Y);
                if (tmp < minY) minY = tmp;

                tmp = line.Points.Max(p => p.X);
                if (tmp > maxX) maxX = tmp;
                tmp = line.Points.Min(p => p.X);
                if (tmp < minX) minX = tmp;
            }
            float maxXVal = (float)(Math.Floor(maxX / XAxisSpacing) + 1) * XAxisSpacing;
            float minXVal = (float)Math.Floor(minX / XAxisSpacing) * XAxisSpacing;

            float maxYVal = (float)(Math.Floor(maxY / YAxisSpacing) + 1) * YAxisSpacing;
            float minYVal = (float)Math.Floor(minY / YAxisSpacing) * YAxisSpacing;

            DrawYAxis(minYVal, maxYVal, YAxisSpacing, canvasObjects);
            DrawXAxis(minXVal, maxXVal, XAxisSpacing, canvasObjects);

            foreach (IChartLine line in ItemsSource) {
                var chartLine = new Polyline();

                foreach (Point point in line.Points) {
                    Point self = new Point();
                    self.X = (point.X - minXVal) * mPlot.ActualWidth / (maxXVal - minXVal);
                    self.Y = (point.Y - minYVal) * mPlot.ActualHeight / (maxYVal - minYVal);

                    var linePoint = new Ellipse();
                    linePoint.Width = 10;
                    linePoint.Height = 10;
                    linePoint.Fill = new SolidColorBrush(line.Color);
                    Canvas.SetLeft(linePoint, self.X - 5);
                    Canvas.SetTop(linePoint, self.Y - 5);
                    canvasObjects.Add(linePoint);

                    chartLine.Points.Add(self);
                }
                chartLine.StrokeThickness = 2;
                chartLine.Stroke = new SolidColorBrush(line.Color);

                canvasObjects.Add(chartLine);
            }

            mPlot.ItemsSource = canvasObjects;
        }

    }
}
