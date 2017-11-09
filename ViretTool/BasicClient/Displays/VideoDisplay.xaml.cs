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
    /// Interaction logic for VideoDisplay.xaml
    /// </summary>
    public partial class VideoDisplay : UserControl
    {
        private int mDisplayCols;
        private int mDisplayRows;

        private int mDisplayWidth;

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

        public VideoDisplay()
        {
            InitializeComponent();
            mDisplayWidth = 8;
            ResizeDisplay(1, 1);
        }


        public void DisplayFrames(DataModel.Video video)
        {
            // TODO move to separate method
            // clear display
            for (int i = 0; i < mDisplayFrames.Length; i++)
            {
                mDisplayFrames[i].Frame = null;
            }

            // skip if nothing to show
            if (video == null)
            {
                return;
            }

            ResizeDisplay(((video.Frames.Count - 1) / mDisplayWidth) + 1, mDisplayWidth);

            // display frames
            for (int i = 0; i < video.Frames.Count; i++)
            {
                mDisplayFrames[i].Frame = video.Frames[i];
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
                displayedFrame.FrameSelectionController = mFrameSelectionController;
                mDisplayFrames[i] = displayedFrame;
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
    }
}
