using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ViretTool.DataModel;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.IO;
using VitretTool.EvaluationServer.Controls;

namespace VitretTool.EvaluationServer {
    class UIDrawer {
        private Grid mInfoGrid;
        private Grid mPresentationGrid;
        private TextBlock mTimeInfo;
        private TextBlock mTaskInfo;
        private MediaElement mMediaElement;
        private Grid mMediaElementFullscreen;
        private Chart mTeamsChart;
        private Grid mTeamsGrid;
        private Grid mContentBox;
        private Dataset mDataset;

        private Dictionary<long, TeamUI> mTeams = new Dictionary<long, TeamUI>();

        public UIDrawer(Grid infoGrid, Grid presentationGrid, TextBlock timeInfo, TextBlock taskInfo, MediaElement mediaElement, Grid mediaElementFullscreen, Chart teamsChart, Grid teamsGrid, Grid contentBox, Dataset dataset) {
            mInfoGrid = infoGrid;
            mPresentationGrid = presentationGrid;
            mTimeInfo = timeInfo;
            mTaskInfo = taskInfo;
            mMediaElement = mediaElement;
            mMediaElementFullscreen = mediaElementFullscreen;
            mTeamsChart = teamsChart;
            mTeamsGrid = teamsGrid;
            mContentBox = contentBox;
            mDataset = dataset;
        }

        public void DrawNewKeyframe(long teamId, int videoId, int frameId, int value, int taskId) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                BitmapSource b = null;
                if (videoId < mDataset.Videos.Count && videoId >= 0) {
                    foreach (var item in mDataset.Videos[videoId].Frames) {
                        if (item.FrameNumber >= frameId) {
                            b = item.Bitmap;
                            break;
                        }
                    }
                }

