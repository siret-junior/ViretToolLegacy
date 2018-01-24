using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ViretTool.RankingModel.FilterModels;
using ViretTool.RankingModel.SimilarityModels;

namespace ViretTool.RankingModel
{
    /// <summary>
    /// Storing UI state and providing a facade
    /// TODO: consider locks
    /// </summary>
    public class RankingEngine
    {
        private readonly SimilarityManager mSimilarityManager;
        private readonly FilterManager mFilterManager;

        private List<RankedFrame> mRankedSimilarityResult;
        private List<RankedFrame> mRankedFilteredSortedResult;

        public delegate void RankedResultEventHandler(List<RankedFrame> rankedResult);
        public event RankedResultEventHandler RankingChangedEvent;
        
        public RankingEngine(SimilarityManager similarityManager, FilterManager filterManager)
        {
            mSimilarityManager = similarityManager;
            mFilterManager = filterManager;
        }


        private List<RankedFrame> ComputeSimilarityRanking()
        {
            // cache intermediate ranking result for reusing when changing just filtering
            mRankedSimilarityResult = mSimilarityManager.GetMaxNormalizedAndSortedFinalRanking();
            return mRankedSimilarityResult;
        }

        private List<RankedFrame> ComputeFilteringAndSorting()
        {
            // TODO: mask filters vs. flow filters

            // ranked result is reused, only filtering is applied
            mRankedFilteredSortedResult = mFilterManager.ApplyFilters(mRankedSimilarityResult);

            // fill similarity descriptors
            mSimilarityManager.FillSimilarityDescriptors(mRankedFilteredSortedResult);

            RankingChangedEvent?.Invoke(mRankedFilteredSortedResult);
            return mRankedFilteredSortedResult;
        }

        private List<RankedFrame> ComputeRankingFilteringAndSorting()
        {
            ComputeSimilarityRanking();
            ComputeFilteringAndSorting();

            return mRankedFilteredSortedResult;
        }


        #region --[ Similarity model facade ]--

        // TODO: removed (will be reimplemented in GUI?)
        // toggle "use model" switches are implemented using properties
        //public bool UseColorModel { get; set; }
        //public bool UseVectorModel { get; set; }
        //public bool UseKeywordModel { get; set; }

        public List<RankedFrame> UpdateColorModelRanking(List<Tuple<Point, Color, Point, bool>> colorModelSketchQuery)
        {
            // store current sketch query (TODO remove?)
            //mColorModelSketchQuery = colorModelSketchQuery;
            
            // update model ranking
            mSimilarityManager.UpdateColorModelRanking(colorModelSketchQuery);
            // recompute aggregation, filtering and sorting
            List<RankedFrame> result = ComputeRankingFilteringAndSorting();
            return result;
        }
        
        public List<RankedFrame> UpdateColorModelRanking(List<DataModel.Frame> queryFrames)
        {
            // TODO: clone selected frames?

            // update model ranking
            mSimilarityManager.UpdateColorModelRanking(queryFrames);
            // recompute aggregation, filtering and sorting
            List<RankedFrame> result = ComputeRankingFilteringAndSorting();
            return result;
        }

        public List<RankedFrame> UpdateVectorModelRanking(List<DataModel.Frame> queryFrames)
        {
            // TODO: clone selected frames?

            // update model ranking
            mSimilarityManager.UpdateVectorModelRanking(queryFrames);
            // recompute aggregation, filtering and sorting
            List<RankedFrame> result = ComputeRankingFilteringAndSorting();
            return result;
        }

        public List<RankedFrame> UpdateKeywordModelRanking(List<List<int>> queryKeyword, string source)
        {
            // update model ranking
            mSimilarityManager.UpdateKeywordModelRanking(queryKeyword, source);
            // recompute aggregation, filtering and sorting
            List<RankedFrame> result = ComputeRankingFilteringAndSorting();
            return result;
        }

        public List<RankedFrame> GenerateRandomRanking()
        {
            mRankedSimilarityResult = mSimilarityManager.GenerateRandomRanking();
            List<RankedFrame> result = ComputeFilteringAndSorting();
            return result;
        }

        public List<RankedFrame> GenerateSequentialRanking()
        {
            mRankedSimilarityResult = mSimilarityManager.GenerateSequentialRanking();
            List<RankedFrame> result = ComputeFilteringAndSorting();
            return result;
        }

        #endregion


        #region --[ Filter facade ]--

        // TODO: change to methods

        public bool VideoAggregateFilterEnabled
        {
            get
            { return mFilterManager.VideoAggregateFilterEnabled; }
            set
            { mFilterManager.VideoAggregateFilterEnabled = value; }
        }

        public int VideoAggregateFilterMaxFrames
        {
            get
            { return mFilterManager.VideoAggregateFilterMaxFrames; }
            set
            { mFilterManager.VideoAggregateFilterMaxFrames = value; }
        }

        // TODO:
        // toggle filter: B/W

        // toggle filter: aspect ratio

        // toggle filter: examined

        // filter: N per video

        #endregion


    }
}
