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

        public FilterManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mVideoAggregateFilter = new VideoAggregateFilter(mDataset);
        }


        public List<RankedFrame> ApplyFilters(List<RankedFrame> unfilteredFrames)
        {
            List<RankedFrame> filteredFrames = null;

            // TODO: unsorted mask filters

            // skip filtering if no filters are applied
            if (!mVideoAggregateFilter.Enabled /* TODO other filters */)
            {
                return unfilteredFrames;
            }

            // now flow filters (filtering on sorted ranking) 
            // (currently sorted inside VideoAggregateFilter)
            // unfilteredFrames.Sort();

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
