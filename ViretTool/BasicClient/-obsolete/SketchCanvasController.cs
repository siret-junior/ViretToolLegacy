using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// SketchCanvasController manages sketch drawing on a provided WPF canvas.
    /// </summary>
    class SketchCanvasController
    {
        private Canvas sketchCanvas;
        private List<ColorPoint> mColorPoints;
        private ColorPoint mSelectedColorPoint;

        public delegate void SketchChangedHandler(List<Tuple<Point, Color>> colorSketch);
        
        /// <summary>
        /// SketchChangedEvent is raised whenever users create, move or delete a colored circle.
        /// </summary>
        public event SketchChangedHandler SketchChangedEvent;

        /// <summary>
        /// SketchCanvasController manages sketch drawing on a provided WPF canvas.
        /// </summary>
        /// <param name="canvas">Canvas represents the area, where users place the colored circles.</param>
        public SketchCanvasController(Canvas canvas)
        {
            sketchCanvas = canvas;
            sketchCanvas.Background = Brushes.White;

            sketchCanvas.MouseDown += Canvas_MouseDown;
            sketchCanvas.MouseUp += Canvas_MouseUp;
            sketchCanvas.MouseMove += Canvas_MouseMove;

            mColorPoints = new List<ColorPoint>();
            mSelectedColorPoint = null;

            DrawGrid();
        }

        /// <summary>
        /// Clears all colored circles from the canvas. SketchChangedEvent is not raised.
        /// </summary>
        public void Clear()
        {
            foreach (ColorPoint CP in mColorPoints)
                CP.RemoveFromCanvas(sketchCanvas);

            mColorPoints.Clear();
            mSelectedColorPoint = null;
        }

        private void RaiseSketchChangedEvent()
        {
            if (SketchChangedEvent != null)
            {
                List<Tuple<Point, Color>> colorSketch = new List<Tuple<Point, Color>>();

                foreach (ColorPoint CP in mColorPoints)
                {
                    Point p = new Point(CP.Position.X / sketchCanvas.Width, CP.Position.Y / sketchCanvas.Height);
                    colorSketch.Add(new Tuple<Point, Color>(p, CP.FillColor));
                }

                SketchChangedEvent(colorSketch);
            }
        }

        private void DrawGrid()
        {
            double WX = 20, WY = 15;
            double wx = sketchCanvas.Width / WX, wy = sketchCanvas.Height / WY;

            for (int i = 0; i <= WX; i++)
            {
                Line l = new Line();
                l.X1 = i * wx; l.X2 = l.X1;
                l.Y1 = 0; l.Y2 = sketchCanvas.Height;
                l.Stroke = Brushes.Lavender;
                if (i % 5 != 0) l.StrokeDashArray = new DoubleCollection() { 3 };
                sketchCanvas.Children.Add(l);
            }

            for (int j = 0; j <= WY; j++)
            {
                Line l = new Line();
                l.X1 = 0; l.X2 = sketchCanvas.Width;
                l.Y1 = j * wy; l.Y2 = j * wy;
                l.Stroke = Brushes.Lavender;
                if (j % 5 != 0) l.StrokeDashArray = new DoubleCollection() { 3 };
                sketchCanvas.Children.Add(l);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(sketchCanvas);

            mSelectedColorPoint = ColorPoint.IsSelected(mColorPoints, p);

            // remove circle
            if (e.RightButton == MouseButtonState.Pressed && mSelectedColorPoint != null)
            {
                mColorPoints.Remove(mSelectedColorPoint);
                mSelectedColorPoint.RemoveFromCanvas(sketchCanvas);

                RaiseSketchChangedEvent();

                mSelectedColorPoint = null;
                return;
            }

            // add new circle
            if (mSelectedColorPoint == null)
            {
                // TODO - load palette from file
                SolidColorBrush[] brushes = typeof(Brushes).GetProperties().Select(b => b.GetValue(null) as SolidColorBrush).OrderBy(x => x.Color.ToString()).ToArray();
                ColorPicker CP = new ColorPicker(brushes);

                if (CP.Show(Mouse.GetPosition(Application.Current.MainWindow)))
                {
                    mSelectedColorPoint = new ColorPoint(p, CP.SelectedColor, sketchCanvas);
                    mColorPoints.Add(mSelectedColorPoint);
                    mSelectedColorPoint = null;
                    RaiseSketchChangedEvent();
                }
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mSelectedColorPoint != null)
            {
                RaiseSketchChangedEvent();
            }

            mSelectedColorPoint = null;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(sketchCanvas);
            if (p.X < ColorPoint.Radius || p.X > sketchCanvas.Width - ColorPoint.Radius) return;
            if (p.Y < ColorPoint.Radius || p.Y > sketchCanvas.Height - ColorPoint.Radius) return;

            if (mSelectedColorPoint != null)
            {
                mSelectedColorPoint.UpdatePosition(p);
            }
        }

        private class ColorPoint
        {
            public Point Position;
            public Color FillColor;
            public Ellipse FillEllipse;
            public Ellipse SearchRadiusEllipse;
            public const int Radius = 15;
            public const int SearchRadius = 40;

            public ColorPoint(Point p, Color c, Canvas canvas)
            {
                Position = p;
                FillColor = c;

                FillEllipse = new Ellipse();
                FillEllipse.Width = 2 * Radius;
                FillEllipse.Height = 2 * Radius;
                FillEllipse.Fill = new SolidColorBrush(c);
                canvas.Children.Add(FillEllipse);

                SearchRadiusEllipse = new Ellipse();
                SearchRadiusEllipse.Width = 2 * SearchRadius;
                SearchRadiusEllipse.Height = 2 * SearchRadius;
                SearchRadiusEllipse.Stroke = Brushes.LightGray;
                canvas.Children.Add(SearchRadiusEllipse);

                UpdatePosition(p);
            }

            public void RemoveFromCanvas(Canvas canvas)
            {
                canvas.Children.Remove(FillEllipse);
                canvas.Children.Remove(SearchRadiusEllipse);
            }

            public void UpdatePosition(Point p)
            {
                Position = p;
                Canvas.SetTop(FillEllipse, Position.Y - Radius);
                Canvas.SetLeft(FillEllipse, Position.X - Radius);

                Canvas.SetTop(SearchRadiusEllipse, Position.Y - SearchRadius);
                Canvas.SetLeft(SearchRadiusEllipse, Position.X - SearchRadius);
            }

            public static ColorPoint IsSelected(List<ColorPoint> colorPoints, Point p)
            {
                ColorPoint result = null;
                foreach (ColorPoint CP in colorPoints)
                    if ((CP.Position.X - p.X) * (CP.Position.X - p.X) + (CP.Position.Y - p.Y) * (CP.Position.Y - p.Y) <= ColorPoint.Radius * ColorPoint.Radius)
                        return CP;

                return result;
            }
        }

        private class ColorPicker : Window
        {
            private Canvas mColorPickerPanel;
            private int mColorButtonWidth = 20;
            public Color SelectedColor = Colors.White;

            public ColorPicker(SolidColorBrush[] brushes)
            {
                //brushes = CreateBrushes();
                mColorPickerPanel = new Canvas();

                Content = mColorPickerPanel;

                int ColorsOnLine = 15;
                for (int i = 0; i < brushes.Length; i++)
                {
                    Button b = new Button();
                    b.Width = mColorButtonWidth;
                    b.Height = b.Width;
                    b.Background = brushes[i];
                    b.Click += LeftClick;

                    mColorPickerPanel.Children.Add(b);
                    Canvas.SetLeft(b, (i % ColorsOnLine) * mColorButtonWidth);
                    Canvas.SetTop(b, 10 + (int)Math.Floor(i / (double)ColorsOnLine) * mColorButtonWidth);
                }

                Width = ColorsOnLine * mColorButtonWidth + 15;
                Height = (brushes.Length / ColorsOnLine) * mColorButtonWidth + 70;
            }

            private SolidColorBrush[] CreateBrushes()
            {
                List<SolidColorBrush> brushes = new List<SolidColorBrush>();

                for (int r = 0; r < 256; r += 63)
                    for (int g = 0; g < 256; g += 63)
                        for (int b = 0; b < 256; b += 63)
                            brushes.Add(new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b)));

                //brushes.Add(new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                return brushes.ToArray();
            }

            private bool mColorSelected = false;
            private void LeftClick(object sender, RoutedEventArgs e)
            {
                SelectedColor = ((SolidColorBrush)((Button)sender).Background).Color;
                mColorSelected = true;
                Close();
            }

            public bool Show(Point p)
            {
                Left = p.X - 20;
                Top = p.Y - 20;
                ShowDialog();
                return mColorSelected;
            }
        }
    }



}
