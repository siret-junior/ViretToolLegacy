using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ViretTool.SimilarityModels
{
    class ColorSignatureModel
    {
        private readonly DataModel.Dataset mDataset;
        private List<byte[]> mColorSignatures;

        private const int mSignatureWidth = 20;
        private const int mSignatureHeight = 15;
        private const int mGridRadius = 2;

        private readonly string mDescriptorsFilename;

        public ColorSignatureModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mColorSignatures = new List<byte[]>();

            // TODO - name should be connected to the dataset name
            mDescriptorsFilename = System.IO.Path.Combine(mDataset.AllExtractedFramesFilename, "ColorSignatures.vt");

            LoadDescriptors();
        }

        public List<RankedFrame> RankFramesBasedOnSketch(List<Tuple<double, double, Color>> queryCentroids, List<DataModel.Frame> filteredFrames = null)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset, filteredFrames);

            // transform [x, y] to a list of investigated positions in mGridRadius
            List<Tuple<int[], Color>> queries = PrepareQueries(queryCentroids);

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

                    rf.Rank += Math.Sqrt(minRank);
                }
            });

            result.Sort();

            return result;
        }

        private List<Tuple<int[], Color>> PrepareQueries(List<Tuple<double, double, Color>> queryCentroids)
        {
            List<Tuple<int[], Color>> queries = new List<Tuple<int[], Color>>();

            foreach (Tuple<double, double, Color> t in queryCentroids)
            {
                int x = (int)(t.Item1 * mSignatureWidth), y = (int)(t.Item2 * mSignatureHeight);
                int x1 = Math.Max(0, x - mGridRadius), x2 = Math.Min(mSignatureWidth - 1, x + mGridRadius);
                int y1 = Math.Max(0, y - mGridRadius), y2 = Math.Min(mSignatureHeight - 1, y + mGridRadius);

                List<int> offsets = new List<int>();
                for (int i = x1; i <= x2; i++)
                    for (int j = y1; j <= y2; j++)
                        offsets.Add(j * mSignatureWidth * 3 + i * 3);

                queries.Add(new Tuple<int[], System.Windows.Media.Color>(offsets.ToArray(), t.Item3));
            }

            return queries;
        }

        private int L2SquareDistance(int r1, int r2, int g1, int g2, int b1, int b2)
        {
            return (r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2);
        }


        public List<RankedFrame> RankFramesBasedOnExampleFrames(List<DataModel.Frame> queryFrames, List<DataModel.Frame> filteredFrames = null)
        {
            List<RankedFrame> result = RankedFrame.InitializeResultList(mDataset, filteredFrames);

            Parallel.For(0, result.Count(), i =>
            {
                RankedFrame rf = result[i];
                foreach (DataModel.Frame queryFrame in queryFrames)
                    rf.Rank += L2Distance(mColorSignatures[rf.Frame.ID], mColorSignatures[queryFrame.ID]);
            });

            result.Sort();

            return result;
        }

        private double L2Distance(byte[] x, byte[] y)
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
