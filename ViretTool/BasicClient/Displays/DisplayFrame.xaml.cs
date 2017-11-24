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

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for DisplayFrame.xaml
    /// </summary>
    public partial class DisplayFrame : UserControl
    {
        public DisplayControl ParentDisplay { get; private set; }

        private DataModel.Frame mFrame = null;
        public DataModel.Frame Frame
        {
            get
            { return mFrame; }
            set
            {
                mFrame = value;
                if (value != null)
                {
                    image.Source = mFrame.Bitmap;
                    //label.Content = mFrame.ID.ToString();
                    //label.Content = mFrame.FrameVideo.VideoID;
                    //labelFrame.Content = mFrame.FrameNumber;
                }
                else
                {
                    image.Source = null;
                    //label.Content = null;
                }
                mVideoFrames = null;
            }
        }

        private bool mIsSelected = false;
        public bool IsSelected
        {
            get
            { return mIsSelected; }
            set
            {
                // update GUI only if value was changed
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    UpdateSelectionVisualization();
                }
            }
        }

        // TODO: check memory leaking (dangling pointers, events, etc...)
        private DataModel.Frame[] mVideoFrames = null;
        public DataModel.Frame[] VideoFrames
        {
            get
            { return mVideoFrames; }
            set
            {
                mVideoFrames = value;

                // do not display any video frames
                mDisplayedVideoFrameId = -1;
            }
        }
        private int mDisplayedVideoFrameId = -1;

        public DisplayFrame(DisplayControl parentDisplay)
        {
            InitializeComponent();
            ParentDisplay = parentDisplay;
        }

        public DisplayFrame()
        {
            InitializeComponent();
            ParentDisplay = null;
        }


        private int ComputeClosestVideoFrameId()
        {
            if (VideoFrames == null)
            {
                return -1;
            }
            else
            {
                // find video frame closest to the displayed frame
                for (int i = 0; i < VideoFrames.Length; i++)
                {
                    mDisplayedVideoFrameId = i;
                    DataModel.Frame videoFrame = VideoFrames[i];
                    if (videoFrame.FrameNumber > mFrame.FrameNumber)
                    {
                        break;
                    }
                }
                return mDisplayedVideoFrameId;
            }
        }

        private void DisplayVideoFrame(int videoFrameId)
        {
            // null check
            if (VideoFrames == null || VideoFrames.Length == 0)
            {
                return;
            }

            // range check 0..videoFrameCount
            if (videoFrameId >= VideoFrames.Length)
            {
                videoFrameId = VideoFrames.Length - 1;
            }
            else if (videoFrameId < 0)
            {
                videoFrameId = 0;
            }
            mDisplayedVideoFrameId = videoFrameId;

            // set frame and content
            image.Source = VideoFrames[mDisplayedVideoFrameId].Bitmap;
            //label.Content = VideoFrames[mDisplayedVideoFrameId].FrameNumber;
        }

        private void ResetDisplayVideoFrame()
        {
            mDisplayedVideoFrameId = -1;
            
            // restore the selected frame image
            if (Frame != null && image.Source != Frame.Bitmap)
            {
                image.Source = Frame.Bitmap;
                //label.Content = mFrame.FrameNumber;
            }
        }

        private void UpdateSelectionVisualization()
        {
            switch (mIsSelected)
            {
                case false:
                    rectangle.Stroke = Brushes.Transparent;
                    break;

                case true:
                    rectangle.Stroke = Brushes.Lime;
                    break;
            }
        }

        private void ToggleSelection()
        {
            if (mIsSelected == false)
            {
                Select();
            }
            else
            {
                Deselect();
            }
        }


        private void Select()
        {
            if (Frame != null)
            {
                ParentDisplay.RaiseAddingToSelectionEvent(Frame);
            }
        }

        private void Deselect()
        {
            if (Frame != null)
            {
                ParentDisplay.RaiseRemovingFromSelectionEvent(Frame);
            }
        }


        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: reconfigurable buttons
            if (Frame != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed && ParentDisplay != null)
                {
                    ParentDisplay.RaiseResettingSelectionEvent();
                    Select();
                    ParentDisplay.RaiseDisplayingFrameVideoEvent(Frame);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    ToggleSelection();
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // video scrolling when middle button pressed
            if (Frame != null)
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    Point point = Mouse.GetPosition(this);

                    // read selected keyframes (obsolete)
                    //DataModel.Video video = Frame.FrameVideo;

                    // read all video frames (lazy)
                    if (mVideoFrames == null)
                    {
                        mVideoFrames = Frame.FrameVideo.VideoDataset.ReadAllVideoFrames(Frame.FrameVideo);
                    }

                    mDisplayedVideoFrameId = (int)((point.X / ActualWidth) * (mVideoFrames.Length - 1));
                    image.Source = mVideoFrames[mDisplayedVideoFrameId].Bitmap;
                }
                //// the original frame image otherwise
                //else if (image.Source != Frame.Bitmap)
                //{
                //    image.Source = Frame.Bitmap;
                //}
            }

            // display buttons
            if (Frame != null)
            {
                displayButtons.Visibility = Visibility.Visible;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            // restore the selected frame image
            ResetDisplayVideoFrame();

            // hide buttons
            displayButtons.Visibility = Visibility.Hidden;
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO: find better solution: 
            // disable scrolling on video display
            if (ParentDisplay is VideoDisplay)
            {
                return;
            }

            // read all video frames (lazy)
            if (VideoFrames == null)
            {
                VideoFrames = Frame.FrameVideo.VideoDataset.ReadAllVideoFrames(Frame.FrameVideo);
            }

            if (mDisplayedVideoFrameId == -1)
            {
                mDisplayedVideoFrameId = ComputeClosestVideoFrameId();
            }

            if (e.Delta > 0)
            {
                DisplayVideoFrame(mDisplayedVideoFrameId - 1);
            }
            else
            {
                DisplayVideoFrame(mDisplayedVideoFrameId + 1);
            }
        }

        private void colorSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Select();
            ParentDisplay.RaiseSelectionColorSearchEvent();
        }

        private void semanticSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Select();
            ParentDisplay.RaiseSelectionSemanticSearchEvent();
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            // get displayed frame
            DataModel.Frame submittedFrame = Frame; ;
            if (mDisplayedVideoFrameId != -1)
            {
                // a video frame is shown and submitted
                submittedFrame = VideoFrames[mDisplayedVideoFrameId];
            }

            ParentDisplay.RaiseSubmittingToServerEvent(submittedFrame);
        }
    }
}
