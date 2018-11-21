using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.SimilarityModels
{
    class ByteVectorModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// Extracted features from DCNN, normalized to |v| = 1 and each dimension globally quantized to byte
        /// </summary>
        private List<byte[]> mByteVectors;

        private int mDimension;

        private readonly string mDescriptorsFilename;


        public ByteVectorModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mByteVectors = new List<byte[]>();

            mDescriptorsFilename = dataset.GetFileNameByExtension(".vector");

            LoadDescriptors();
        }

        public List<RankedFrame> RankFramesBasedOnExampleFrames(List<DataModel.Frame> queryFrames)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset.Frames);

            foreach (DataModel.Frame queryFrame in queryFrames)
            {
                // TODO - use cache for already evaluated queries

                byte[] query = mByteVectors[queryFrame.Id];

                // detect nonzero query dimensions
                List<int> idx = new List<int>();
                for (int j = 0; j < query.Length; j++)
                    if (query[j] > 0) idx.Add(j);

                int[] indexes = idx.ToArray();

                // compute sequentially distances to all database frames 
                Parallel.For(0, result.Count(), i =>
                {
                    RankedFrame rankedFrame = result[i];
                    rankedFrame.Rank += CosineSimilarity(mByteVectors[rankedFrame.Frame.Id], query, indexes);
                });
            }

            return result;
        }

        public byte[] GetFrameSemanticVector(DataModel.Frame frame)
        {
            return mByteVectors[frame.Id];
        }

        public static double ComputeDistance(byte[] vectorA, byte[] vectorB)
        {
            return L2Distance(vectorA, vectorB);
        }
        
        /// <summary>
        /// Compares two vectors, where each dimension is quantized to one byte. Assumes normalized vectors.
        /// </summary>
        /// <param name="x">First byte vector.</param>
        /// <param name="y">Second byte vector.</param>
        /// <param name="indexes">Optimization focusing only on nonzero query dimensions.</param>
        /// <returns></returns>
        private static double CosineSimilarity(byte[] x, byte[] y, int[] indexes)
        {
            double result = 0.0;

            foreach (int i in indexes)
            {
                result += x[i] * y[i];
            }

            return result;
        }

        private static double CosineSimilarity(byte[] x, byte[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }

            return result;
        }

        private static double L2Distance(byte[] x, byte[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                double difference = x[i] - y[i];
                result += difference * difference;
            }

            return Math.Sqrt(result);
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
                    mByteVectors.Add(BR.ReadBytes(mDimension));
            }
        }

    }
}
