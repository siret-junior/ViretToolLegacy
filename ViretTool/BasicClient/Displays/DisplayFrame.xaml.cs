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
                    label.Content = mFrame.ID.ToString();
                }
                else
                {
                    image.Source = null;
                    label.Content = null;
                }
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

        public DisplayFrame(DisplayControl parentDisplay)
        {
            InitializeComponent();
            ParentDisplay = parentDisplay;
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
            ParentDisplay.RaiseAddingToSelectionEvent(Frame);
        }

        private void Deselect()
        {
            ParentDisplay.RaiseRemovingFromSelectionEvent(Frame);
        }


        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: reconfigurable buttons
            if (Frame != null)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    // select
                    ToggleSelection();
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // submit selection
                    Select();
                    ParentDisplay.RaiseSubmittingSelectionEvent();
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    // show video in video display
                    ParentDisplay.RaiseDisplayingFrameVideoEvent(Frame);
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

                    int frameIndex = (int)((point.X / ActualWidth) * (mVideoFrames.Length - 1));
                    image.Source = mVideoFrames[frameIndex].Bitmap;
                }
                // the original frame image otherwise
                else if (image.Source != Frame.Bitmap)
                {
                    image.Source = Frame.Bitmap;
                }
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            // restore the selected frame image
            if (Frame != null && image.Source != Frame.Bitmap)
            {
                image.Source = Frame.Bitmap;
            }
        }
    }
}
