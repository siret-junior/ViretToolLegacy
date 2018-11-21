﻿using System;
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
        public int MaxGroupsPerVideo { get; set; }

        private bool mVideoFilterEnabled = true;
        private HashSet<int> mVideoFilterHashset = new HashSet<int>();

        public VideoAggregateFilter(DataModel.Dataset dataset) : base(dataset)
        {
            MaxGroupsPerVideo = int.MaxValue;
        }

        public override List<RankedFrame> ApplyFilter(List<RankedFrame> rankedFrames)
        {
            int[] groupHitCounter = new int[mDataset.Groups.Count];
            int[] videoHitCounter = new int[mDataset.Groups.Count];
            List<RankedFrame> filteredResult = new List<RankedFrame>(rankedFrames.Count);

            for (int i = 0; i < rankedFrames.Count; i++)
            {
                RankedFrame rankedFrame = rankedFrames[i];
                int groupId = rankedFrame.Frame.ParentGroup.Id;
                int videoId = rankedFrame.Frame.ParentVideo.Id;

                if (videoHitCounter[videoId] < MaxGroupsPerVideo &&
                    (groupHitCounter[groupId] < 1)
                    && (!mVideoFilterEnabled || !mVideoFilterHashset.Contains(rankedFrame.Frame.ParentVideo.Id)))
                {
                    filteredResult.Add(rankedFrames[i]);
                    groupHitCounter[groupId]++;
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
            mVideoFilterHashset.Add(video.Id);
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
