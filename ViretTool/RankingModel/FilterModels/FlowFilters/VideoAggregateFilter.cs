using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.FilterModels
{
    class VideoAggregateFilter : FlowFilter
    {
        // TODO: check constraints
        public int MaxFramesPerVideo { get; set; }

        private bool mVideoFilterEnabled = true;
        private HashSet<int> mVideoFilterHashset = new HashSet<int>();

        public VideoAggregateFilter(DataModel.Dataset dataset) : base(dataset)
        {
            MaxFramesPerVideo = int.MaxValue;
        }

        public override List<RankedFrame> ApplyFilter(List<RankedFrame> rankedFrames)
        {
            //int[] groupHitCounter = new int[mDataset.Groups.Count];
            int[] videoHitCounter = new int[mDataset.Videos.Count];
            List<RankedFrame> filteredResult = new List<RankedFrame>(rankedFrames.Count);

            for (int i = 0; i < rankedFrames.Count; i++)
            {
                RankedFrame rankedFrame = rankedFrames[i];
                int groupId = rankedFrame.Frame.FrameGroup.GroupID;
                int videoId = rankedFrame.Frame.FrameVideo.VideoID;

                if (videoHitCounter[videoId] < MaxFramesPerVideo
                    //&& (groupHitCounter[groupId] < 1)
                    && (!mVideoFilterEnabled || !mVideoFilterHashset.Contains(rankedFrame.Frame.FrameVideo.VideoID)))
                {
                    filteredResult.Add(rankedFrames[i]);
                    //groupHitCounter[groupId]++;
                    videoHitCounter[videoId]++;
                }
            }

            return filteredResult;
        }


        public void AddVideoToFilterList(int videoId)
        {
            mVideoFilterHashset.Add(videoId);
        }

        public void AddVideoToFilterList(Video video)
        {
            mVideoFilterHashset.Add(video.VideoID);
        }

        public void EnableVideoFilter()
        {
            mVideoFilterEnabled = true;
        }
        public void DisableVideoFilter()
        {
            mVideoFilterEnabled = false;
        }

        public void ResetVideoFilter()
        {
            mVideoFilterHashset.Clear();
        }

    }
}
