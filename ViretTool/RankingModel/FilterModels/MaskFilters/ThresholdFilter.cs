using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels.MaskFilters
{
    class ThresholdFilter : MaskFilter
    {
        private float[] mFrameAttribute;

        public ThresholdFilter(DataModel.Dataset dataset, float[] frameAttribute) : base(dataset, new bool[dataset.Frames.Count])
        {
            mFrameAttribute = frameAttribute;
        }

        public bool UseInvertedMask { get; set; }

        public bool[] GetActualMask()
        {
            if (UseInvertedMask) return MaskInverted;
            else return Mask;
        }

        public ThresholdFilter(DataModel.Dataset dataset, string frameAttributeFileName) : base(dataset, new bool[dataset.Frames.Count])
        {
            if (!System.IO.File.Exists(frameAttributeFileName))
            {
                //if (dataset.UseOldDatasetID)
                //    return;

                throw new Exception("Filter was not created to " + frameAttributeFileName);
            }

            using (System.IO.BinaryReader BR = new System.IO.BinaryReader(System.IO.File.OpenRead(frameAttributeFileName)))
            {
                if (!mDataset.ReadAndCheckFileHeader(BR))
                    throw new Exception("Filter header mismatch. Delete file " + frameAttributeFileName);

                int count = BR.ReadInt32();
                if (count < dataset.Frames.Count)
                    throw new Exception("Too few filter values in file " + frameAttributeFileName);

                count = dataset.Frames.Count;
                
                mFrameAttribute = new float[count];
                for (int i = 0; i < count; i++)
                    mFrameAttribute[i] = BR.ReadSingle();
            }
        }

        public void SetMaskTo(float threshold)
        {
            if (mFrameAttribute == null)
                return;

            // set mask using the threshold
            bool[] mask = Mask;
            Parallel.For(0, mask.Length, i =>
                mask[i] = mFrameAttribute[i] > threshold);

            Mask = mask; // sets also inverted mask
        }

    }
}
