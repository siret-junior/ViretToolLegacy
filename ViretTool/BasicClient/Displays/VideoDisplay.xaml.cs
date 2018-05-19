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
using ViretTool.DataModel;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for VideoDisplay.xaml
    /// </summary>
    public partial class VideoDisplay : DisplayControl
    {
        
        private int mDisplayWidth = 16;
        private int maxFramesToDisplay = 16 * 10;
        
        public VideoDisplay() : base(true)
        {
            InitializeComponent();
            ResizeDisplay(1, 1, displayGrid);
        }

        internal void SelectedFrameChanged(DataModel.Frame selectedFrame) {
            UpdateSelectionVisualization();
        }

        public void DisplayFrameVideo(DataModel.Frame frame)
        {
            // skip if nothing to show
            if (frame == null)
            {
                return;
            }

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
                DisplayedFrames[i].Frame = framesToDisplay[i];
                if (DisplayedFrames[i].Frame == frame) {
                    DisplayedFrames[i].IsGlobalSelectedFrame = true;
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

            for (int i = 0; i < maxFrames; i++)
            {
                int index = (int)(((double)i / maxFrames) * frames.Count);

                if (!wasSourceFrameAdded && frames[index].FrameNumber >= sourceFrame.FrameNumber)
                {
                    result.Add(sourceFrame);
                    wasSourceFrameAdded = true;
                    continue;
                }

                result.Add(frames[index]);
            }
            
            return result;
        }
    }
}
