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
    /// Interaction logic for TimeFrameDisplay.xaml
    /// </summary>
    public partial class TimeFrameDisplay : UserControl {
        public TimeFrameDisplay() {
            InitializeComponent();
        }
        public Dataset Dataset;
        public RankingModel.SimilarityModels.FloatVectorModel SimilarityModel;
        private List<RankedFrame> mResultFrames = null;
        private List<RankedTimeFrame> mTimeFrames = null;
        private HashSet<int> mUsedIndices = null;
        private int aggregatedUpTo = 0;
        private int mDisplayWidth = 4;

        public enum ThresholdType { Similarity, Time }

        public static readonly DependencyProperty PageProperty = DependencyProperty.Register("Page", typeof(int), typeof(TimeFrameDisplay), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(double), typeof(TimeFrameDisplay), new FrameworkPropertyMetadata(0.3d));
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(ThresholdType), typeof(TimeFrameDisplay), new FrameworkPropertyMetadata(ThresholdType.Similarity));


        public int Page {
            get { return (int)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public ThresholdType Type {
            get { return (ThresholdType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public double Threshold {
            get { return (double)GetValue(ThresholdProperty); }
            set { SetValue(ThresholdProperty, value); }
        }

        public int DisplaySize {
            get { return timeFrameGrid.Rows; }
            set {
                timeFrameGrid.Children.Clear();
                timeFrameGrid.Rows = value;
                DisplayPage(0);
            }
        }

        public List<RankedFrame> ResultFrames {
            get { return mResultFrames; }
            set {
                if (value == null || value.Count == 0) {
                    return;
                }
                mResultFrames = value;
                AggregateResult();
                DisplayPage(0);
            }
        }

        private void ControlUIChanged(object sender, EventArgs e) {
            timeFrameGrid.Children.Clear();
            AggregateResult();
            DisplayPage(0);
        }

        private void AggregateResult() {
            aggregatedUpTo = 0;
            mUsedIndices = new HashSet<int>();
            mTimeFrames = new List<RankedTimeFrame>();

            AggregateUpTo(DisplaySize);
        }

        private void AggregateUpTo(int numberOfTimeFrames) {
            int i = 0;
            while (aggregatedUpTo < numberOfTimeFrames && i < mResultFrames.Count) {
                if (!mUsedIndices.Contains(mResultFrames[i].Frame.ID)) {
                    mUsedIndices.Add(mResultFrames[i].Frame.ID);

                    List<Tuple<DataModel.Frame, int>> lf = CalcTimeFrame(mResultFrames[i], -1);
                    List<Tuple<DataModel.Frame, int>> rf = CalcTimeFrame(mResultFrames[i], 1);

                    mTimeFrames.Add(new RankedTimeFrame(mResultFrames[i], lf, rf, mDisplayWidth));
                    aggregatedUpTo++;
                } else {
                    if (Dataset.Frames[mResultFrames[i].Frame.ID].FrameVideo.VideoID == 12) {
                        System.Diagnostics.Debug.WriteLine("indice from vid 12: " + i.ToString());
                    }
                }
                i++;
            }
        }


        public void DisplayPage(int page) {
            pageTextBox.Text = page.ToString();

            AggregateUpTo((page + 1) * DisplaySize);

            timeFrameGrid.Children.Clear();
            int offset = page * DisplaySize;
            int count = (aggregatedUpTo - offset < DisplaySize) ? aggregatedUpTo - offset : DisplaySize;

            for (int i = offset; i < offset + count; i++) {
                var tf = new TimeFrame(mTimeFrames[i]);
                timeFrameGrid.Children.Add(tf);
            }
            this.UpdateLayout();
        }

        private List<Tuple<DataModel.Frame, int>> CalcTimeFrame(RankedFrame f, int dir) {
            int fID = f.Frame.ID;
            int vID = f.Frame.FrameVideo.VideoID;

            int i = fID;
            int thrIndex = 0;
            var ret = new List<Tuple<DataModel.Frame, int>>();

            for (; i >= 0 && i < Dataset.Frames.Count && Dataset.Frames[i].FrameVideo.VideoID == vID; i += dir) {
                double d = 0;
                if (Type == ThresholdType.Similarity) {
                    d = RankingModel.SimilarityModels.FloatVectorModel.ComputeDistance(
                        SimilarityModel.mFloatVectors[fID], SimilarityModel.mFloatVectors[i]);
                } else {
                    d = Math.Abs(Dataset.Frames[fID].FrameNumber - Dataset.Frames[i].FrameNumber);
                    d = d / 99;
                }
                mUsedIndices.Add(i);

                if (Threshold < d) {
                    ret.Add(new Tuple<DataModel.Frame, int>(Dataset.Frames[i], Math.Abs(fID - i) - 1));
                    fID = i;
                    thrIndex++;
                }
                if (thrIndex == mDisplayWidth) break;
            }
            return ret;
        }

        private void pageTextBox_KeyUp(object sender, KeyEventArgs e) {
            int page = 0;
            if (e.Key == Key.Enter && int.TryParse(pageTextBox.Text, out page)) {
                Page = page;
                DisplayPage(Page);
            }
        }
    }
}
