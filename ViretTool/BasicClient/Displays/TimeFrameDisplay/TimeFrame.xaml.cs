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
using ViretTool.DataModel;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient {
    /// <summary>
    /// Interaction logic for TimeFrame.xaml
    /// </summary>
    public partial class TimeFrame : UserControl {

        private List<DisplayFrame> DisplayFrames = new List<DisplayFrame>();
        
        public TimeFrame(IDisplayControl disp, RankedTimeFrame rankedTimeFrame) {
            InitializeComponent();
            Fill(disp, rankedTimeFrame);
        }

        private void Fill(IDisplayControl disp, RankedTimeFrame rankedTimeFrame) {
            SingleTimeFrame timeFrame;
            DisplayFrame displayedFrame;
            frameGrid.Children.Clear();
            frameGrid.Columns = rankedTimeFrame.Length * 2 + 1;

            for (int i = 0; i < rankedTimeFrame.Length - rankedTimeFrame.LeftFrames.Count; i++) {
                frameGrid.Children.Add(new DisplayFrame(null));
            }

            rankedTimeFrame.LeftFrames.Reverse();
            foreach (var t in rankedTimeFrame.LeftFrames) {
                displayedFrame = new DisplayFrame(disp);
                displayedFrame.Frame = t.Item1;

                DisplayFrames.Add(displayedFrame);

                timeFrame = new SingleTimeFrame(displayedFrame, t.Item2, SingleTimeFrame.Position.Left);
                frameGrid.Children.Add(timeFrame);
            }

            var border = new Border();
            border.BorderBrush = Brushes.Blue;
            border.BorderThickness = new Thickness(2);

            displayedFrame = new DisplayFrame(disp);
            displayedFrame.Frame = rankedTimeFrame.RankedFrame.Frame;
            DisplayFrames.Add(displayedFrame);
            border.Child = displayedFrame;
            frameGrid.Children.Add(border);

            foreach (var t in rankedTimeFrame.RightFrames) {
                displayedFrame = new DisplayFrame(disp);
                displayedFrame.Frame = t.Item1;

                DisplayFrames.Add(displayedFrame);

                timeFrame = new SingleTimeFrame(displayedFrame, t.Item2, SingleTimeFrame.Position.Right);
                frameGrid.Children.Add(timeFrame);
            }
        }

        public void UpdateSelection(List<DataModel.Frame> selectedFrames) {
            foreach (var item in DisplayFrames) {
                item.IsSelected = selectedFrames.Contains(item.Frame);
            }
        }
    }
}
