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
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for SemanticModelDisplay.xaml
    /// </summary>
    public partial class SemanticModelDisplay : UserControl
    {
        private int mDisplayCols;
        private int mDisplayRows;

        private int mColRatio;
        private int mRowRatio;
        

        private DisplayFrame[] mDisplayFrames = null;
        
        private FrameSelectionController mFrameSelectionController;
        public FrameSelectionController FrameSelectionController
        {
            set
            {
                mFrameSelectionController = value;
                
                // pass the controller instance to all displayed frame controls
                if (mDisplayFrames != null)
                {
                    foreach (DisplayFrame displayFrame in mDisplayFrames)
                    {
                        displayFrame.FrameSelectionController = value;
                    }
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

        public SemanticModelDisplay()
        {
            InitializeComponent();
            
            mColRatio = 3;
            mRowRatio = 2;
            FitDisplay(1);
        }


        public void DisplayFrames(List<DataModel.Frame> selectedFrames)
        {
            // TODO move to separate method
            // clear display
            for (int i = 0; i < mDisplayFrames.Length; i++)
            {
                mDisplayFrames[i].Frame = null;
            }

            // skip if nothing to show
            if (selectedFrames == null)
            {
                return;
            }

            FitDisplay(selectedFrames.Count);

            // display frames
            for (int i = 0; i < selectedFrames.Count; i++)
            {
                mDisplayFrames[i].Frame = selectedFrames[i];
            }
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
                displayGrid.Children.Add(displayedFrame);
            }
        }

        private void FitDisplay(int frameCount)
        {
            int cols = mColRatio;
            int rows = mRowRatio;

            // scale down
            while (cols - 1 >= mColRatio && rows - 1 >= mRowRatio
                && (cols - 1) * (rows - 1) >= frameCount)
            {
                cols--;
                rows--;
            }

            // scale up
            while ((cols) * (rows) < frameCount)
            {
                cols++;
                rows++;
            }

            // resize if needed
            if (cols != mDisplayCols || rows != mDisplayRows)
            {
                ResizeDisplay(rows, cols);
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

        private void semanticClearButton_Click(object sender, RoutedEventArgs e)
        {
            mFrameSelectionController.ResetSelection();
            mFrameSelectionController.SubmitSelection();
        }


        //// TODO use bindings and a custom converter
        //private void colRatioTextbox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    int number;
        //    if (int.TryParse(colRatioTextbox.Text, out number) && number < 32)
        //    {
        //        mColRatio = number;
        //    }
        //    else
        //    {
        //        mColRatio = 1;
        //        colRatioTextbox.Text = "";
        //    }
            
        //}

        //private void rowRatioTextbox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    int number;
        //    if (int.TryParse(rowRatioTextbox.Text, out number) && number < 32)
        //    {
        //        mRowRatio = number;
        //    }
        //    else
        //    {
        //        mRowRatio = 1;
        //        rowRatioTextbox.Text = "";
        //    }
        //}
    }
}
