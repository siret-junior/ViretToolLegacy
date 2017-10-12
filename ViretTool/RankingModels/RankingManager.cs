using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ViretTool.RankingModels
{
    class RankingManager
    {
        private readonly DataModel.Dataset mDataset;

        private readonly SimilarityModels.ColorSignatureModel mColorSignatureModel;
        private List<SimilarityModels.RankedFrame> mColorSignatureBasedRanking;

        private readonly SimilarityModels.DCNNFeatures.ByteVectorModel mVGGByteVectorModel;
        private List<SimilarityModels.RankedFrame> mVGGByteVectorBasedRanking;

        // TODO - keyword based ranking model

        public RankingManager(DataModel.Dataset dataset)
        {
            mDataset = dataset;

            mColorSignatureModel = new SimilarityModels.ColorSignatureModel(mDataset);
            mVGGByteVectorModel = new SimilarityModels.DCNNFeatures.ByteVectorModel(mDataset);
            // TODO - keyword based ranking

            Reset();
        }

        public void EvaluateRankingBasedOnSketch(List<Tuple<Point, Color>> queryCentroids)
        {
            mColorSignatureBasedRanking = mColorSignatureModel.RankFramesBasedOnSketch(queryCentroids);
        }

        public void EvaluateRankingBasedOnVGGByteVector(List<DataModel.Frame> queryFrames)
        {
            mVGGByteVectorBasedRanking = mVGGByteVectorModel.RankFramesBasedOnExampleFrames(queryFrames);
        }

        public List<SimilarityModels.RankedFrame> GetMaxNormalizedAndSortedFinalRanking(bool colorSketch, bool VGGvector)
        {
            List<SimilarityModels.RankedFrame> result = SimilarityModels.RankedFrame.InitializeResultList(mDataset);

            if (colorSketch && mColorSignatureBasedRanking != null)
            {
                double max = mColorSignatureBasedRanking.Select(x => x.Rank).Max();
                Parallel.For(0, result.Count, i =>
                    result[i].Rank += mColorSignatureBasedRanking[i].Rank / max);
            }

            if (VGGvector && mVGGByteVectorBasedRanking != null)
            {
                double max = mVGGByteVectorBasedRanking.Select(x => x.Rank).Max();
                Parallel.For(0, result.Count, i =>
                    result[i].Rank += mVGGByteVectorBasedRanking[i].Rank / max);
            }

            // TODO - use some parallel sorting
            result.Sort();

            // TODO - filter only top K or the most distinct frames from each video

            return result;
        }

        public void Reset()
        {
            mColorSignatureBasedRanking = null;
            mVGGByteVectorBasedRanking = null;
        }
    }
}
