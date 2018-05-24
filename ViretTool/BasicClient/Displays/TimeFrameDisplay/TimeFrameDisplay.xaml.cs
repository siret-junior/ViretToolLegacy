using System;
using System.Collections.Generic;
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
using ViretTool.BasicClient.Displays;
using ViretTool.DataModel;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient {
    /// <summary>
    /// Interaction logic for TimeFrameDisplay.xaml
    /// </summary>
    public partial class TimeFrameDisplay : UserControl, IDisplayControl, IMainDisplay {
        public TimeFrameDisplay() {
            InitializeComponent();
            ClearAndResize();
        }
        public Dataset Dataset;
        public RankingModel.SimilarityModels.FloatVectorModel SimilarityModel;
        private List<RankedFrame> mResultFrames = null;
        private List<RankedTimeFrame> mTimeFrames = null;
        private Dictionary<int, int> mUsedIndices = null;
        private int aggregatedUpTo = 0;
        private int aggregatedUpTo_ResultIndex = 0;
        public enum ThresholdType { Similarity, Time }

        TimeFrame[] UITimeFrames;

        public static readonly DependencyProperty PageProperty = DependencyProperty.Register("Page", typeof(int), typeof(TimeFrameDisplay), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(double), typeof(TimeFrameDisplay), new FrameworkPropertyMetadata(0.3d));
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(ThresholdType), typeof(TimeFrameDisplay), new FrameworkPropertyMetadata(ThresholdType.Similarity));

        private List<DataModel.Frame> mSelectedFrames = new List<DataModel.Frame>();
        public List<DataModel.Frame> SelectedFrames {
            get { return mSelectedFrames; }
            set {
                mSelectedFrames = value;
                UpdateSelectionVisualization();
            }
        }

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

        public List<RankedFrame> ResultFrames {
            get { return mResultFrames; }
            set {
                if (value == null || value.Count == 0) {
                    return;
                }
                mResultFrames = value;
                RefillNeeded = true;

                if (GlobalItemSelector.ActiveDisplay == this) {
                    AggregateResult();
                    DisplayPage(0);
                }
            }
        }

        private void ControlUIChanged(object sender, EventArgs e) {
            if (GlobalItemSelector.ActiveDisplay != this) {
                RefillNeeded = true;
                return;
            }
            AggregateResult();
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            } else {
                DisplayPage(0);
            }
        }

        private void AggregateResult() {
            aggregatedUpTo = 0;
            aggregatedUpTo_ResultIndex = 0;
            mUsedIndices = new Dictionary<int, int>();
            mTimeFrames = new List<RankedTimeFrame>();

            AggregateUpTo(TimelinesPerPage);
        }

        private void AggregateUpTo(int numberOfTimeFrames) {
            int i = aggregatedUpTo_ResultIndex;
            while (aggregatedUpTo < numberOfTimeFrames && mResultFrames != null && i < mResultFrames.Count) {
                if (!mUsedIndices.ContainsKey(mResultFrames[i].Frame.ID)) {
                    mUsedIndices.Add(mResultFrames[i].Frame.ID, aggregatedUpTo);

                    List<Tuple<DataModel.Frame, int>> lf = CalcTimeFrame(mResultFrames[i], -1);
                    List<Tuple<DataModel.Frame, int>> rf = CalcTimeFrame(mResultFrames[i], 1);

                    mTimeFrames.Add(new RankedTimeFrame(mResultFrames[i], lf, rf, (ColumsPerTimeline - 1)/2));
                    aggregatedUpTo++;
                }
                i++;
            }
            aggregatedUpTo_ResultIndex = i;
        }


        private void SeekToFrame(DataModel.Frame selectedFrame) {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            while (true) {
                if (mUsedIndices.ContainsKey(selectedFrame.ID)) {
                    int timelinePosition = mUsedIndices[selectedFrame.ID];
                    GlobalItemSelector.SelectedFrame = mTimeFrames[timelinePosition].RankedFrame.Frame;

                    DisplayPage(timelinePosition / TimelinesPerPage, updateSelected:false);
                    return;
                }

                if (stopWatch.ElapsedMilliseconds > 1000) {
                    MessageBox.Show("Frame seek takes more than 1 second, showing first page.", "It takes soo long :(",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DisplayPage(0, updateSelected: false);
                    return;
                }

                int cache = aggregatedUpTo_ResultIndex;
                AggregateUpTo(aggregatedUpTo + TimelinesPerPage * 2);
                if (aggregatedUpTo_ResultIndex == cache) {
                    MessageBox.Show("Frame not availible, probably some filter is set. Click Clear All to reset filters.", "Frame not availible :(",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
        }


        const double DESIRED_ASPECT_RATIO = 3.0 / 4.0;
        const int DEFAULT_SIZE = 11;
        int TimelinesPerRow { get; set; } = 1;
        int TimelinesPerPage {
            get {
                return TimelinesPerRow * timeFrameGrid.Rows;
            }
        }

        int Size = DEFAULT_SIZE;
        int ColumsPerTimeline {
            get {
                int i = 1;
                for (; i < 18; i+=2) {
                    if (TimelinesPerRow * i >= Size)
                        return i;
                }
                return i;
            }
        }
        int Colums {
            get {
                return ColumsPerTimeline * TimelinesPerRow;
            }
        }
        int Rows {
            get {
                double desiredFrameHeight = timeFrameGrid.ActualWidth / Colums * DESIRED_ASPECT_RATIO;
                int rows = (int)((timeFrameGrid.ActualHeight + desiredFrameHeight / 2) / desiredFrameHeight);
                return Math.Max(rows, 1);
            }
        }

        bool RefillNeeded = true;

        public void ClearAndResize() {
            RefillNeeded = true;

            if (UITimeFrames == null || UITimeFrames.Length != TimelinesPerRow * Rows || timeFrameGrid.Rows != Rows) {
                timeFrameGrid.Children.Clear();
                timeFrameGrid.Rows = Rows;
                timeFrameGrid.Columns = TimelinesPerRow;

                UITimeFrames = new TimeFrame[TimelinesPerPage];
                
                for (int i = 0; i < TimelinesPerPage; i++) {
                    UITimeFrames[i] = new TimeFrame(this, ColumsPerTimeline);
                }
                for (int j = 0; j < Rows; j++) {
                    for (int i = 0; i < TimelinesPerRow; i++) {
                        if (i != TimelinesPerRow - 1) {
                            var border = new Border();
                            border.BorderBrush = Brushes.DarkGray;
                            border.BorderThickness = new Thickness(0, 0, 5, 0);
                            border.Child = UITimeFrames[j + i * Rows];
                            timeFrameGrid.Children.Add(border);
                        } else {
                            timeFrameGrid.Children.Add(UITimeFrames[j + i * Rows ]);
                        }
                    }
                }
            } else {
                for (int i = 0; i < TimelinesPerPage; i++) {
                    UITimeFrames[i].Clear();
                }
            }
        }


        protected void UpdateSelectionVisualization() {
            foreach (var item in UITimeFrames) {
                item.UpdateSelection(SelectedFrames);
            }
        }


        public void DisplayPage(int page, bool updateSelected = true) {
            ClearAndResize();

            //pageTextBox.Text = page.ToString();
            if (page < 0) {
                page = 0;
            }
            AggregateUpTo((page + 1) * TimelinesPerPage);

            if ((page + 1) * TimelinesPerPage > aggregatedUpTo) {
                page = (aggregatedUpTo - 1) / TimelinesPerPage;
            }

            int offset = page * TimelinesPerPage;
            int count = (aggregatedUpTo - offset < TimelinesPerPage) ? aggregatedUpTo - offset : TimelinesPerPage;
            Page = page;

            if (updateSelected) {
                GlobalItemSelector.SelectedFrame = mTimeFrames[offset].RankedFrame.Frame;
            }
            for (int i = offset, j = 0; i < offset + count; i++, j++) {
                UITimeFrames[j].Set(mTimeFrames[i]);
            }
            this.UpdateLayout();
            UpdateSelectionVisualization();

            RefillNeeded = false;
        }

        private List<Tuple<DataModel.Frame, int>> CalcTimeFrame(RankedFrame f, int dir) {
            int fID = f.Frame.ID;
            int vID = f.Frame.FrameVideo.VideoID;

            int i = fID;
            int thrIndex = 0;
            var ret = new List<Tuple<DataModel.Frame, int>>();

            int mDisplayWidth = (ColumsPerTimeline - 1) / 2;
            for (; i >= 0 && i < Dataset.Frames.Count && Dataset.Frames[i].FrameVideo.VideoID == vID; i += dir) {
                double d = 0;
                if (Type == ThresholdType.Similarity) {
                    d = RankingModel.SimilarityModels.FloatVectorModel.ComputeDistance(
                        SimilarityModel.mFloatVectors[fID], SimilarityModel.mFloatVectors[i]);
                } else {
                    d = Math.Abs(Dataset.Frames[fID].FrameNumber - Dataset.Frames[i].FrameNumber);
                    d = d / 99;
                }
                if (!mUsedIndices.ContainsKey(i))
                    mUsedIndices.Add(i, aggregatedUpTo);

                if (Threshold < d) {
                    ret.Add(new Tuple<DataModel.Frame, int>(Dataset.Frames[i], Math.Abs(fID - i) - 1));
                    fID = i;
                    thrIndex++;
                }
                if (thrIndex == mDisplayWidth) break;
            }

            if (thrIndex != mDisplayWidth) {
                i = i - dir;
                if (fID != i) {
                    ret.Add(new Tuple<DataModel.Frame, int>(Dataset.Frames[i], Math.Abs(fID - i) - 1));
                }
            }
            return ret;
        }

        //private void pageTextBox_KeyUp(object sender, KeyEventArgs e) {
        //    int page = 0;
        //    if (e.Key == Key.Enter && int.TryParse(pageTextBox.Text, out page)) {
        //        Page = page;
        //        DisplayPage(Page);
        //    }
        //}

        public event FrameSelectionEventHandler AddingToSelectionEvent;
        public void RaiseAddingToSelectionEvent(DataModel.Frame selectedFrame) {
            AddingToSelectionEvent?.Invoke(selectedFrame);
        }

        public event FrameSelectionEventHandler RemovingFromSelectionEvent;
        public void RaiseRemovingFromSelectionEvent(DataModel.Frame selectedFrame) {
            RemovingFromSelectionEvent?.Invoke(selectedFrame);
        }

        public event SubmitSelectionEventHandler ResettingSelectionEvent;
        public void RaiseResettingSelectionEvent() {
            ResettingSelectionEvent?.Invoke();
        }

        //public event SubmitSelectionEventHandler SelectionColorSearchEvent;
        //public void RaiseSelectionColorSearchEvent()
        //{
        //    SelectionColorSearchEvent?.Invoke();
        //}

        public event SubmitSelectionEventHandler SelectionSemanticSearchEvent;
        public void RaiseSelectionSemanticSearchEvent() {
            SelectionSemanticSearchEvent?.Invoke();
        }

        public event FrameSelectionEventHandler DisplayingFrameVideoEvent;
        public void RaiseDisplayingFrameVideoEvent(DataModel.Frame selectedFrame) {
            DisplayingFrameVideoEvent?.Invoke(selectedFrame);
        }


        public event FrameSelectionEventHandler SubmittingToServerEvent;
        public void RaiseSubmittingToServerEvent(DataModel.Frame submittedFrame) {
            SubmittingToServerEvent?.Invoke(submittedFrame);
        }

        public void DisplaySelected() {
            Visibility = Visibility.Visible;
            if (RefillNeeded) {
                AggregateResult();
                DisplayPage(0, updateSelected: false);
            }
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            }
        }

        public void DisplayHidden() {
            Visibility = Visibility.Hidden;
        }

        public void SelectedFrameChanged(DataModel.Frame selectedFrame) {
            UpdateSelectionVisualization();
        }

        internal void UpdateDisplayGrid() {
            ClearAndResize();
            if (GlobalItemSelector.ActiveDisplay == this) {
                AggregateResult();
                if (GlobalItemSelector.SelectedFrame != null) {
                    SeekToFrame(GlobalItemSelector.SelectedFrame);
                } else {
                    DisplayPage(0);
                }
            }
        }

        private void nextPageButton_Click(object sender, RoutedEventArgs e) {
            DisplayPage(Page + 1);
        }

        private void previousPageButton_Click(object sender, RoutedEventArgs e) {
            DisplayPage(Page - 1);
        }

        private void firstPageButton_Click(object sender, RoutedEventArgs e) {
            DisplayPage(0);
        }

        private void lastPageButton_Click(object sender, RoutedEventArgs e) {
            DisplayPage(mResultFrames.Count);
        }

        private void timelinesPerRowUp_Click(object sender, RoutedEventArgs e) {
            TimelinesPerRow += 1;
            Size = DEFAULT_SIZE;
            AggregateResult();
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            } else {
                DisplayPage(0);
            }
        }

        private void timelinesPerRowDown_Click(object sender, RoutedEventArgs e) {
            if (TimelinesPerRow == 1) return;
            Size = DEFAULT_SIZE;
            TimelinesPerRow -= 1;
            AggregateResult();
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            } else {
                DisplayPage(0);
            }
        }

        private void columsUp_Click(object sender, RoutedEventArgs e) {
            int colums = ColumsPerTimeline;
            if (colums >= 19) return;
            while (ColumsPerTimeline == colums) {
                Size++;
            }

            AggregateResult();
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            } else {
                DisplayPage(0);
            }
        }

        private void columsDown_Click(object sender, RoutedEventArgs e) {
            int colums = ColumsPerTimeline;
            if (colums <= 3) return;
            while (ColumsPerTimeline == colums) {
                Size--;
            }

            AggregateResult();
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            } else {
                DisplayPage(0);
            }
        }

        public void IncrementDisplay(int pages) {
            DisplayPage(Page + pages);
        }

        public void GoToPage(int page) {
            if (page == int.MaxValue) {
                page = mResultFrames.Count;
            }
            DisplayPage(page);
        }
    }
}
