using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FrameIO;
using System.Drawing;
using System.Collections.Generic;

namespace ImageDatasetTest
{
    [TestClass]
    public class FrameReaderTest
    {
        private const string THUMBNAILS_FILE = "..\\..\\TestData\\thumbnails.thumb";
        private const string KEYFRAMES_FILE = "..\\..\\TestData\\keyframes.thumb";


        [TestMethod]
        public void ReadVideoThumbnails()
        {
            Assert.IsTrue(File.Exists(THUMBNAILS_FILE),
                "Thumbnails file missing: " + THUMBNAILS_FILE);
            
            FrameReader reader = new FrameReader(THUMBNAILS_FILE);

            Tuple<int, int, byte[]>[] frames = reader.ReadVideoFrames(0);
            List<Image> images = new List<Image>();
            foreach (Tuple<int, int, byte[]> frameData in frames)
            {
                int videoId = frameData.Item1;
                int frameNumber = frameData.Item2;
                byte[] jpgThumbnail = frameData.Item3;

                Image image = ConvertToImage(jpgThumbnail);
                images.Add(image);
            }
        }


        [TestMethod]
        public void ReadVideoKeyframes()
        {
            Assert.IsTrue(File.Exists(KEYFRAMES_FILE),
                "Keyframes file missing: " + KEYFRAMES_FILE);

            FrameReader reader = new FrameReader(KEYFRAMES_FILE);

            Tuple<int, int, byte[]>[] frames = reader.ReadVideoFrames(0);
            List<Image> images = new List<Image>();
            foreach (Tuple<int, int, byte[]> frameData in frames)
            {
                int videoId = frameData.Item1;
                int frameNumber = frameData.Item2;
                byte[] jpgThumbnail = frameData.Item3;

                Image image = ConvertToImage(jpgThumbnail);
                images.Add(image);
            }
        }

        /// <summary>
        /// Converts the JPEG data into an Image bitmap.
        /// </summary>
        /// <param name="jpgData">Image stored in JPEG format.</param>
        /// <returns>Converted Image bitmap.</returns>
        public static Image ConvertToImage(byte[] jpgData)
        {
            // disposing memory stream does not work, apparently Image class still uses it under the hood
            //using (MemoryStream stream = new MemoryStream(jpgData))
            //{

            // memory leak?
            MemoryStream stream = new MemoryStream(jpgData, false);
            Image result = Image.FromStream(stream);
            return result;
            
            //}
        }
    }
}
