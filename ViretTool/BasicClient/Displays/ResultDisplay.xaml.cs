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
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class ResultDisplay : UserControl, INotifyPropertyChanged
    {
        const double DESIRED_ASPECT_RATIO = 3.0 / 4.0;

        public DataModel.Dataset Dataset { get; set; }
        public RankingEngine RankingEngine { get; set; }

        private FrameSelectionController mFrameSelectionController;
        public FrameSelectionController FrameSelectionController
        {
            set
            {
                mFrameSelectionController = value;
                
                // pass the controller instance to all displayed frame controls
                for (int i = 0; i < mDisplayFrames.Length; i++)
                {
                    mDisplayFrames[i].FrameSelectionController = mFrameSelectionController;
                }

                // update display after every selection change
                mFrameSelectionController.SelectionChangedEvent += UpdateSelection;
            }
        }

        private VideoDisplay mVideoDisplay;
        public VideoDisplay VideoDisplay
        {
            get
            { return mVideoDisplay; }
            set
            {
                mVideoDisplay = value;
                
                // pass the video display instance to all displayed frame controls
                for (int i = 0; i < mDisplayFrames.Length; i++)
                {
                    mDisplayFrames[i].VideoDisplay = VideoDisplay;
                }
            }
        }

        private List<RankedFrame> mResultFrames = null;
        public List<RankedFrame> ResultFrames
        {
            get
            { return mResultFrames; }
            set
            {
                if (value == null || value.Count == 0)
                {
                    return;
                }
                mResultFrames = value;
                DisplayPage(0);
                UpdateSelection();
            }
        }

        private DisplayFrame[]/*[]*/ mDisplayFrames = null;
        
        private int mDisplayCols;
        private int mDisplayRows;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int mPage = 0;
        public string PageNumberLabel
        {
            get
            { return mPage.ToString(); }
            set
            {
                NotifyPropertyChanged("PageNumberLabel");
            }
        }

        Random random = new Random();


        public ResultDisplay()
        {
            InitializeComponent();
            DataContext = this;
            ResizeDisplay(1, 1);
        }

        private void RecomputeDisplaySize()
        {
            // TODO: custom nColumns from the GUI
            int nColumns = 8;
            double desiredFrameHeight = displayGrid.ActualWidth / nColumns * DESIRED_ASPECT_RATIO;
            int nRows = (int)((displayGrid.ActualHeight + desiredFrameHeight / 2) / desiredFrameHeight);

            ResizeDisplay(nRows, nColumns);

            // TODO: recompute correct page
            DisplayPage(0);
        }


        private void ResizeDisplay(int nRows, int nCols)
        {
            // setup display grid
            mDisplayRows = nRows;
            mDisplayCols = nCols;
            int displaySize = nRows * nCols;
            
            displayGrid.Columns = mDisplayCols;
            displayGrid.Rows = mDisplayRows;

            // create and fill new displayed frames
            mDisplayFrames = new DisplayFrame[displaySize];
            displayGrid.Children.Clear();
            for (int i = 0; i < displaySize; i++)
            {
                DisplayFrame displayedFrame = new DisplayFrame();
                mDisplayFrames[i] = displayedFrame;
                displayedFrame.FrameSelectionController = mFrameSelectionController;
                displayedFrame.VideoDisplay = VideoDisplay;
                displayGrid.Children.Add(displayedFrame);
            }
        }

        private void UpdateSelection()
        {
            foreach (DisplayFrame displayedFrame in mDisplayFrames)
            {
                if (mFrameSelectionController.SelectedFrames.Contains(displayedFrame.Frame))
                {
                    displayedFrame.IsSelected = true;
                }
                else
                {
                    displayedFrame.IsSelected = false;
                }
            }
        }

        private void EmptyDisplay()
        {
            for (int i = 0; i < mDisplayFrames.Length; i++)
            {
                mDisplayFrames[i].Frame = null;
            }
        }

        private void DisplayPage(int page)
        {
            // display check
            int displaySize = mDisplayFrames.Length;
            if (displaySize == 0)
            {
                return;
            }

            EmptyDisplay();

            // result check
            if (mResultFrames == null || mResultFrames.Count == 0)
            {
                return;
            }

            // range check 0..maxPage
            if (page * displaySize >= mResultFrames.Count)
            {
                mPage = (mResultFrames.Count - 1) / displaySize;
            }
            else if (page < 0)
            {
                mPage = 0;
            }
            else
            {
                mPage = page;
            }

            // update page label
            PageNumberLabel = "Page: " + (mPage + 1).ToString();

            // extract frame subset
            int offset = mPage * displaySize;
            int count = (mResultFrames.Count - offset < displaySize) ? mResultFrames.Count - offset : displaySize;
            count = (count > 0) ? count : 0;
            List <RankedFrame> framesToDisplay = mResultFrames.GetRange(offset, count);
            
            // TODO: semantic/color sorting of displayed items on a page

            // display frames
            for (int i = 0; i < count; i++)
            {
                mDisplayFrames[i].Frame = framesToDisplay[i].Frame;
            }
        }

        private void DisplayRandomItems()
        {
            mResultFrames.Clear();

            for (int i = 0; i < mDisplayFrames.Length; i++)
            {
                DataModel.Frame randomFrame = Dataset.Frames[random.Next(Dataset.Frames.Count - 1)];
                mResultFrames.Add(new RankedFrame(randomFrame, 0));
            }

            DisplayPage(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mPage++;
            DisplayPage(mPage);
        }

        private void displayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecomputeDisplaySize();
        }

        private void randomDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayRandomItems();
        }

        private void firstPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(0);
        }

        private void previousPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(mPage - 1);
        }

        private void nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(mPage + 1);
        }

        private void lastPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(mResultFrames.Count / mDisplayFrames.Length);
        }
    }
}
