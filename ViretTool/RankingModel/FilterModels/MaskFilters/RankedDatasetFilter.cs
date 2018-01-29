using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.FilterModels.MaskFilters
{
    class RankedDatasetFilter : MaskFilter
    {
        private int mSampleSize;
        private int[] mSampleIndexes;
        private double[] mSampleValues;

        public RankedDatasetFilter(Dataset dataset) : base(dataset, new bool[dataset.Frames.Count])
        {
            mSampleSize = 1000;
            mSampleIndexes = new int[mSampleSize];
            mSampleValues = new double[mSampleSize];

            Random r = new Random(10);
            for (int i = 0; i < mSampleSize; i++)
                mSampleIndexes[i] = r.Next() % dataset.Frames.Count();
        }

        public void SetMaskTo(List<RankedFrame> unsortedRankedFrames, double percentageOfDatabase)
        {
            // estimate a rank value threshold for a given percentageOfDatabase
            Parallel.For(0, mSampleSize, i =>
                mSampleValues[i] = unsortedRankedFrames[mSampleIndexes[i]].Rank);

            Array.Sort(mSampleValues);
            double threshold = mSampleValues[(int)(mSampleSize * percentageOfDatabase)];

            // set mask using the threshold
            bool[] mask = Mask;
            Parallel.For(0, mask.Length, i => 
                mask[i] = unsortedRankedFrames[i].Rank > threshold);

            Mask = mask; // sets also inverted mask
        }

    }
}
