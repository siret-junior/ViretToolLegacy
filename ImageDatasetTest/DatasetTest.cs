using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ViretTool.DataModel;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageDatasetTest
{
    [TestClass]
    public class DatasetTest
    {
        //private const string THUMBNAILS_FILE = "..\\..\\TestData\\thumbnails.thumb";
        //private const string KEYFRAMES_FILE = "..\\..\\TestData\\keyframes.thumb";

        private const string THUMBNAILS_FILE = "..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb";
        private const string KEYFRAMES_FILE = "..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb";

        //private const string THUMBNAILS_FILE = "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb";
        //private const string KEYFRAMES_FILE = "..\\..\\..\\TestData\\TRECVid\\TRECVid-KF-100x75.thumb";

        [TestMethod]
        public void ConstructDataset()
        {
            Assert.IsTrue(File.Exists(THUMBNAILS_FILE),
                "Thumbnails file missing: " + THUMBNAILS_FILE);
            Assert.IsTrue(File.Exists(KEYFRAMES_FILE),
                "Keyframes file missing: " + KEYFRAMES_FILE);

            // TODO
            //Dataset dataset = new Dataset(KEYFRAMES_FILE, THUMBNAILS_FILE);

            //List<BitmapSource> bitmaps = new List<BitmapSource>();
            //foreach (Frame frame in dataset.Frames)
            //{
            //    bitmaps.Add(frame.GetImage());
            //}
        }


        
    }
}
