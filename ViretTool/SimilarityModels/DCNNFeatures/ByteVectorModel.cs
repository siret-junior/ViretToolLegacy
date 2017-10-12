using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.SimilarityModels.DCNNFeatures
{
    class ByteVectorModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// Extracted features from DCNN, normalized to |v| = 1 and each dimension globally quantized to byte
        /// </summary>
        private List<byte[]> mByteVectors;

        private int mDimension = 4096;

        private readonly string mDescriptorsFilename;

        public ByteVectorModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mByteVectors = new List<byte[]>();

            // TODO - name should be connected to the dataset name
            mDescriptorsFilename = System.IO.Path.Combine(mDataset.AllExtractedFramesFilename, "ByteVectors.vt");

            LoadDescriptors();
        }

        public List<RankedFrame> RankFramesBasedOnExampleFrames(List<DataModel.Frame> queryFrames)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset);

            foreach (DataModel.Frame queryFrame in queryFrames)
            {
                // TODO - use cache for already evaluated queries

                byte[] query = mByteVectors[queryFrame.ID];

                // detect nonzero query dimensions
                List<int> idx = new List<int>();
                for (int j = 0; j < query.Length; j++)
                    if (query[j] > 0) idx.Add(j);

                int[] indexes = idx.ToArray();

                // compute sequentially distances to all database frames 
                Parallel.For(0, result.Count(), i =>
                {
                    RankedFrame rf = result[i];
                    rf.Rank += CosineDistance(mByteVectors[rf.Frame.ID], query, indexes);
                });
            }

            return result;
        }

        /// <summary>
        /// Compares two vectors, where each dimension is quantized to one byte. Assumes normalized vectors.
        /// </summary>
        /// <param name="x">First byte vector.</param>
        /// <param name="y">Second byte vector.</param>
        /// <param name="indexes">Optimization focusing only on nonzero query dimensions.</param>
        /// <returns></returns>
        private double CosineDistance(byte[] x, byte[] y, int[] indexes)
        {
            double result = 0.0;

            foreach(int i in indexes)
                result += x[i] * y[i];

            return result;
        }

        private void LoadDescriptors()
        {
            if (!System.IO.File.Exists(mDescriptorsFilename))
                throw new Exception("Descriptors were not created to " + mDescriptorsFilename);

            using (System.IO.BinaryReader BR = new System.IO.BinaryReader(System.IO.File.OpenRead(mDescriptorsFilename)))
            {
                int datasetID = BR.ReadInt32();
                if (mDataset.DatasetID != datasetID)
                    throw new Exception("Dataset/descriptor mismatch. Delete file " + mDescriptorsFilename);

                // TODO - read also dimension

                int count = BR.ReadInt32();

                for (int i = 0; i < count; i++)
                    mByteVectors.Add(BR.ReadBytes(mDimension));
            }
        }

    }
}
