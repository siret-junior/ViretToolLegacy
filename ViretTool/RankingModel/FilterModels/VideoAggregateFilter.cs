using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    class VideoAggregateFilter : FilterBase
    {
        // TODO: check constraints
        public int MaxFramesPerVideo { get; set; }

        public VideoAggregateFilter(DataModel.Dataset dataset) 
            : base(dataset)
        {
            MaxFramesPerVideo = int.MaxValue;
        }

        //public override bool[] GetFilterMask(List<RankedFrame> rankedFrames)
        //{
        //    // TODO: move sorting
        //    rankedFrames.Sort();

        //    int[] hitCounter = new int[mDataset.Videos.Count];
        //    List<RankedFrame> filteredResult = new List<RankedFrame>(rankedFrames.Count);

        //    for (int i = 0; i < rankedFrames.Count; i++)
        //    {
        //        int videoId = rankedFrames[i].Frame.FrameVideo.VideoID;
        //        if (hitCounter[videoId] < MaxFramesPerVideo)
        //        {
        //            filteredResult.Add(rankedFrames[i]);
        //            hitCounter[videoId]++;
        //        }
        //    }

        //    return filteredResult;
        //}

        public override List<RankedFrame> ApplyFilter(List<RankedFrame> rankedFrames)
        {
            // TODO: move sorting
            rankedFrames.Sort();

            int[] hitCounter = new int[mDataset.Videos.Count];
            List<RankedFrame> filteredResult = new List<RankedFrame>(rankedFrames.Count);

            for (int i = 0; i < rankedFrames.Count; i++)
            {
                int videoId = rankedFrames[i].Frame.FrameVideo.VideoID;
                if (hitCounter[videoId] < MaxFramesPerVideo)
                {
                    filteredResult.Add(rankedFrames[i]);
                    hitCounter[videoId]++;
                }
            }

            return filteredResult;
        }


    }
}
