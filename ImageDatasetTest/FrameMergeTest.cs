using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ImageDatasetTest
{
    [TestClass]
    public class FrameMergeTest
    {
        // TRECVid == 7536714 (TODO)
        // ITEC == 1736
        const string DATASET_ID = "-1";

        const string FRAME_WIDTH = "100";
        const string FRAME_HEIGHT = "75";
        const string FRAMERATE = "4";

        [TestMethod]
        public void MergeThumbnails()
        {
            string inputFramesDirectory = "..\\..\\TestData\\1-frames\\";
            string outputMergedFile = "..\\..\\TestData\\thumbnails.thumb";
            
            Assert.IsTrue(Directory.Exists(inputFramesDirectory),
                "Input directory does not exist: " + inputFramesDirectory);

            string[] arguments = new string[] {
                inputFramesDirectory, outputMergedFile, FRAME_WIDTH, FRAME_HEIGHT, FRAMERATE, DATASET_ID };
            FrameMerge.Program.Main(arguments);
        }


        [TestMethod]
        public void MergeKeyframes()
        {
            string inputFramesDirectory = "..\\..\\TestData\\1-keyframes\\";
            string outputMergedFile = "..\\..\\TestData\\keyframes.thumb";
            string framerate = "-1";
            
            Assert.IsTrue(Directory.Exists(inputFramesDirectory),
                "Input directory does not exist: " + inputFramesDirectory);
            
            string[] arguments = new string[] {
                inputFramesDirectory, outputMergedFile, FRAME_WIDTH, FRAME_HEIGHT, framerate, DATASET_ID };
            FrameMerge.Program.Main(arguments);
        }
    }
}
