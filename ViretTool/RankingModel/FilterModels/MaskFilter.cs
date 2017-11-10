using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    /// <summary>
    /// TODO: precompute various mask combinations (2^(n masks + 1))
    /// </summary>
    abstract class MaskFilter : Filter
    {
        bool[] mMask;
        bool[] mMaskInverted;

        public bool[] Mask
        {
            get
            { return mMask; }
            protected set
            {
                mMask = value;
                mMaskInverted = InvertMask(mMask);
            }
        }
        public bool[] MaskInverted
        {
            get
            { return mMaskInverted; }
            private set
            {
                mMaskInverted = value;
            }
        }


        public MaskFilter(DataModel.Dataset dataset, bool[] mask)
            : base(dataset)
        {
            Mask = mask;
        }

        public MaskFilter(DataModel.Dataset dataset)
            : base(dataset)
        {
            Mask = null;
        }


        public bool[] AggregateMasks(params bool[][] masks)
        {
            // check null input
            if (masks == null || masks.Length == 0)
            {
                throw new ArgumentException("Input masks are empty!");
            }

            // check mask length
            int datasetLength = mDataset.Frames.Count;
            if (!masks.All(x => x.Length == datasetLength))
            {
                throw new ArgumentException("Input masks do not have the same length!");
            }

            // initialize result
            bool[] result = new bool[datasetLength];   // TODO: array reusing
            SetMaskTo(result, true);

            // aggregate masks
            Parallel.For(0, datasetLength, index =>
            {
                for (int iMask = 0; iMask < masks.Length; iMask++)
                {
                    if (masks[iMask][index] == false)
                    {
                        result[index] = false;
                        break;
                    }
                }
            });

            return result;
        }
        

        private static void SetMaskTo(bool[] mask, bool value)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = value;
            }
        }

        private static bool[] InvertMask(bool[] mask)
        {
            if (mask == null)
            {
                return null;
            }

            bool[] result = new bool[mask.Length];
            Parallel.For(0, mask.Length, index =>
            {
                result[index] = !mask[index];
            });
            return result;
        }
    }
}
