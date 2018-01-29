using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace BlackAndWhiteFilter {
    class Program {

        /// <param name="selectedFramesFilename"></param>
        /// <param name="allExtractedFramesFilename"></param>
        /// <param name="thresholdForBlackColor"></param>
        static void Main(string[] args) {
            if (args.Length != 3) {
                Console.WriteLine("invalid arguments\n1: selected frames filename\n2: all extracted frames filename\n3: black color selection threshold");
                return;
            }

            int threshold = int.Parse(args[2]);
            var dataset = new ViretTool.DataModel.Dataset(args[0], args[1]);

            // prepare arrays with statistics for each frame
            float[] bwDeltaValues = new float[dataset.Frames.Count];
            float[] pbValues = new float[dataset.Frames.Count];
            for (int i = 0; i < dataset.Frames.Count; i++)
            {
                Tuple<float, float> statistics = ComputeColorStatistics(dataset.Frames[i].ActualBitmap, threshold);
                bwDeltaValues[i] = statistics.Item1;
                pbValues[i] = statistics.Item2;
            }

            // store arrays
            // TODO - use constants for filenames
            StoreFilterValues(dataset.GetFileNameByExtension(".bwfilter"), bwDeltaValues, dataset);
            StoreFilterValues(dataset.GetFileNameByExtension(".pbcfilter"), pbValues, dataset);
            
        }

        private static void StoreFilterValues(string filename, float[] filterValues, Dataset dataset)
        {
            using (var FS = new FileStream(filename, FileMode.Create))
                using (var BW = new BinaryWriter(FS))
                {
                    BW.Write(dataset.DatasetFileHeader);
                    BW.Write((Int32)filterValues.Length);

                    // TODO - optimize
                    foreach (float f in filterValues)
                        BW.Write(f);
                }   
        }

        public static unsafe Tuple<float, float> ComputeColorStatistics(Bitmap bitmap, int blackThreshold)
        {
            float maxRGBDelta = 0;
            float percentageOfBlack = 0;
            int blackThresholdSquared = blackThreshold * blackThreshold;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            System.Diagnostics.Debug.Assert(data.PixelFormat == PixelFormat.Format24bppRgb);

            byte* ptr = (byte*)data.Scan0.ToPointer();
            int bitmapSize = data.Height * data.Width * 3;

            int delta;
            byte R, G, B;
            for (int i = 0; i < bitmapSize; i += 3, ptr += 3) {
                R = *ptr; G = *(ptr + 1); B = *(ptr + 2);

                // count pixels with a limited L2 distance from black color
                if (R * R + G * G + B * B < blackThresholdSquared)
                    percentageOfBlack++;

                // find the maximal RGB delta
                delta = Math.Abs(R - G) + Math.Abs(G - B) + Math.Abs(B - R);
                if (delta > maxRGBDelta)
                    maxRGBDelta = (float)delta;
            }

            bitmap.UnlockBits(data);

            return new Tuple<float, float>(maxRGBDelta, percentageOfBlack / (data.Height * data.Width));
        }
    }
}
