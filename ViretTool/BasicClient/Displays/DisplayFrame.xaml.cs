#define LABELS

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
#if LABELS
                    videoLabel.Content = mFrame.ParentVideo.Id.ToString();
                    //groupLabel.Content = mFrame.FrameGroup.GroupID.ToString();
                    frameLabel.Content = mFrame.Id.ToString();
#endif
                }
                else
                {
                    image.Source = null;
#if LABELS
                    videoLabel.Content = null;
                    groupLabel.Content = null;
                    frameLabel.Content = null;
#endif
                }
                mVideoFrames = null;
                LeftScrollHelper.Visibility = Visibility.Hidden;
                RightScrollHelper.Visibility = Visibility.Hidden;
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

        public static readonly RoutedEvent OnEnterEvent = EventManager.RegisterRoutedEvent("OnEnter", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DisplayFrame));
        public static readonly RoutedEvent OnExitEvent = EventManager.RegisterRoutedEvent("OnExit", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DisplayFrame));

        public event RoutedEventHandler OnEnter {
            add { AddHandler(OnEnterEvent, value); }
            remove { RemoveHandler(OnEnterEvent, value); }
        }

        public event RoutedEventHandler OnExit {
            add { AddHandler(OnExitEvent, value); }
            remove { RemoveHandler(OnExitEvent, value); }
        }

        private void viewBox_MouseEnter(object sender, MouseEventArgs e) {
            if (ParentDisplay == null) return;
            RoutedEventArgs evargs = new RoutedEventArgs(OnEnterEvent, this);
            RaiseEvent(evargs);
        }

        private void viewBox_MouseLeave(object sender, MouseEventArgs e) {
            if (ParentDisplay == null) return;
            RoutedEventArgs evargs = new RoutedEventArgs(OnExitEvent, this);
            RaiseEvent(evargs);
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
        

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: reconfigurable buttons
            if (Frame != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed && ParentDisplay != null)
                {
                    ParentDisplay.RaiseDisplayingFrameVideoEvent(Frame);
                }
            }
        }


        #region --[ Inside frame browsing ]--

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

            if (ParentDisplay == null) return;
            RoutedEventArgs evargs = new RoutedEventArgs(OnEnterEvent, VideoFrames[mDisplayedVideoFrameId]);
            RaiseEvent(evargs);
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


        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // video scrolling when middle button pressed
            if (Frame != null)
            {
                Point point = Mouse.GetPosition(this);

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    // read all video frames (lazy)
                    if (mVideoFrames == null)
                    {
                        mVideoFrames = Frame.ParentVideo.ParentDataset.ReadAllVideoFrames(Frame.ParentVideo);
                    }

                    mDisplayedVideoFrameId = (int)((point.X / ActualWidth) * (mVideoFrames.Length - 1));
                    if (mDisplayedVideoFrameId < 0) { mDisplayedVideoFrameId = 0; }
                    image.Source = mVideoFrames[mDisplayedVideoFrameId].Bitmap;
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    // read selected keyframes
                    if (mVideoFrames == null)
                    {
                        mVideoFrames = Frame.ParentVideo.Frames.ToArray();
                    }

                    mDisplayedVideoFrameId = (int)((point.X / ActualWidth) * (mVideoFrames.Length - 1));
                    image.Source = mVideoFrames[mDisplayedVideoFrameId].Bitmap;
                }
            }

            // display buttons
            if (Frame != null && ParentDisplay != null)
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
            LeftScrollHelper.Visibility = Visibility.Hidden;
            RightScrollHelper.Visibility = Visibility.Hidden;
        }

       
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO: find better solution: 
            // disable scrolling on video display
            if (ParentDisplay is VideoDisplay || Frame == null)
            {
                return;
            }

            // read all video frames (lazy)
            if (VideoFrames == null)
            {
                DataModel.Frame[] frames = Frame.ParentVideo.ParentDataset.ReadAllVideoFrames(Frame.ParentVideo);

                // show every second
                VideoFrames = new DataModel.Frame[frames.Length / 2];
                for (int i = 0; i < frames.Length / 2; i++)
                {
                    VideoFrames[i] = frames[i * 2];
                }
            }

            if (mDisplayedVideoFrameId == -1)
            {
                mDisplayedVideoFrameId = ComputeClosestVideoFrameId();
            }

            if (e.Delta > 0)
            {
                DisplayVideoFrame(mDisplayedVideoFrameId - 1);
                LeftScrollHelper.Visibility = Visibility.Visible;
                RightScrollHelper.Visibility = Visibility.Hidden;
            }
            else
            {
                DisplayVideoFrame(mDisplayedVideoFrameId + 1);
                RightScrollHelper.Visibility = Visibility.Visible;
                LeftScrollHelper.Visibility = Visibility.Hidden;
            }
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
                    if (videoFrame.FrameNumber >= mFrame.FrameNumber)
                    {
                        break;
                    }
                }
                return mDisplayedVideoFrameId;
            }
        }


        #endregion


        #region --[ Display buttons ]--

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                ParentDisplay.RaiseAddingToSelectionEvent(Frame);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                ParentDisplay.RaiseRemovingFromSelectionEvent(Frame);
            }
        }

        private void AddSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                ParentDisplay.RaiseAddingToSelectionEvent(Frame);
                ParentDisplay.RaiseSelectionSemanticSearchEvent();
            }
        }

        private void RemoveSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                ParentDisplay.RaiseRemovingFromSelectionEvent(Frame);
                ParentDisplay.RaiseSelectionSemanticSearchEvent();
            }
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
#endregion
    }
}
