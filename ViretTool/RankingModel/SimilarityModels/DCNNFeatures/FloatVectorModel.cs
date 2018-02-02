using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.SimilarityModels
{
    class FloatVectorModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// Extracted features from DCNN, normalized to |v| = 1 and each dimension globally quantized to byte
        /// </summary>
        private List<float[]> mFloatVectors;

        private int mVectorDimension;

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
            if (mCache.ContainsKey(query.ID))
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

        public List<RankedFrame> RankFramesBasedOnExampleFrames(
            List<DataModel.Frame> positiveExamples, 
            List<DataModel.Frame> negativeExamples = null)
        {
            List<RankedFrame> rankedFrames = RankedFrame.InitializeResultList(mDataset.Frames);

            foreach (DataModel.Frame queryFrame in positiveExamples)
            {
                float[] partialResults = AddQueryResultsToCache(queryFrame, true);
                Parallel.For(0, rankedFrames.Count(), i =>
                    { rankedFrames[i].Rank += partialResults[i]; });
            }

            if (negativeExamples != null)
            {
                foreach (DataModel.Frame queryFrame in negativeExamples)
                {
                    float[] partialResults = AddQueryResultsToCache(queryFrame, false);
                    Parallel.For(0, rankedFrames.Count(), i =>
                        { rankedFrames[i].Rank -= partialResults[i]; });
                }
            }

            return rankedFrames;
        }

        public float[] GetFrameSemanticVector(DataModel.Frame frame)
        {
            return mFloatVectors[frame.ID];
        }

        public static float ComputeDistance(float[] vectorA, float[] vectorB)
        {
            return CosineDistance(vectorA, vectorB);
        }

        private static float CosineDistance(float[] x, float[] y)
        {
            return 1 - CosineSimilarity(x, y);
        }

        private static float CosineSimilarity(float[] x, float[] y)
        {
            return CosineSimilaritySISD(x, y);
        }

        private static float CosineSimilaritySISD(float[] x, float[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }

            return Convert.ToSingle(result);
        }

        private static float CosineSimilaritySIMD(float[] vector1, float[] vector2)
        {
            int chunkSize = Vector<float>.Count;
            float result = 0f;

            Vector<float> vectorChunk1;
            Vector<float> vectorChunk2;
            for (var i = 0; i < vector1.Length; i += chunkSize)
            {
                vectorChunk1 = new Vector<float>(vector1, i);
                vectorChunk2 = new Vector<float>(vector2, i);

                result += Vector.Dot(vectorChunk1, vectorChunk2);
            }

            return result;
        }

        //private static float L2Distance(float[] x, float[] y)
        //{
        //    double result = 0.0;

        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        double difference = x[i] - y[i];
        //        result += difference * difference;
        //    }

        //    return Convert.ToSingle(Math.Sqrt(result));
        //}

        private void LoadDescriptors()
        {
            if (!File.Exists(mDescriptorsFilename))
                throw new Exception("Descriptors were not created to " + mDescriptorsFilename);

            using (BinaryReader reader = new BinaryReader(File.OpenRead(mDescriptorsFilename)))
            {
                if (!mDataset.ReadAndCheckFileHeader(reader))
                    throw new Exception("Dataset/descriptor mismatch. Delete file " + mDescriptorsFilename);

                int vectorCount = reader.ReadInt32();
                if (vectorCount < mDataset.Frames.Count)
                    throw new Exception("Too few descriptors in file " + mDescriptorsFilename);

                vectorCount = mDataset.Frames.Count;

                mVectorDimension = reader.ReadInt32();

                for (int i = 0; i < vectorCount; i++)
                {
                    byte[] data = reader.ReadBytes(mVectorDimension * sizeof(float));
                    float[] dataVector = new float[mVectorDimension];

                    Buffer.BlockCopy(data, 0, dataVector, 0, data.Length);

                    mFloatVectors.Add(dataVector);
                }
            }
        }


    }
}