                mTeams[teamId].InsertNewResult(mTeamsGrid, b, value, taskId);
                if (value > 0) UpdateChart();
            });
        }

        public void DrawNewTeam(Team team) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                var t = new TeamUI(team.Color);
                mTeams.Add(team.Id, t);
                t.InsertIntoGrid(mTeamsGrid, team.Name);
            });
        }
        
        public void UpdateTime(TimeSpan remaining) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                mTimeInfo.Text = remaining.ToString();
            });
        }

        public void ShowStartScreen(string ip, int port) {
            var tb = new TextBlock();
            tb.Inlines.Add("Connect to ");
            tb.Inlines.Add(new Bold(new Run(ip)));
            tb.Inlines.Add(":");
            tb.Inlines.Add(new Bold(new Run(port.ToString())));
            tb.Inlines.Add(" to enter the competition and submit results.");

            tb.Foreground = Brushes.White;
            tb.FontSize = 30;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.TextAlignment = TextAlignment.Center;
            tb.TextWrapping = TextWrapping.Wrap;

            mInfoGrid.Children.Clear();
            mInfoGrid.Children.Add(tb);
        }

        public void ShowEndScreen() {
            mInfoGrid.Children.Clear();
            mInfoGrid.Visibility = Visibility.Visible;
            mPresentationGrid.Visibility = Visibility.Hidden;

            mPresentationGrid.Children.Remove(mTeamsChart);
            mInfoGrid.Children.Add(mTeamsChart);
        }

        private void UpdateChart() {
            var lines = new List<TeamUI.Line>();

            foreach (KeyValuePair<long, TeamUI> pair in mTeams) {
                lines.Add(pair.Value.ChartLine);
            }

            mTeamsChart.ItemsSource = lines;
            mTeamsChart.DrawGraph();
        }

        public void LoadTask(int taskId, string source) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                foreach(var pair in mTeams) {
                    pair.Value.RemoveHistory();
                }
                foreach (var pair in mTeams) {
                    pair.Value.UpdateChartLine(taskId);
                }
                UpdateChart();

                mInfoGrid.Visibility = Visibility.Hidden;
                mPresentationGrid.Visibility = Visibility.Visible;

                mTaskInfo.Text = "Task " + taskId;
                var s = Path.GetFullPath(source);
                mMediaElement.Source = new Uri(s, UriKind.Absolute);
                mMediaElement.LoadedBehavior = MediaState.Manual;
            });
        }

        public void StartTask(int taskId) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                mContentBox.Children.Remove(mMediaElement);
                mMediaElementFullscreen.Children.Add(mMediaElement);
                mMediaElementFullscreen.Visibility = Visibility.Visible;

                mMediaElement.Play();
                mMediaElement.MediaEnded += OnVideoEnded;
            });
        }

        private void OnVideoEnded(object sender, RoutedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                if (mMediaElementFullscreen.Children.Contains(mMediaElement)) {
                    mMediaElementFullscreen.Children.Remove(mMediaElement);
                    mContentBox.Children.Add(mMediaElement);
                    mMediaElementFullscreen.Visibility = Visibility.Hidden;
                }
                mMediaElement.Position = TimeSpan.Zero;
                mMediaElement.Play();
            });
        }

        public void FinishTask(int taskId) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                if (mMediaElementFullscreen.Children.Contains(mMediaElement)) {
                    mMediaElementFullscreen.Children.Remove(mMediaElement);
                    mContentBox.Children.Add(mMediaElement);
                    mMediaElementFullscreen.Visibility = Visibility.Hidden;
                }

                mMediaElement.Stop();
                mMediaElement.Source = null;
                UpdateChart();
                mTaskInfo.Text = "Task " + taskId + " finished!";
            });
        }

        class TeamUI {
            Color mColor;
            StackPanel mStackPanel;
            public Line ChartLine { get; }

            public TeamUI(Color c) {
                mColor = c;
                ChartLine = new Line() { Color = c };
            }

            public void InsertIntoGrid(Grid g, string name) {
                mStackPanel = new StackPanel();
                var b = new Border();
                b.Background = new SolidColorBrush(mColor);
                b.Height = 100;

                var s = new StackPanel();
                s.VerticalAlignment = VerticalAlignment.Center;
                b.Child = s;

                var t = new TextBlock();
                t.FontSize = 26;
                t.TextWrapping = TextWrapping.NoWrap;
                t.TextAlignment = TextAlignment.Center;
                t.Text = name;
                s.Children.Add(t);

                t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.FontSize = 24;
                t.FontWeight = FontWeights.Bold;
                t.Text = "0";
                s.Children.Add(t);

                mStackPanel.Children.Add(b);

                mStackPanel.Children.Add(new StackPanel());
                mStackPanel.MaxWidth = 200;

                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.Children.Add(mStackPanel);
                Grid.SetColumn(mStackPanel, g.Children.Count - 1);
            }

            public void RemoveHistory() {
                var sp = mStackPanel.Children[1] as StackPanel;
                sp.Children.Clear();
            }

            public void InsertNewResult(Grid g, BitmapSource bitmap, int value, int taskId) {
                int newVal = value;
                if (ChartLine.Points[ChartLine.Points.Count - 1].X == taskId) {
                    if (ChartLine.Points.Count > 1) {
                        newVal += (int)ChartLine.Points[ChartLine.Points.Count - 2].Y;
                    }
                    ChartLine.Points[ChartLine.Points.Count - 1] = new Point(taskId, newVal);
                } else {
                    newVal += (int)ChartLine.Points[ChartLine.Points.Count - 1].Y;
                    ChartLine.Points.Add(new Point(taskId,  newVal));
                }
                if (value > 0) {
                    var teamNameGrid = ((Border)mStackPanel.Children[0]).Child as StackPanel;
                    ((TextBlock)teamNameGrid.Children[1]).Text = newVal.ToString();
                }

                var sp = mStackPanel.Children[1] as StackPanel;

                if (sp.Children.Count > 0) {
                    if (((Border)sp.Children[0]).ActualHeight + sp.ActualHeight + 100 > g.ActualHeight || ((Border)sp.Children[0]).ActualHeight == 0) {
                        sp.Children.RemoveAt(0);
                    } 
                }

                var b = new Border();
                b.Padding = new Thickness(10);
                b.Background = value > 0 ? Brushes.Green : Brushes.Red;
                var gr = new Grid();
                var i = new Image();
                if (bitmap == null) {
                    System.Drawing.Bitmap bmp = System.Drawing.Bitmap.FromHicon(System.Drawing.SystemIcons.Error.Handle);
                    i.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                } else {
                    i.Source = bitmap;
                }
                i.Stretch = Stretch.Fill;
                b.Child = gr;
                gr.Children.Add(i);
                var tb = new TextBlock();
                tb.Text = value.ToString();
                tb.VerticalAlignment = VerticalAlignment.Bottom;
                tb.TextAlignment = TextAlignment.Right;
                tb.Foreground = Brushes.White;
                tb.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
                tb.FontSize = 24;
                tb.Padding = new Thickness(5, 0, 5, 0);
                tb.FontWeight = FontWeights.Bold;

                gr.Children.Add(tb);

                sp.Children.Add(b);
            }

            public void UpdateChartLine(int taskId) {
                if (ChartLine.Points[ChartLine.Points.Count - 1].X != taskId) {
                    ChartLine.Points.Add(new Point(taskId, ChartLine.Points[ChartLine.Points.Count - 1].Y));
                }
            }

            public class Line : IChartLine {
                public List<Point> Points = new List<Point>() { new Point(0, 0) };
                public Color Color { get; set; }

                IEnumerable<Point> IChartLine.Points => Points;
            }
        }
    }
}
