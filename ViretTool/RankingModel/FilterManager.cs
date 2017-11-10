using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    public class FilterManager
    {
        private DataModel.Dataset mDataset;

        private VideoAggregateFilter mVideoAggregateFilter;
        private AspectRatioFilter mAspectRatioFilter;

        public FilterManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mVideoAggregateFilter = new VideoAggregateFilter(mDataset);
            //mAspectRatioFilter = new AspectRatioFilter(mDataset);
        }


        public List<RankedFrame> ApplyFilters(List<RankedFrame> unfilteredFrames)
        {
            List<RankedFrame> filteredFrames = null;

            // mask filters
            // TODO:

            // sort mask filtered result
            unfilteredFrames.Sort();
            filteredFrames = unfilteredFrames;

            // Flow filters (filtering on sorted ranking) 
            // TODO: parallel pipeline
            if (mVideoAggregateFilter.Enabled)
            {
                filteredFrames = mVideoAggregateFilter.ApplyFilter(unfilteredFrames);
            }

            return filteredFrames;
        }


        public bool VideoAggregateFilterEnabled
        {
            get
            { return mVideoAggregateFilter.Enabled; }
            set
            { mVideoAggregateFilter.Enabled = value; }

        }

        public int VideoAggregateFilterMaxFrames
        {
            get
            { return mVideoAggregateFilter.MaxFramesPerVideo; }
            set
            { mVideoAggregateFilter.MaxFramesPerVideo = value; }

        }



    }
}
