using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ViretTool.BasicClient
{
    public class CompetitionScenePlayer
    {
        private Button mButton;
        private DataModel.Dataset mDataset;
        private DataModel.Frame mSearchedFrame = null;
        private List<DataModel.Frame> mSearchedFrames = null;
        private System.Windows.Threading.DispatcherTimer mDispatcherTimer;
        private int mSceneLength;

        public CompetitionScenePlayer(DataModel.Dataset dataset, Button button, int sceneLength)
        {
            mDataset = dataset;
            mButton = button;
            mButton.Click += Button_Click;
            mSceneLength = sceneLength;

            mDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            mDispatcherTimer.Tick += ShowImage;
            mDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
        }

        public bool IsSceneFound(DataModel.Frame frame)
        {
            if (frame.ParentVideo.Id == mSearchedFrame.ParentVideo.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            DataModel.Video v = mDataset.Videos[r.Next() % mDataset.Videos.Count];

            mSearchedFrame = v.Frames[r.Next() % (v.Frames.Count - 2)];

            List<DataModel.Frame> frames = mSearchedFrame.ParentVideo.ParentDataset.GetAllExtractedFrames(mSearchedFrame.ParentVideo.Id);

            mSearchedFrames = new List<DataModel.Frame>();
            for (int i = 0; i < frames.Count; i++)
            {
                if ((frames[i].FrameNumber >= mSearchedFrame.FrameNumber - mSceneLength / 2)
                    && (frames[i].FrameNumber <= mSearchedFrame.FrameNumber + mSceneLength / 2))
                    mSearchedFrames.Add(frames[i]);
            }

            mTickCounter = r.Next() % mSearchedFrames.Count();
            mDispatcherTimer.Start();
        }

        public void Reset()
        {
            mDispatcherTimer.Stop();
            mButton.Content = null;
        }

        private int mTickCounter;
        private void ShowImage(object sender, EventArgs e)
        {
            mTickCounter = mTickCounter % mSearchedFrames.Count();
            mButton.Content = new Image
            {
                Source = mSearchedFrames[mTickCounter].Bitmap,
                VerticalAlignment = VerticalAlignment.Top
            };
            mTickCounter++;
        }


        private const int TASK_DURATION_SECONDS = 5 * 60;

        private int EvaluateEffort(int secondsRemaining, int nTries)
        {
            return (int)Math.Max(0, (50 + 50 * ((double)secondsRemaining / TASK_DURATION_SECONDS) - nTries * 10));
        }
    }
}
