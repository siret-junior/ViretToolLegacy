using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using ViretTool.InteractionLogging;
using System.Windows.Media;

namespace ViretTool.BasicClient
{
    public class RandomScenePlayer
    {
        private Button mButton;
        private DataModel.Dataset mDataset;
        private DataModel.Frame mSearchedFrame = null;
        private List<DataModel.Frame> mSearchedFrames = null;
        private System.Windows.Threading.DispatcherTimer mDispatcherTimer;
        private System.Windows.Threading.DispatcherTimer mTimeRemainingTimer;
        private int mSceneLength;

        public Button TimeButton { get; set; }
        public Button ScoreButton { get; set; }
        public Button AvgScoreButton { get; set; }
        List<int> scores = new List<int>();

        public RandomScenePlayer(DataModel.Dataset dataset, Button button, int sceneLength)
        {
            mDataset = dataset;
            mButton = button;
            mButton.Click += Button_Click;
            mSceneLength = sceneLength;

            mDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            mDispatcherTimer.Tick += ShowImage;
            mDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            mTimeRemainingTimer = new System.Windows.Threading.DispatcherTimer();
            mTimeRemainingTimer.Tick += UpdateTimer;
            mTimeRemainingTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);

        }

        public string ReturnSearchedItemPosition(List<RankingModel.RankedFrame> resultList)
        {
            string result = "";
            if (mSearchedFrame != null)
            {
                // find first video frame
                for (int i = 0; i < resultList.Count; i++)
                    if (resultList[i].Frame.ParentVideo.Id == mSearchedFrame.ParentVideo.Id)
                    {
                        result = "video: " + i; break;
                    }

                // find first group frame
                for (int i = 0; i < resultList.Count; i++)
                    if (resultList[i].Frame.ParentGroup.Id == mSearchedFrame.ParentGroup.Id)
                    {
                        result += ", group: " + i; break;
                    }
                // try to find the frame if not filtered
                for (int i = 0; i < resultList.Count; i++)
                    if (resultList[i].Frame.ParentVideo.Id == mSearchedFrame.ParentVideo.Id)
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
            secondsLeft = TASK_DURATION_SECONDS;
            SetTimeButton(secondsLeft);
            TimeButton.Background = Brushes.Black;
            mTimeRemainingTimer.Start();

            Random r = new Random();
            DataModel.Video v = mDataset.Videos[r.Next() % mDataset.Videos.Count];

            mSearchedFrame = v.Frames[r.Next() % (v.Frames.Count - 2)];
            InteractionLogger.Instance.LogInteraction("task", "start",
                "V(" + mSearchedFrame.ParentVideo.Id + "), F(" + mSearchedFrame.IdInVideo + ")");

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

            InteractionLogger.Instance.ResetLog();
        }

        private void SetTimeButton(int secondsLeft)
        {
            if (secondsLeft >= 0)
            {
                int minutes = secondsLeft / 60;
                int seconds = secondsLeft % 60;
                if (TimeButton != null)
                {
                    TimeButton.Content = "Time: " + minutes.ToString("0") + ":" + seconds.ToString("00")
                    + ", score: " + EvaluateEffort(secondsLeft, 0).ToString("000");
                }
            }

            if (secondsLeft == 0)
            {
                mTimeRemainingTimer.Stop();
                TimeButton.Background = Brushes.DarkRed;
            }
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            secondsLeft--;
            SetTimeButton(secondsLeft);

            if (secondsLeft == 0)
            {
                mTimeRemainingTimer.Stop();
            }
        }

        public void Reset()
        {
            mDispatcherTimer.Stop();
            mTimeRemainingTimer.Stop();
            mButton.Content = null;
            scores.Clear();
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


        public void Submit(int videoId, int frameNumber)
        {
            int searchedVideoId = mSearchedFrames[0].ParentVideo.Id;
            int searchedFrameNumberStart = mSearchedFrames[0].FrameNumber;
            int searchedFrameNumberEnd = mSearchedFrames[mSearchedFrames.Count - 1].FrameNumber;

            if (videoId == searchedVideoId
                && frameNumber >= searchedFrameNumberStart
                && frameNumber <= searchedFrameNumberEnd)
            {
                mTimeRemainingTimer.Stop();
                TimeButton.Background = Brushes.DarkGreen;
                ScoreButton.Content += " 0,";
            }
        }


        private const int TASK_DURATION_SECONDS = 5 * 60;
        private int secondsLeft = TASK_DURATION_SECONDS;

        private int EvaluateEffort(int secondsRemaining, int nTries)
        {
            return (int)Math.Max(0, (50 + 50 * ((double)secondsRemaining / TASK_DURATION_SECONDS) - nTries * 10));
        }

    }
}