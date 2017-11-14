using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackAndWhiteFilter {
    class Program {

        /// <param name="selectedFramesFilename"></param>
        /// <param name="allExtractedFramesFilename"></param>
        static void Main(string[] args) {
            if (args.Length != 3) {
                Console.WriteLine("invalid arguments\n1: selected frames filename\n2: all extracted frames filename\n3: selection threshold");
                return;
            }

            int threshold = int.Parse(args[2]);
            var dataset = new ViretTool.DataModel.Dataset(args[0], args[1]);
            string filename = dataset.AllExtractedFramesFilename.Split('-')[0] + ".bwfilter";

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true)) {
                fs.WriteAsync(new byte[] { (byte)'B', (byte)'W', (byte)'f', (byte)'i', (byte)'l', (byte)'t', (byte)'e', (byte)'r' }, 0, 8);
                fs.WriteAsync(BitConverter.GetBytes(threshold), 0, 4);

                foreach (ViretTool.DataModel.Frame frame in dataset.Frames) {
                    int val = IsGrayscale(frame.ActualBitmap);
                    if (val < threshold) {
                        fs.WriteAsync(BitConverter.GetBytes(frame.ID), 0, 4);
                        Console.WriteLine("Frame ID: {0}, Val: {1}", frame.ID, val);
                    }
                }
            }
        }

        public static unsafe int IsGrayscale(Bitmap bitmap) {
            int maxRGBDelta = 0;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            System.Diagnostics.Debug.Assert(data.PixelFormat == PixelFormat.Format24bppRgb);

            byte* ptr = (byte*)data.Scan0.ToPointer();
            int bitmapSize = data.Height * data.Width * 3;

            int delta;
            for (int i = 0; i < bitmapSize; i += 3, ptr += 3) {
                delta = RGBDelta(*ptr, *(ptr + 1), *(ptr + 2));
                if (delta > maxRGBDelta) maxRGBDelta = delta;
            }

            bitmap.UnlockBits(data);

            return maxRGBDelta;
        }

        private static int RGBDelta(byte R, byte G, byte B) {
            return Math.Abs(R - G) + Math.Abs(G - B) + Math.Abs(B - R);
        }
    }
}
