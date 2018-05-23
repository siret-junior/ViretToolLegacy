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
        DisplayFrame[] LeftFrames;
        DisplayFrame[] RightFrames;
        DisplayFrame CenterFrame;
        
        public TimeFrame(IDisplayControl disp, int colsPerTimeline) {
            InitializeComponent();
            ParentDisplay = disp;
            Fill(disp, colsPerTimeline);
        }

        private void Fill(IDisplayControl disp, int colsPerTimeline) {
            frameGrid.Children.Clear();
            frameGrid.Columns = colsPerTimeline;
            LeftFrames = new DisplayFrame[(colsPerTimeline - 1) / 2];
            RightFrames = new DisplayFrame[(colsPerTimeline - 1) / 2];

            for (int i = 0; i < (colsPerTimeline - 1) / 2; i++) {
                LeftFrames[LeftFrames.Length - 1 - i] = new DisplayFrame(disp);
                frameGrid.Children.Add(LeftFrames[LeftFrames.Length - 1 - i]);
            }

            var border = new Border();
            border.BorderBrush = Brushes.Blue;
            border.BorderThickness = new Thickness(2);

            CenterFrame =  new DisplayFrame(disp);
            border.Child = CenterFrame;
            frameGrid.Children.Add(border);

            for (int i = 0; i < (colsPerTimeline - 1) / 2; i++) {
                RightFrames[i] = new DisplayFrame(disp);
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
            CenterFrame.SelectIf(selectedFrames);
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
            var lf = rankedTimeFrame.LeftFrames;
            var rf = rankedTimeFrame.RightFrames;

            for (int j = 0; j < lf.Count; j++) {
                int skipped = lf.Count > j + 1 ? lf[j + 1].Item2 : 0;
                LeftFrames[j].Set(lf[j].Item1, skippedFrames:skipped);
            }

            int skippedCenter = (lf.Count > 0 ? lf[0].Item2 : 0) + (rf.Count > 0 ? rf[0].Item2 : 0);
            CenterFrame.Set(rankedTimeFrame.RankedFrame.Frame, skippedFrames:skippedCenter);

            for (int j = 0; j < rf.Count; j++) {
                int skipped = rf.Count > j + 1 ? rf[j + 1].Item2 : 0;
                RightFrames[j].Set(rf[j].Item1, skippedFrames: skipped);
            }

        }
    }
}
