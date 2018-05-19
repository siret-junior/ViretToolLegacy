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
using ViretTool.RankingModel;

namespace ViretTool.BasicClient {
    /// <summary>
    /// Interaction logic for TimeFrame.xaml
    /// </summary>
    public partial class TimeFrame : UserControl {

        IDisplayControl ParentDisplay;
        SingleTimeFrame[] LeftFrames;
        SingleTimeFrame[] RightFrames;
        DisplayFrame CenterFrame;
        
        public TimeFrame(IDisplayControl disp, int colsPerTimeline) {
            InitializeComponent();
            ParentDisplay = disp;
            Fill(disp, colsPerTimeline);
        }

        private void Fill(IDisplayControl disp, int colsPerTimeline) {
            frameGrid.Children.Clear();
            frameGrid.Columns = colsPerTimeline;
            LeftFrames = new SingleTimeFrame[(colsPerTimeline - 1) / 2];
            RightFrames = new SingleTimeFrame[(colsPerTimeline - 1) / 2];

            for (int i = 0; i < (colsPerTimeline - 1) / 2; i++) {
                LeftFrames[LeftFrames.Length - 1 - i] = new SingleTimeFrame(disp, SingleTimeFrame.Position.Left);
                frameGrid.Children.Add(LeftFrames[LeftFrames.Length - 1 - i]);
            }

            var border = new Border();
            border.BorderBrush = Brushes.Blue;
            border.BorderThickness = new Thickness(2);

            CenterFrame =  new DisplayFrame(disp);
            border.Child = CenterFrame;
            frameGrid.Children.Add(border);

            for (int i = 0; i < (colsPerTimeline - 1) / 2; i++) {
                RightFrames[i] = new SingleTimeFrame(disp, SingleTimeFrame.Position.Right);
                frameGrid.Children.Add(RightFrames[i]);
            }
        }

        public void UpdateSelection(List<DataModel.Frame> selectedFrames) {
            foreach (var item in LeftFrames) {
                item.SelectIf(selectedFrames);
            }
            foreach (var item in RightFrames) {
                item.SelectIf(selectedFrames);
            }
            CenterFrame.IsSelected = selectedFrames.Contains(CenterFrame.Frame);
            if (GlobalItemSelector.SelectedFrame != null) {
                CenterFrame.IsGlobalSelectedFrame = GlobalItemSelector.SelectedFrame == CenterFrame.Frame;
            }
        }

        internal void Clear() {
            foreach (var item in LeftFrames) {
                item.Clear();
            }
            foreach (var item in RightFrames) {
                item.Clear();
            }
            CenterFrame.Clear();
        }

        internal void Set(RankedTimeFrame rankedTimeFrame) {
            for (int j = 0; j < rankedTimeFrame.LeftFrames.Count; j++) {
                LeftFrames[j].Set(rankedTimeFrame.LeftFrames[j]);
            }

            CenterFrame.Frame = rankedTimeFrame.RankedFrame.Frame;

            for (int j = 0; j < rankedTimeFrame.RightFrames.Count; j++) {
                RightFrames[j].Set(rankedTimeFrame.RightFrames[j]);
            }

        }
    }
}
