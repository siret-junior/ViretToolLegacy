using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ViretTool.RankingModel.SimilarityModels
{
    class ColorSignatureModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// A thumbnail based signature in RGB format, stored as a 1D byte array.
        /// </summary>
        private List<byte[]> mColorSignatures;

        private int mSignatureWidth = 20;     // TODO: load dynamically from provided initializer file
        private int mSignatureHeight = 15;
        private const int mGridRadius = 2;

        private readonly string mDescriptorsFilename;

        public ColorSignatureModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mColorSignatures = new List<byte[]>();

            // TODO - name should be connected to the dataset name or ID
            //mDescriptorsFilename = System.IO.Path.Combine(mDataset.AllExtractedFramesFilename, "ColorSignatures.vt");
            string stripFilename = System.IO.Path.GetFileNameWithoutExtension(mDataset.AllExtractedFramesFilename);
            string modelFilename = stripFilename.Split('-')[0] + ".color";    // TODO: find better solution
            string parentDirectory = System.IO.Directory.GetParent(mDataset.AllExtractedFramesFilename).ToString();
            mDescriptorsFilename = System.IO.Path.Combine(parentDirectory, modelFilename);

            LoadDescriptors();
        }


        public List<RankedFrame> RankFramesBasedOnSketch(List<Tuple<Point, Color>> queryCentroids)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset);

            // transform [x, y] to a list of investigated positions in mGridRadius
            List<Tuple<int[], Color>> queries = PrepareQueries(queryCentroids);

            // TODO - cache results
            Parallel.For(0, result.Count(), i =>
            {
                RankedFrame rf = result[i];
                byte[] signature = mColorSignatures[rf.Frame.ID];

                foreach (Tuple<int[], Color> t in queries)
                {
                    double minRank = int.MaxValue;
                    int R = t.Item2.R, G = t.Item2.G, B = t.Item2.B;

                    foreach (int offset in t.Item1)
                        minRank = Math.Min(minRank, L2SquareDistance(R, signature[offset], G, signature[offset + 1], B, signature[offset + 2]));

                    rf.Rank -= Math.Sqrt(minRank);
                }
            });

            return result;
        }
        
        public List<RankedFrame> RankFramesBasedOnExampleFrames(List<DataModel.Frame> queryFrames)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset);

            Parallel.For(0, result.Count(), i =>
            {
                RankedFrame rf = result[i];
                foreach (DataModel.Frame queryFrame in queryFrames)
                    rf.Rank -= L2Distance(mColorSignatures[rf.Frame.ID], mColorSignatures[queryFrame.ID]);
            });

            return result;
        }

        public byte[] GetFrameColorSignature(DataModel.Frame frame)
        {
            return mColorSignatures[frame.ID];
        }

        public static double ComputeDistance(byte[] vectorA, byte[] vectorB)
        {
            return L2Distance(vectorA, vectorB);
        }

        /// <summary>
        /// Precompute a set of 2D grid cells (represented as offsets in 1D array) that should be investigated for the most similar query color.
        /// </summary>
        /// <param name="queryCentroids">Set of colored points from the color sketch.</param>
        /// <returns></returns>
        private List<Tuple<int[], Color>> PrepareQueries(List<Tuple<Point, Color>> queryCentroids)
        {
            List<Tuple<int[], Color>> queries = new List<Tuple<int[], Color>>();

            foreach (Tuple<Point, Color> t in queryCentroids)
            {
                double x = t.Item1.X * mSignatureWidth, y = t.Item1.Y * mSignatureHeight;
                List<int> offsets = new List<int>();
                for (int i = 0; i < mSignatureWidth; i++)
                    for (int j = 0; j < mSignatureHeight; j++)
                        if (mGridRadius * mGridRadius >= (x - i - 0.5) * (x - i - 0.5) + (y - j - 0.5) * (y - j - 0.5))
                            offsets.Add(j * mSignatureWidth * 3 + i * 3);

                //queries.Add(new Tuple<int[], Color>(offsets.ToArray(), t.Item2));
                queries.Add(new Tuple<int[], Color>(offsets.ToArray(), ImageHelper.RGBtoLabByte(t.Item2.R, t.Item2.G, t.Item2.B)));
            }

            return queries;
        }

        private static int L2SquareDistance(int r1, int r2, int g1, int g2, int b1, int b2)
        {
            return (r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2);
        }

        private static double L2Distance(byte[] x, byte[] y)
        {
            double result = 0, r;
            for (int i = 0; i < x.Length; i++)
            {
                r = x[i] - y[i];
                result += r * r;
            }
            return Math.Sqrt(result);
        }

        private void LoadDescriptors()
        {
            if (!System.IO.File.Exists(mDescriptorsFilename))
                CreateDescriptors();

            using (System.IO.BinaryReader BR = new System.IO.BinaryReader(System.IO.File.OpenRead(mDescriptorsFilename)))
            {
                int datasetID = BR.ReadInt32();
                if (mDataset.DatasetID != datasetID)
                    throw new Exception("Dataset/descriptor mismatch. Delete file " + mDescriptorsFilename);

                int count = BR.ReadInt32();
                mSignatureWidth = BR.ReadInt32();
                mSignatureHeight = BR.ReadInt32();
                int size = mSignatureWidth * mSignatureHeight * 3;

                for (int i = 0; i < count; i++)
                    mColorSignatures.Add(BR.ReadBytes(size));
            }
        }

        private void CreateDescriptors()
        {
            // create RGB thumbnails stored in byte[]
            for (int ID = 0; ID < mDataset.Frames.Count(); ID++)
            {
                DataModel.Frame frame = mDataset.Frames[ID];

                if (frame.ID != ID) throw new Exception("Frame ID mismatch.");

                mColorSignatures.Add(ImageHelper.ResizeAndStoreImageToRGBByteArray(frame.GetImage(), mSignatureWidth, mSignatureHeight));
            }

            // save all thumbnails to the file for a given dataset
            using (System.IO.FileStream FS = new System.IO.FileStream(mDescriptorsFilename, System.IO.FileMode.Create))
            {
                System.IO.BinaryWriter BW = new System.IO.BinaryWriter(FS);
                BW.Write(mDataset.DatasetID);
                BW.Write(mColorSignatures.Count());

                foreach (byte[] descriptor in mColorSignatures)
                    BW.Write(descriptor, 0, descriptor.Length);
            }

        }
    }
}
