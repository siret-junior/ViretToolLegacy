//#define USE_MULTIPLICATION

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

        private int mSignatureWidth = 32;     // TODO: load dynamically from provided initializer file
        private int mSignatureHeight = 18;

        private readonly string mDescriptorsFilename;

        private Dictionary<string, float[]> mCache;

        public ColorSignatureModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mColorSignatures = new List<byte[]>();
            mDescriptorsFilename = dataset.GetFileNameByExtension(".color");
            LoadDescriptors();
            Clear();
        }

        public void Clear()
        {
            mCache = new Dictionary<string, float[]>();
        }

        public List<RankedFrame> RankFramesBasedOnSketch(List<Tuple<Point, Color, Point, bool>> queryCentroids)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset.Frames);

            // reuse cached results
            Dictionary<string, float[]> cache = new Dictionary<string, float[]>();

            foreach (Tuple<Point, Color, Point, bool> t in queryCentroids)
            {
                string key = t.ToString();
                if (mCache.ContainsKey(key))
                    cache.Add(key, mCache[key]);
                else
                    cache.Add(key, EvaluateOneQueryCentroid(t));
            }

            mCache = cache;

            foreach (float[] distances in cache.Values)
                Parallel.For(0, result.Count, i =>
                {
                    result[i].Rank += distances[i];
                });

            return result;
        }

        private float[] EvaluateOneQueryCentroid(Tuple<Point, Color, Point, bool> qc)
        {
            float[] distances = new float[mDataset.Frames.Count];


            // initialize to -1
            for (int i = 0; i < distances.Length; i++)
            {
#if USE_MULTIPLICATION
                distances[i] = -1;
#else
                distances[i] = 0;
#endif
            }

            // transform [x, y] to a list of investigated positions in mGridRadius
            Tuple<int[], Color, bool> t = PrepareQuery(qc);

            Parallel.For(0, distances.Length, i =>
            {
                byte[] signature = mColorSignatures[i];

                int R = t.Item2.R, G = t.Item2.G, B = t.Item2.B;

                if (!t.Item3)
                {
                    double minRank = int.MaxValue;
                    foreach (int offset in t.Item1)
                        minRank = Math.Min(minRank, L2SquareDistance(R, signature[offset], G, signature[offset + 1], B, signature[offset + 2]));

#if USE_MULTIPLICATION
                    distances[i] *= Convert.ToSingle(Math.Sqrt(minRank));
#else
                    distances[i] -= Convert.ToSingle(Math.Sqrt(minRank));
#endif
                }
                else
                {
                    double avgRank = 0;
                    foreach (int offset in t.Item1)
                        avgRank += Math.Sqrt(L2SquareDistance(R, signature[offset], G, signature[offset + 1], B, signature[offset + 2]));

#if USE_MULTIPLICATION
                    distances[i] *= Convert.ToSingle(avgRank / t.Item1.Length);
#else
                    distances[i] -= Convert.ToSingle(avgRank / t.Item1.Length);
#endif
                }
            });

            return distances;
        }
        
        public List<RankedFrame> RankFramesBasedOnExampleFrames(List<DataModel.Frame> queryFrames)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset.Frames);

            Parallel.For(0, result.Count(), i =>
            {
                RankedFrame rf = result[i];
                foreach (DataModel.Frame queryFrame in queryFrames)
                    rf.Rank -= L2Distance(mColorSignatures[rf.Frame.Id], mColorSignatures[queryFrame.Id]);
            });

            return result;
        }

        public byte[] GetFrameColorSignature(DataModel.Frame frame)
        {
            return mColorSignatures[frame.Id];
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
        private List<Tuple<int[], Color, bool>> PrepareQueries(List<Tuple<Point, Color, Point, bool>> queryCentroids)
        {
            List<Tuple<int[], Color, bool>> queries = new List<Tuple<int[], Color, bool>>();

            foreach (Tuple<Point, Color, Point, bool> t in queryCentroids)
                queries.Add(PrepareQuery(t));

            return queries;
        }

        /// <summary>
        /// Precompute a set of 2D grid cells (represented as offsets in 1D array) that should be investigated for the most similar query color.
        /// </summary>
        /// <param name="queryCentroid">Colored point from the color sketch.</param>
        /// <returns></returns>
        private Tuple<int[], Color, bool> PrepareQuery(Tuple<Point, Color, Point, bool> queryCentroid)
        {
            List<Tuple<int[], Color, bool>> queries = new List<Tuple<int[], Color, bool>>();

            double x = queryCentroid.Item1.X * mSignatureWidth, y = queryCentroid.Item1.Y * mSignatureHeight;
            double ax = queryCentroid.Item3.X * mSignatureWidth, ay = queryCentroid.Item3.Y * mSignatureHeight;
            double ax2 = ax * ax, ay2 = ay * ay;

            List<int> offsets = new List<int>();
            for (int i = 0; i < mSignatureWidth; i++)
                for (int j = 0; j < mSignatureHeight; j++)
                    if (1 >= (x - i - 0.5) * (x - i - 0.5) / ax2 + (y - j - 0.5) * (y - j - 0.5) / ay2)
                        offsets.Add(j * mSignatureWidth * 3 + i * 3);

            return new Tuple<int[], Color, bool>(offsets.ToArray(), 
                ImageHelper.RGBtoLabByte(queryCentroid.Item2.R, queryCentroid.Item2.G, queryCentroid.Item2.B), queryCentroid.Item4);
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
                if (!mDataset.ReadAndCheckFileHeader(BR))
                    throw new Exception("Dataset/descriptor mismatch. Delete file " + mDescriptorsFilename);

                int count = BR.ReadInt32();
                if (count < mDataset.Frames.Count)
                    throw new Exception("Too few descriptors in file " + mDescriptorsFilename);

                count = mDataset.Frames.Count;
                
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

                if (frame.Id != ID) throw new Exception("Frame ID mismatch.");

                mColorSignatures.Add(ImageHelper.ResizeAndStoreImageToRGBByteArray(frame.GetImage(), mSignatureWidth, mSignatureHeight));
            }

            // save all thumbnails to the file for a given dataset
            using (System.IO.FileStream FS = new System.IO.FileStream(mDescriptorsFilename, System.IO.FileMode.Create))
            {
                System.IO.BinaryWriter BW = new System.IO.BinaryWriter(FS);
                BW.Write(mDataset.DatasetId);
                BW.Write(mColorSignatures.Count());

                foreach (byte[] descriptor in mColorSignatures)
                    BW.Write(descriptor, 0, descriptor.Length);
            }

        }
    }
}
