using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SequentialDisplay.xaml
    /// </summary>
    public partial class SequentialDisplay : DisplayControl, IMainDisplay {

        public Dataset Dataset;
        public RankingModel.SimilarityModels.FloatVectorModel SimilarityModel;

        public SequentialDisplay() : base(true) {
            InitializeComponent();
            ResizeDisplay(1, 1, frameGrid);
        }

        public static readonly DependencyProperty PageProperty = DependencyProperty.Register("Page", typeof(int), typeof(SequentialDisplay), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register("Threshold", typeof(double), typeof(SequentialDisplay), new FrameworkPropertyMetadata(0.3d));

        public int Page {
            get { return (int)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }
        
        public double Threshold {
            get { return (double)GetValue(ThresholdProperty); }
            //set { SetValue(ThresholdProperty, value); }
        }

        public int FramesPerPage {
            get { return DisplayedFrames.Length; }
        }

        private Dictionary<int, int> Map = new Dictionary<int, int>();
        private List<Tuple<DataModel.Frame, int>> Results = new List<Tuple<DataModel.Frame, int>>();

        public void Aggregate() {
            int i = 0;
            int pos = 0;
            while (i < Dataset.Frames.Count) {
                if (!Map.ContainsKey(Dataset.Frames[i].ID)) {
                    DataModel.Frame thisFrame = Dataset.Frames[i];
                    Map.Add(thisFrame.ID, pos);

                    int fID = thisFrame.ID;
                    int vID = thisFrame.FrameVideo.VideoID;

                    i++;
                    while (i < Dataset.Frames.Count && Dataset.Frames[i].FrameVideo.VideoID == vID) {
                        double d = RankingModel.SimilarityModels.FloatVectorModel.ComputeDistance(
                            SimilarityModel.mFloatVectors[fID], SimilarityModel.mFloatVectors[i]);

                        if (Threshold > d) {
                            Map.Add(i, pos);
                            i++;
                        } else {
                            break;
                        }
                    }
                    Results.Add(new Tuple<DataModel.Frame, int>(thisFrame, Math.Abs(fID - i) - 1));
                    pos++;
                } else {
                    i++;
                }
            }
        }

        public void DisplayPage(int page, bool updateSelected = true) {
            if (page < 0) {
                page = 0;
            }
            if ((page + 1) * FramesPerPage > Results.Count) {
                page = (Results.Count - 1) / FramesPerPage;
            }

            int offset = page * FramesPerPage;
            int count = (Results.Count - offset < FramesPerPage) ? Results.Count - offset : FramesPerPage;
            Page = page;

            if (updateSelected) {
                GlobalItemSelector.SelectedFrame = Results[offset].Item1;
            }
            int j = 0;
            for (int i = offset; i < offset + count; i++, j++) {
                DisplayedFrames[j].Set(Results[i].Item1, skippedFrames:Results[i].Item2);
            }
            for (; j < DisplayedFrames.Length; j++) {
                DisplayedFrames[j].Clear();
            }
            UpdateSelectionVisualization();
        }

        private void SeekToFrame(DataModel.Frame selectedFrame) {
            int framePosition = Map[selectedFrame.ID];
            GlobalItemSelector.SelectedFrame = Results[framePosition].Item1;

            DisplayPage(framePosition / FramesPerPage, updateSelected: false);
        }


        const double DESIRED_ASPECT_RATIO = 3.0 / 4.0;
        int nColumns = 10;

        public void UpdateDisplayGrid() {
            double desiredFrameHeight = frameGrid.ActualWidth / nColumns * DESIRED_ASPECT_RATIO;
            int nRows = (int)((frameGrid.ActualHeight + desiredFrameHeight / 2) / desiredFrameHeight);

            if (nRows != mDisplayRows || nColumns != mDisplayCols) {
                ResizeDisplay(nRows, nColumns, frameGrid);

                if (GlobalItemSelector.SelectedFrame != null) {
                    SeekToFrame(GlobalItemSelector.SelectedFrame);
                } else {
                    DisplayPage(0);
                }
            }
        }


        public void DisplaySelected() {
            Visibility = Visibility.Visible;
            if (GlobalItemSelector.SelectedFrame != null) {
                SeekToFrame(GlobalItemSelector.SelectedFrame);
            } else {
                DisplayPage(0, updateSelected: false);
            }
        }

        public void DisplayHidden() {
            Visibility = Visibility.Hidden;
        }

        public void IncrementDisplay(int pages) {
            DisplayPage(Page + pages);
        }

        public void GoToPage(int page) {
            if (page == int.MaxValue) {
                page = Results.Count;
            }
            DisplayPage(page);
        }

        public void SelectedFrameChanged(DataModel.Frame selectedFrame) {
            UpdateSelectionVisualization();
        }

        private void frameGrid_Loaded(object sender, RoutedEventArgs e) {
            UpdateDisplayGrid();
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
            DisplayPage(Results.Count);
        }
    }
}
