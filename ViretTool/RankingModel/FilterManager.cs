using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.RankingModel.FilterModels.MaskFilters;

namespace ViretTool.RankingModel.FilterModels
{
    public class FilterManager
    {
        private DataModel.Dataset mDataset;

        private VideoAggregateFilter mVideoAggregateFilter;
        // TODO filters - BW, % of black pixels, shot length

        private RankedDatasetFilter mKeywordRankingFilter;
        private RankedDatasetFilter mColorRankingFilter;
        private RankedDatasetFilter mVectorRankingFilter;

        private ThresholdFilter mBlackAndWhiteFilter;
        private ThresholdFilter mPercentageOfBlackColorFilter;

        public FilterManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mVideoAggregateFilter = new VideoAggregateFilter(mDataset);
            //mAspectRatioFilter = new AspectRatioFilter(mDataset);

            mKeywordRankingFilter = new RankedDatasetFilter(mDataset);
            mColorRankingFilter = new RankedDatasetFilter(mDataset);
            mVectorRankingFilter = new RankedDatasetFilter(mDataset);

            // TODO - use constants for filenames
            string bwfilterFileName = dataset.GetFileNameByExtension(".bwfilter");
            string pbcfilterFileName = dataset.GetFileNameByExtension(".pbcfilter");
            mBlackAndWhiteFilter = new ThresholdFilter(mDataset, bwfilterFileName);
            mPercentageOfBlackColorFilter = new ThresholdFilter(mDataset, pbcfilterFileName);
        }

        public void Clear()
        {
            
        }

        public List<DataModel.Frame> GetMaskFilteredFrames()
        {
            // apply mask filters
            List<bool[]> maskFilters = new List<bool[]>();

            if (mKeywordRankingFilter.Enabled) maskFilters.Add(mKeywordRankingFilter.Mask);
            if (mColorRankingFilter.Enabled) maskFilters.Add(mColorRankingFilter.Mask);
            if (mVectorRankingFilter.Enabled) maskFilters.Add(mVectorRankingFilter.Mask);

            if (mBlackAndWhiteFilter.Enabled) maskFilters.Add(mBlackAndWhiteFilter.GetActualMask());
            if (mPercentageOfBlackColorFilter.Enabled) maskFilters.Add(mPercentageOfBlackColorFilter.GetActualMask());

            if (maskFilters.Count == 0) return mDataset.Frames;

            List<DataModel.Frame> filteredFrames = new List<DataModel.Frame>();
            List<DataModel.Frame> allFrames = mDataset.Frames;

            bool[] filtered = MaskFilter.AggregateMasks(mDataset.Frames.Count, maskFilters);
            for (int i = 0; i < filtered.Length; i++)
                if (filtered[i]) filteredFrames.Add(allFrames[i]);
            
            return filteredFrames;
        }

        public List<RankedFrame> SortAndApplyVideoAggregateFilter(List<RankedFrame> rankedFrames)
        {
            // TODO - sort in parallel?
            rankedFrames.Sort();

            // Flow filters (filtering on sorted ranking) 
            // TODO: parallel pipeline
            if (mVideoAggregateFilter.Enabled)
                rankedFrames = mVideoAggregateFilter.ApplyFilter(rankedFrames);

            return rankedFrames;
        }

