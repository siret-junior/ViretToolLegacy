using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    class VideoAggregateFilter : FlowFilter
    {
        // TODO: check constraints
        public int MaxGroupsPerVideo { get; set; }

        public VideoAggregateFilter(DataModel.Dataset dataset) : base(dataset)
        {
            MaxGroupsPerVideo = int.MaxValue;
        }

        public override List<RankedFrame> ApplyFilter(List<RankedFrame> rankedFrames)
        {
            int[] groupHitCounter = new int[mDataset.Groups.Count];
            List<RankedFrame> filteredResult = new List<RankedFrame>(rankedFrames.Count);

            for (int i = 0; i < rankedFrames.Count; i++)
            {
                int groupId = rankedFrames[i].Frame.FrameGroup.GroupID;
                if (groupHitCounter[groupId] < MaxGroupsPerVideo)
                {
                    filteredResult.Add(rankedFrames[i]);
                    groupHitCounter[groupId]++;
                }
            }

            return filteredResult;
        }


    }
}
