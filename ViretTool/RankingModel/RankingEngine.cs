﻿using System;
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

        private List<RankedFrame> mFilteredRankedSortedResult;

        public delegate void RankedResultEventHandler(List<RankedFrame> rankedResult);
        public event RankedResultEventHandler RankingChangedEvent;

        private double mPercentageOfDatabaseKeyword = 0.5;
        private double mPercentageOfDatabaseColor = 0.975;
        private double mPercentageOfDatabaseSemantic = 0.5;

        // TODO - set to false once updated from GUI
        private bool mSortByKeyword = false;
        private bool mSortByColor = true;
        private bool mSortBySemantic = true;

        public RankingEngine(SimilarityManager similarityManager, FilterManager filterManager)
        {
            mSimilarityManager = similarityManager;
            mFilterManager = filterManager;
        }

        public void ResetEngine()
        {
            mSimilarityManager.Reset();
            
            // TODO
        }

        public void GenerateRandomRanking()
        {
            RaiseRankingChangedEvent(mSimilarityManager.GenerateRandomRanking());
        }

        public void GenerateSequentialRanking()
        {
            RaiseRankingChangedEvent(mSimilarityManager.GenerateSequentialRanking());
        }

        /// <summary>
        /// The main ranking process, assumes updated models and filters. Invoked after each update.
        /// </summary>
        private void ComputeFilteredRankedSortedResult()
        {
            List<DataModel.Frame> filteredFrames = mFilterManager.GetMaskFilteredFrames();

            List<RankedFrame> aggregatedRankingResult =
                mSimilarityManager.GetMaxNormalizedFinalRanking(filteredFrames, SortByKeyword, SortByColor, SortBySemantic);

            RaiseRankingChangedEvent(mFilterManager.SortAndApplyVideoAggregateFilter(aggregatedRankingResult));
        }

        private void RaiseRankingChangedEvent(List<RankedFrame> result)
        { 
            mFilteredRankedSortedResult = result;

            mSimilarityManager.FillSimilarityDescriptors(mFilteredRankedSortedResult);

            RankingChangedEvent?.Invoke(mFilteredRankedSortedResult);
        }

        #region --[ Sort properties ]--

        public bool SortByKeyword {
            get { return mSortByKeyword; }
            set { mSortByKeyword = value;
                    ComputeFilteredRankedSortedResult(); }
        }

        public bool SortByColor {
            get { return mSortByColor; }
            set { mSortByColor = value;
                    ComputeFilteredRankedSortedResult(); }
        }

        public bool SortBySemantic {
            get { return mSortBySemantic; }
            set { mSortBySemantic = value;
                ComputeFilteredRankedSortedResult(); }
        }

        #endregion

        #region --[ Set flow filters ]--

        public bool VideoAggregateFilterEnabled
        {
            get
            { return mFilterManager.VideoAggregateFilterEnabled; }
            set
            { mFilterManager.VideoAggregateFilterEnabled = value; ComputeFilteredRankedSortedResult(); }
        }

        public int VideoAggregateFilterMaxFrames
        {
            get
            { return mFilterManager.VideoAggregateFilterMaxFrames; }
            set
            { mFilterManager.VideoAggregateFilterMaxFrames = value; ComputeFilteredRankedSortedResult(); }
        }

        #endregion

        #region --[ Set threshold filters]--

        public void SetBlackAndWhiteFilter(bool enable, bool useInvertedMask)
        {
            mFilterManager.SetBlackAndWhiteFilter(enable, useInvertedMask);
            ComputeFilteredRankedSortedResult();
        }

        public void SetBlackAndWhiteFilterMask(float maxAllowedDeltaRGB)
        {
            mFilterManager.SetBlackAndWhiteFilterMask(maxAllowedDeltaRGB);
            ComputeFilteredRankedSortedResult();
        }

        public void SetPercentageOfBlackColorFilter(bool enable, bool useInvertedMask)
        {
            mFilterManager.SetPercentageOfBlackColorFilter(enable, useInvertedMask);
            ComputeFilteredRankedSortedResult();
        }

        public void SetPercentageOfBlackColorFilterMask(float maxAllowedPercentageOfBlackColor)
        {
            mFilterManager.SetPercentageOfBlackColorFilterMask(maxAllowedPercentageOfBlackColor);
            ComputeFilteredRankedSortedResult();
        }

        #endregion

        #region --[ Set rank dataset filters ]--
        public void SetFilterThresholdForKeywordModel(double percentageOfDatabase)
        {
            mPercentageOfDatabaseKeyword = percentageOfDatabase;

            if (mPercentageOfDatabaseKeyword == 0 || mSimilarityManager.KeywordBasedRanking == null)
                mFilterManager.KeywordRankingFilterEnabled = false;
            else
            {
                mFilterManager.KeywordRankingFilterEnabled = true;
                mFilterManager.SetKeywordRankingFilterMask(mSimilarityManager.KeywordBasedRanking, percentageOfDatabase);   
            }

            ComputeFilteredRankedSortedResult();
        }

        public void SetFilterThresholdForColorModel(double percentageOfDatabase)
        {
            mPercentageOfDatabaseColor = percentageOfDatabase;

            if (mPercentageOfDatabaseColor == 0 || mSimilarityManager.ColorSketchBasedRanking == null)
                mFilterManager.ColorRankingFilterEnabled = false;
            else
            {
                mFilterManager.ColorRankingFilterEnabled = true;
                mFilterManager.SetColorRankingFilterMask(mSimilarityManager.ColorSketchBasedRanking, percentageOfDatabase);
            }

            ComputeFilteredRankedSortedResult();
        }

        public void SetFilterThresholdForSemanticModel(double percentageOfDatabase)
        {
            mPercentageOfDatabaseSemantic = percentageOfDatabase;

            if (mPercentageOfDatabaseSemantic == 0 || mSimilarityManager.VectorBasedRanking == null)
                mFilterManager.VectorRankingFilterEnabled = false;
            else
            {
                mFilterManager.VectorRankingFilterEnabled = true;
                mFilterManager.SetVectorRankingFilterMask(mSimilarityManager.VectorBasedRanking, percentageOfDatabase);
            }

            ComputeFilteredRankedSortedResult();
        }

        #endregion

        #region --[ Update queries ]--

        public void UpdateKeywordModelRankingAndFilterMask(List<List<int>> queryKeyword, string source)
        {
            mSimilarityManager.UpdateKeywordModelRanking(queryKeyword, source);

            SetFilterThresholdForKeywordModel(mPercentageOfDatabaseKeyword);
        }

        public void UpdateColorModelRankingAndFilterMask(List<Tuple<Point, Color, Point, bool>> colorModelSketchQuery)
        {
            mSimilarityManager.UpdateColorModelRanking(colorModelSketchQuery);

            SetFilterThresholdForColorModel(mPercentageOfDatabaseColor);
        }

        public void UpdateVectorModelRankingAndFilterMask(List<DataModel.Frame> queryFrames, bool onlyUpdateCache)
        {
            mSimilarityManager.UpdateVectorModelRanking(queryFrames);

            if (!onlyUpdateCache)
                SetFilterThresholdForSemanticModel(mPercentageOfDatabaseSemantic);
        }

        #endregion

     }
}