        // WARNING NOT USED !!!
        public List<RankedFrame> ApplyFiltersAndSort(List<RankedFrame> unfilteredRankedFrames)
        {
            // apply mask filters
            List<bool[]> maskFilters = new List<bool[]>();

            if (mKeywordRankingFilter.Enabled) maskFilters.Add(mKeywordRankingFilter.Mask);
            if (mColorRankingFilter.Enabled) maskFilters.Add(mColorRankingFilter.Mask);
            if (mVectorRankingFilter.Enabled) maskFilters.Add(mVectorRankingFilter.Mask);

            if (mBlackAndWhiteFilter.Enabled) maskFilters.Add(mBlackAndWhiteFilter.GetActualMask());
            if (mPercentageOfBlackColorFilter.Enabled) maskFilters.Add(mPercentageOfBlackColorFilter.GetActualMask());

            List<RankedFrame> filteredFrames = unfilteredRankedFrames;

            if (maskFilters.Count > 0)
            {
                bool[] filtered = MaskFilter.AggregateMasks(mDataset.Frames.Count, maskFilters);
                filteredFrames = new List<RankedFrame>();
                for (int i = 0; i < filtered.Length; i++)
                    if (filtered[i]) filteredFrames.Add(unfilteredRankedFrames[i]);
            }

            // sort mask filtered result - TODO - sort in parallel?
            filteredFrames.Sort();

            // Flow filters (filtering on sorted ranking) 
            // TODO: parallel pipeline
            if (mVideoAggregateFilter.Enabled)
            {
                filteredFrames = mVideoAggregateFilter.ApplyFilter(filteredFrames);
            }

            return filteredFrames;
        }

        #region --[ FlowFilters ]--
        
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

        #endregion

        #region --[ RankedDatasetFilters ]--

        public void SetKeywordRankingFilterMask(List<RankedFrame> unsortedRankedFrames, double percentageOfDatabase)
        {
            mKeywordRankingFilter.SetMaskTo(unsortedRankedFrames, percentageOfDatabase);
        }

        public bool KeywordRankingFilterEnabled
        {
            get
            { return mKeywordRankingFilter.Enabled; }
            set
            { mKeywordRankingFilter.Enabled = value; }
        }

        public void SetColorRankingFilterMask(List<RankedFrame> unsortedRankedFrames, double percentageOfDatabase)
        {
            mColorRankingFilter.SetMaskTo(unsortedRankedFrames, percentageOfDatabase);
        }

        public bool ColorRankingFilterEnabled
        {
            get
            { return mColorRankingFilter.Enabled; }
            set
            { mColorRankingFilter.Enabled = value; }
        }

        public void SetVectorRankingFilterMask(List<RankedFrame> unsortedRankedFrames, double percentageOfDatabase)
        {
            mVectorRankingFilter.SetMaskTo(unsortedRankedFrames, percentageOfDatabase);
        }

        public bool VectorRankingFilterEnabled
        {
            get
            { return mVectorRankingFilter.Enabled; }
            set
            { mVectorRankingFilter.Enabled = value; }
        }

        #endregion

        #region --[ ThresholdFilters ]--

        public bool SetBlackAndWhiteFilter(bool enable, bool useInvertedMask)
        {
            if (mBlackAndWhiteFilter.Enabled == enable && mBlackAndWhiteFilter.UseInvertedMask == useInvertedMask) return false;

            mBlackAndWhiteFilter.Enabled = enable;
            mBlackAndWhiteFilter.UseInvertedMask = useInvertedMask;
            return true;
        }

        public bool SetBlackAndWhiteFilterMask(float maxAllowedDeltaRGB)
        {
            if (mBlackAndWhiteFilter.Threshold == maxAllowedDeltaRGB) return false;

            mBlackAndWhiteFilter.SetMaskTo(maxAllowedDeltaRGB);
            return true;
        }

        public bool SetPercentageOfBlackColorFilter(bool enable, bool useInvertedMask)
        {
            if (mPercentageOfBlackColorFilter.Enabled == enable && mPercentageOfBlackColorFilter.UseInvertedMask == useInvertedMask) return false;

            mPercentageOfBlackColorFilter.Enabled = enable;
            mPercentageOfBlackColorFilter.UseInvertedMask = useInvertedMask;
            return true;
        }

        public bool SetPercentageOfBlackColorFilterMask(float maxAllowedPercentageOfBlackColor)
        {
            if (mPercentageOfBlackColorFilter.Threshold == maxAllowedPercentageOfBlackColor) return false;

            mPercentageOfBlackColorFilter.SetMaskTo(maxAllowedPercentageOfBlackColor);
            return true;
        }

        #endregion

    }
}
