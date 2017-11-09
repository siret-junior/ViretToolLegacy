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
using static ViretTool.BasicClient.FrameSelectionController;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for DisplayFrame.xaml
    /// </summary>
    public partial class DisplayFrame : UserControl
    {
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

        // TODO: consider event handlers
        public FrameSelectionController FrameSelectionController { get; set; }
        
        public VideoDisplay VideoDisplay { get; set; }

        public DisplayFrame()
        {
            InitializeComponent();
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
            IsSelected = true;
            FrameSelectionController.AddToSelection(Frame);
        }

        private void Deselect()
        {
            IsSelected = false;
            FrameSelectionController.RemoveFromSelection(Frame);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: reconfigurable buttons
            if (Frame != null)
            {
                if (VideoDisplay != null)
                {
                    VideoDisplay.DisplayFrames(Frame.FrameVideo);
                }

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    ToggleSelection();
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Select();
                    FrameSelectionController.SubmitSelection();
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
                    DataModel.Video video = Frame.FrameVideo;
                    int frameNumber = (int)((point.X / ActualWidth) * (video.Frames.Count - 1));
                    image.Source = video.Frames[frameNumber].Bitmap;
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
            if (Frame != null && image.Source != Frame.Bitmap)
            {
                image.Source = Frame.Bitmap;
            }
        }
    }
}
