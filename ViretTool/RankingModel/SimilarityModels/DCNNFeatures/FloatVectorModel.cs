using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.SimilarityModels.DCNNFeatures
{
    class FloatVectorModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// Extracted features from DCNN, normalized to |v| = 1 and each dimension globally quantized to byte
        /// </summary>
        private List<float[]> mFloatVectors;

        private int mDimension;

        private readonly string mDescriptorsFilename;

        private Dictionary<int, float[]> mCache;

        public FloatVectorModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mFloatVectors = new List<float[]>();

            mDescriptorsFilename = dataset.GetFileNameByExtension(".floatvector");

            LoadDescriptors();

            Clear();
        }

        public void Clear()
        {
            mCache = new Dictionary<int, float[]>();
        }        

        public float[] AddQueryResultsToCache(DataModel.Frame query, bool positiveExample)
        {
            if (mCache[query.ID] != null)
                return mCache[query.ID];

            float[] results = new float[mDataset.Frames.Count];

            float[] queryData = mFloatVectors[query.ID];

            Parallel.For(0, results.Length, i =>
            {
                results[i] = CosineSimilarity(mFloatVectors[mDataset.Frames[i].ID], queryData);
            });

            mCache.Add(query.ID, results);
            return mCache[query.ID];
        }

        public List<RankedFrame> RankFramesBasedOnExampleFrames(List<DataModel.Frame> positiveExamples, List<DataModel.Frame> negativeExamples)
        {
            List<RankedFrame> rankedFrames = RankedFrame.InitializeResultList(mDataset.Frames);

            foreach (DataModel.Frame queryFrame in positiveExamples)
            {
                float[] partialResults = AddQueryResultsToCache(queryFrame, true);
                Parallel.For(0, rankedFrames.Count(), i =>
                    { rankedFrames[i].Rank += partialResults[i]; });
            }

            foreach (DataModel.Frame queryFrame in negativeExamples)
            {
                float[] partialResults = AddQueryResultsToCache(queryFrame, false);
                Parallel.For(0, rankedFrames.Count(), i =>
                    { rankedFrames[i].Rank -= partialResults[i]; });
            }

            return rankedFrames;
        }

        public float[] GetFrameSemanticVector(DataModel.Frame frame)
        {
            return mFloatVectors[frame.ID];
        }

        public static float ComputeDistance(float[] vectorA, float[] vectorB)
        {
            return L2Distance(vectorA, vectorB);
        }

        private static float CosineSimilarity(float[] x, float[] y)
        {
            double result = 0.0;

            // TODO - use vector instructions

            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }

            return Convert.ToSingle(result);
        }

        private static float L2Distance(float[] x, float[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                double difference = x[i] - y[i];
                result += difference * difference;
            }

            return Convert.ToSingle(Math.Sqrt(result));
        }

        private void LoadDescriptors()
        {
            if (!System.IO.File.Exists(mDescriptorsFilename))
                throw new Exception("Descriptors were not created to " + mDescriptorsFilename);

            using (System.IO.BinaryReader BR = new System.IO.BinaryReader(System.IO.File.OpenRead(mDescriptorsFilename)))
            {
                if (!mDataset.ReadAndCheckFileHeader(BR))
                    throw new Exception("Dataset/descriptor mismatch. Delete file " + mDescriptorsFilename);

                int count = BR.ReadInt32();
                if (count < mDataset.Frames.Count)
                    throw new Exception("Too few descriptors in file " + mDescriptorsFilename);

                count = mDataset.Frames.Count;

                mDimension = BR.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    byte[] data = BR.ReadBytes(mDimension * sizeof(float));
                    float[] dataVector = new float[mDimension];

                    Buffer.BlockCopy(data, 0, dataVector, 0, data.Length);

                    mFloatVectors.Add(dataVector);
                }
            }
        }


    }
}
