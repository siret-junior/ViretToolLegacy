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
    public partial class VideoDisplay : DisplayControl
    {
        
        private int mDisplayWidth = 16;

        
        public VideoDisplay()
        {
            InitializeComponent();
            ResizeDisplay(1, 1, displayGrid);
        }


        public void DisplayFrameVideo(DataModel.Frame frame)
        {
            // skip if nothing to show
            if (frame == null)
            {
                return;
            }
            DataModel.Video video = frame.FrameVideo;
            // TODO move to separate method
            // clear display
            for (int i = 0; i < DisplayedFrames.Length; i++)
            {
                DisplayedFrames[i].Frame = null;
            }

            // resize display
            int nRows = ((video.Frames.Count - 1) / mDisplayWidth) + 1;
            int nCols = mDisplayWidth;
            ResizeDisplay(nRows, nCols, displayGrid);

            // display frames
            for (int i = 0; i < video.Frames.Count; i++)
            {
                DisplayedFrames[i].Frame = video.Frames[i];
            }

            UpdateSelectionVisualization();
        }
    }
}
