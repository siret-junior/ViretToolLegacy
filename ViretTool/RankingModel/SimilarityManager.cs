//#define LEGACY

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

#if LEGACY
        private readonly ByteVectorModel mVectorModel; // TODO: add generic vector model
#else
        private readonly FloatVectorModel mVectorModel; // TODO: add generic vector model
#endif
        private List<RankedFrame> mVectorBasedRanking;

        // TODO - keyword based ranking model
        private readonly KeywordModel mKeywordModel;
        private List<RankedFrame> mKeywordBasedRanking;


        public SimilarityManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;

#if LEGACY
            mVectorModel = new ByteVectorModel(mDataset);        
#else
            mVectorModel = new FloatVectorModel(mDataset);
#endif
            mColorSignatureModel = new ColorSignatureModel(mDataset);
            mKeywordModel = new KeywordModel(mDataset, new string[] {
                "GoogLeNet", "YFCC100M"
            });

            Reset();
        }

        public List<RankedFrame> KeywordBasedRanking
        {
            get { return mKeywordBasedRanking; }
        }

        public List<RankedFrame> ColorSketchBasedRanking
        {
            get { return mColorSignatureBasedRanking; }
        }

        public List<RankedFrame> VectorBasedRanking
        {
            get { return mVectorBasedRanking; }
        }

        public void UpdateColorModelRanking(List<Tuple<Point, Color, Point, bool>> queryCentroids)
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
            List<RankedFrame> rankedFrames = new List<RankedFrame>();

            foreach (DataModel.Video v in mDataset.Videos)
            {
                int idx = v.Frames.Count / 2;

                if (v.Frames.Count > 6) idx = 3 + (random.Next() % (v.Frames.Count - 6));

                rankedFrames.Add(new RankedFrame(v.Frames[idx], random.Next()));
            }

            return rankedFrames;
        }

        public List<RankedFrame> GenerateSequentialRanking()
        {
            return GenerateSequentialRanking(mDataset.Frames);
        }

        public List<RankedFrame> GenerateSequentialRanking(List<DataModel.Frame> filteredFrames)
        {
            List<RankedFrame> sequentialRanking = new List<RankedFrame>();

            foreach (DataModel.Frame f in filteredFrames)
                sequentialRanking.Add(new RankedFrame(f, f.ID));

            return sequentialRanking;
        }

        // TODO: different aggregation strategies (sum, max)
        public List<RankingModel.RankedFrame> GetMaxNormalizedFinalRanking(List<DataModel.Frame> frames, bool keywordBasedRanking, bool colorSignatureBasedRanking, bool vectorBasedRanking)
        {
            // rankings are kept max-normalized

            // TODO: different aggregation strategies
            List<RankingModel.RankedFrame> resultRanking = AggregateRankingSum(frames, keywordBasedRanking, colorSignatureBasedRanking, vectorBasedRanking);
            
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

        private List<RankedFrame> AggregateRankingSum(List<DataModel.Frame> frames, bool keywordBasedRanking, bool colorSignatureBasedRanking, bool vectorBasedRanking)
        {
            List<List<RankedFrame>> rankingListsForSorting = new List<List<RankedFrame>>();

            if (mColorSignatureBasedRanking != null && colorSignatureBasedRanking)
                rankingListsForSorting.Add(mColorSignatureBasedRanking);

            if (mVectorBasedRanking != null && vectorBasedRanking)
                rankingListsForSorting.Add(mVectorBasedRanking);

            if (mKeywordBasedRanking != null && keywordBasedRanking)
                rankingListsForSorting.Add(mKeywordBasedRanking);

            if (rankingListsForSorting.Count == 0)
                return GenerateSequentialRanking(frames);

            List<RankedFrame> aggregatedResult = RankedFrame.InitializeResultList(frames); ;

            Parallel.For(0, aggregatedResult.Count, index =>
            {
                RankedFrame rf = aggregatedResult[index];

                // TODO: multipliers and vector instructions
                foreach (List<RankedFrame> list in rankingListsForSorting)
                    rf.Rank += list[rf.Frame.ID].Rank;
            });

            return aggregatedResult;
        }

        
    }
}
