﻿using System;
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
using ViretTool.BasicClient.Displays;
using ViretTool.DataModel;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for VideoDisplay.xaml
    /// </summary>
    public partial class VideoDisplay : DisplayControl {

        private int mDisplayWidth = 16;
        private int maxFramesToDisplay = 16 * 10;
        public MainWindow ParentWindow;
        private int LastVideoId = -1;

        public VideoDisplay() : base(true) {
            InitializeComponent();
            ResizeDisplay(1, 1, displayGrid);
        }

        private bool mFrameReductionSampled = false;
        public bool FrameReductionSampled {
            get {
                return mFrameReductionSampled;
            }
        }
        public bool FrameReductionDense {
            get {
                return !mFrameReductionSampled;
            }
        }

        internal void SelectedFrameChanged(DataModel.Frame selectedFrame) {
            UpdateSelectionVisualization();
        }

        DataModel.Frame LastFrame;

        public void DisplayFrameVideo(DataModel.Frame frame, bool doSwitch=true)
        {
            // skip if nothing to show
            if (frame == null)
            {
                return;
            }

            if (doSwitch && LastVideoId != frame.FrameVideo.VideoID) {
                mFrameReductionSampled = false;
                frameReductionDense.IsChecked = true;
                frameReductionSampled.IsChecked = false;
            }
            else if (LastFrame != null && Math.Abs(LastFrame.ID - frame.ID) < (mDisplayWidth - 1) / 2) {
                foreach (var item in DisplayedFrames) {
                    if (item.Frame == LastFrame) {
                        item.BringIntoView();
                        return;
                    }
                }
            }
            LastFrame = frame;

            LastVideoId = frame.FrameVideo.VideoID;

            List<DataModel.Frame> framesToDisplay = frame.FrameVideo.Frames;

            framesToDisplay = ReduceFrameSet(framesToDisplay, maxFramesToDisplay, frame);
            //DataModel.Frame[] allFrames = frame.FrameVideo.VideoDataset.ReadAllVideoFrames(frame.FrameVideo);
            //List<DataModel.Frame> framesToDisplay = new List<DataModel.Frame>(allFrames.Length / 8);
            //for (int i = 0; i < allFrames.Length; i += 8)
            //{
            //    framesToDisplay.Add(allFrames[i]);
            //}


            // TODO move to separate method
            // clear display
            for (int i = 0; i < DisplayedFrames.Length; i++)
            {
                DisplayedFrames[i].Clear();
            }

            // resize display
            int nRows = ((framesToDisplay.Count - 1) / mDisplayWidth) + 1;
            int nCols = mDisplayWidth;
            ResizeDisplay(nRows, nCols, displayGrid);

            // display frames
            for (int i = 0; i < framesToDisplay.Count; i++)
            {
                DisplayedFrames[i].Set(framesToDisplay[i]);
                if (DisplayedFrames[i].Frame == frame) {
                    DisplayedFrames[i].IsGlobalSelectedFrame = true;
                    DisplayedFrames[i].BringIntoView();
                }
            }
        }


        private List<DataModel.Frame> ReduceFrameSet(List<DataModel.Frame> frames, int maxFrames, DataModel.Frame sourceFrame)
        {
            if (frames.Count <= maxFrames)
            {
                return frames;
            }
            
            List<DataModel.Frame> result = new List<DataModel.Frame>();
            bool wasSourceFrameAdded = false;

            if (mFrameReductionSampled) {
                for (int i = 0; i < maxFrames; i++) {
                    int index = (int)(((double)i / maxFrames) * frames.Count);

                    if (!wasSourceFrameAdded && frames[index].FrameNumber >= sourceFrame.FrameNumber) {
                        result.Add(sourceFrame);
                        wasSourceFrameAdded = true;
                        continue;
                    }

                    result.Add(frames[index]);
                }
            } else {
                int firstFrame = Math.Max(frames[0].ID, sourceFrame.ID - maxFrames / 2);
                if (sourceFrame.ID - firstFrame > mDisplayWidth) {
                    int toRemove = (sourceFrame.ID - firstFrame) % mDisplayWidth + mDisplayWidth / 2;
                    firstFrame += toRemove;
                }
                int i = 0;
                while (frames[i].ID < firstFrame) i++;
                while (i < frames.Count && result.Count < maxFrames) {
                    result.Add(frames[i]);
                    i++;
                }
            }
            
            return result;
        }

        private void hideVideoDisplay_Click(object sender, RoutedEventArgs e) {
            var g = this.Parent as Grid;
            g.RowDefinitions[2].Height = new GridLength(23, GridUnitType.Pixel);
            g.UpdateLayout();

            ParentWindow.GridSplitter_DragCompleted(null, null);
        }

        private void FrameReductionSampled_Checked(object sender, RoutedEventArgs e) {
            mFrameReductionSampled = true;
            if (GlobalItemSelector.SelectedFrame != null) {
                DisplayFrameVideo(GlobalItemSelector.SelectedFrame);
            }
        }

        private void FrameReductionDense_Checked(object sender, RoutedEventArgs e) {
            mFrameReductionSampled = false;
            if (GlobalItemSelector.SelectedFrame != null) {
                DisplayFrameVideo(GlobalItemSelector.SelectedFrame, doSwitch:false);
            }
        }
    }
}
