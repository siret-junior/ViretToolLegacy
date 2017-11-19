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
        private Random random = new Random();

        private readonly ColorSignatureModel mColorSignatureModel;
        private List<RankedFrame> mColorSignatureBasedRanking;

        private readonly ByteVectorModel mVectorModel;
        private List<RankedFrame> mVectorBasedRanking;

        // TODO - keyword based ranking model
        private readonly KeywordModel mKeywordModel;
        private List<RankedFrame> mKeywordBasedRanking;


        public SimilarityManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;

            mColorSignatureModel = new ColorSignatureModel(mDataset);
            mVectorModel = new RankingModel.SimilarityModels.ByteVectorModel(mDataset);
            mKeywordModel = new RankingModel.SimilarityModels.KeywordModel(mDataset, new string[] {
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


        public List<RankedFrame> GenerateRandomRanking()
        {
            List<RankedFrame> randomRanking = RankedFrame.InitializeResultList(mDataset);

            for (int i = 0; i < randomRanking.Count; i++)
            {
                randomRanking[i].Rank = random.Next();
            }

            return randomRanking;
        }

        public List<RankedFrame> GenerateSequentialRanking()
        {
            List<RankedFrame> sequentialRanking = RankedFrame.InitializeResultList(mDataset);

            for (int i = 0; i < sequentialRanking.Count; i++)
            {
                sequentialRanking[i].Rank = sequentialRanking.Count - i;
            }

            return sequentialRanking;
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


        public void FillSimilarityDescriptors(List<RankedFrame> rankedFrames)
        {
            Parallel.For(0, rankedFrames.Count, i =>
            {
                rankedFrames[i].ColorSignature = mColorSignatureModel.GetFrameColorSignature(rankedFrames[i].Frame);
                rankedFrames[i].SemanticDescriptor = mVectorModel.GetFrameSemanticVector(rankedFrames[i].Frame);
            });
        }

        private void MaxNormalizeRanking(List<RankingModel.RankedFrame> ranking)
        {
            double maxRank;
            double minRank;

            FindMinimumAndMaximum(ranking, out maxRank, out minRank);

            // prepare offset and normalizer
            double offset = -minRank;
            double normalizer = (maxRank != minRank) 
                ? 1.0 / (maxRank - minRank) 
                : 0;

            // normalize to range [0..1]
            Parallel.For(0, ranking.Count, index =>
            {
                RankedFrame rankedFrame = ranking[index];

                rankedFrame.Rank += offset;
                rankedFrame.Rank *= normalizer;
            });
        }

        private static void FindMinimumAndMaximum(List<RankedFrame> list, out double maximum, out double minimum)
        {
            maximum = list[0].Rank;
            minimum = list[0].Rank;

            for (int i = 0; i < list.Count; i++)
            {
                double rank = list[i].Rank;
                maximum = (rank > maximum) ? rank : maximum;
                minimum = (rank < minimum) ? rank : minimum; 
            }
        }

        private List<RankingModel.RankedFrame> AggregateRankingSum()
        {
            List<RankingModel.RankedFrame> aggregatedResult;

            if (mColorSignatureBasedRanking == null
                && mVectorBasedRanking == null
                && mKeywordBasedRanking == null)
            {
                aggregatedResult = GenerateSequentialRanking();
            }
            else
            {
                aggregatedResult = RankingModel.RankedFrame.InitializeResultList(mDataset);
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
            }
            return aggregatedResult;
        }

        
    }
}
