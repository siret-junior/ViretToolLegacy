using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ViretTool.RankingModel.SimilarityModels
{
    /// <summary>
    /// Using color, vector and keyword models
    /// Rankings are kept max-normalized or empty (null)
    /// </summary>
    public class SimilarityManager
    {
        private readonly DataModel.Dataset mDataset;

        private readonly RankingModel.ColorSignatureModel mColorSignatureModel;
        private List<RankingModel.RankedFrame> mColorSignatureBasedRanking;

        private readonly RankingModel.DCNNFeatures.ByteVectorModel mVectorModel;
        private List<RankingModel.RankedFrame> mVectorBasedRanking;

        // TODO - keyword based ranking model
        private readonly RankingModel.DCNNKeywords.KeywordModel mKeywordModel;
        private List<RankingModel.RankedFrame> mKeywordBasedRanking;


        public SimilarityManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;

            mColorSignatureModel = new RankingModel.ColorSignatureModel(mDataset);
            mVectorModel = new RankingModel.DCNNFeatures.ByteVectorModel(mDataset);
            mKeywordModel = new RankingModel.DCNNKeywords.KeywordModel(mDataset, new string[] {
                "GoogLeNet", "YFCC100M"
            });

            Reset();
        }


        public void UpdateColorModelRanking(List<Tuple<Point, Color>> queryCentroids)
        {
            if (queryCentroids != null && queryCentroids.Count > 0)
            {
                mColorSignatureBasedRanking = mColorSignatureModel.RankFramesBasedOnSketch(queryCentroids);
                MaxNormalizeRanking(mColorSignatureBasedRanking);
            }
            else
            {
                mColorSignatureBasedRanking = null;
            }
        }

        public void UpdateColorModelRanking(List<DataModel.Frame> queryFrames)
        {
            if (queryFrames != null && queryFrames.Count > 0)
            {
                mColorSignatureBasedRanking = mColorSignatureModel.RankFramesBasedOnExampleFrames(queryFrames);
                MaxNormalizeRanking(mColorSignatureBasedRanking);
            }
            else
            {
                mColorSignatureBasedRanking = null;
            }
        }

        public void UpdateVectorModelRanking(List<DataModel.Frame> queryFrames)
        {
            if (queryFrames != null && queryFrames.Count > 0)
            {
                mVectorBasedRanking = mVectorModel.RankFramesBasedOnExampleFrames(queryFrames);
                MaxNormalizeRanking(mVectorBasedRanking);
            }
            else
            {
                mVectorBasedRanking = null;
            }
        }

        public void UpdateKeywordModelRanking(List<List<int>> queryKeyword, string source)
        {
                mKeywordBasedRanking = mKeywordModel.RankFramesBasedOnQuery(queryKeyword, source);
                if (mKeywordBasedRanking != null)
                    MaxNormalizeRanking(mKeywordBasedRanking);
        }
        

        // TODO: different aggregation strategies (sum, max)
        public List<RankingModel.RankedFrame> GetMaxNormalizedAndSortedFinalRanking()
        {
            // rankings are kept max-normalized

            // TODO: different aggregation strategies
            List<RankingModel.RankedFrame> resultRanking = AggregateRankingSum();
            
            return resultRanking;
        }
        

        public void Reset()
        {
            mColorSignatureBasedRanking = null;
            mVectorBasedRanking = null;
            mKeywordBasedRanking = null;
        }


        private void MaxNormalizeRanking(List<RankingModel.RankedFrame> ranking)
        {
            double maxRank = ranking.Select(x => x.Rank).Max();

            if (maxRank > 0)
            {
                Parallel.ForEach(ranking, rankedFrame =>
                {
                    rankedFrame.Rank /= maxRank;
                });
            }
            else if (maxRank == 0)
            {
                // TODO: not usual, log warning?
            }
            else // maxRank < 0
            {
                throw new ArithmeticException("Max rank is negative!");
            }
        }

        private List<RankingModel.RankedFrame> AggregateRankingSum()
        {
            List<RankingModel.RankedFrame> aggregatedResult = RankingModel.RankedFrame.InitializeResultList(mDataset);
               
            Parallel.For(0, aggregatedResult.Count, index =>
            {
                // TODO: multipliers
                if (mColorSignatureBasedRanking != null)
                {
                    aggregatedResult[index].Rank += mColorSignatureBasedRanking[index].Rank;
                }
                if (mVectorBasedRanking != null)
                {
                    aggregatedResult[index].Rank += mVectorBasedRanking[index].Rank;
                }
                if (mKeywordBasedRanking != null)
                {
                    aggregatedResult[index].Rank += mKeywordBasedRanking[index].Rank;
                }
            });

            return aggregatedResult;
        }

        
    }
}
