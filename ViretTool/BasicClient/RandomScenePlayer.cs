using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;

namespace ViretTool.BasicClient
{
    class RandomScenePlayer
    {
        private Button mButton;
        private DataModel.Dataset mDataset;
        private DataModel.Frame mSearchedFrame = null;
        private List<DataModel.Frame> mSearchedFrames = null;
        private System.Windows.Threading.DispatcherTimer mDispatcherTimer;
        private int mSceneLength;

        public RandomScenePlayer(DataModel.Dataset dataset, Button button, int sceneLength)
        {
            mDataset = dataset;
            mButton = button;
            mButton.Click += Button_Click;
            mSceneLength = sceneLength;

            mDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            mDispatcherTimer.Tick += ShowImage;
            mDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
        }

        public string ReturnSearchedItemPosition(List<RankingModel.RankedFrame> resultList)
        {
            string result = "";
            if (mSearchedFrame != null)
            {
                // find first video frame
                for (int i = 0; i < resultList.Count; i++)
                    if (resultList[i].Frame.FrameVideo.VideoID == mSearchedFrame.FrameVideo.VideoID)
                    {
                        result = "video: " + i; break;
                    }

                // find first group frame
                for (int i = 0; i < resultList.Count; i++)
                    if (resultList[i].Frame.FrameGroup.GroupID == mSearchedFrame.FrameGroup.GroupID)
                    {
                        result += ", group: " + i; break;
                    }
                // try to find the frame if not filtered
                for (int i = 0; i < resultList.Count; i++)
                    if (resultList[i].Frame.FrameVideo.VideoID == mSearchedFrame.FrameVideo.VideoID)
                        if ((resultList[i].Frame.FrameNumber >= mSearchedFrame.FrameNumber - mSceneLength / 2)
                            && (resultList[i].Frame.FrameNumber <= mSearchedFrame.FrameNumber + mSceneLength / 2))
                        {
                            result += ", frame:" + i; return result;
                        }

                result += ", frame filtered";
            }

            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            DataModel.Video v = mDataset.Videos[r.Next() % mDataset.Videos.Count];

            mSearchedFrame = v.Frames[r.Next() % (v.Frames.Count - 2)];

            List<DataModel.Frame> frames = mSearchedFrame.FrameVideo.GetAllExtractedFrames();

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

    }
}