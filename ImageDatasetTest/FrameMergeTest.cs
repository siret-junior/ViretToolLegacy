using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ImageDatasetTest
{
    [TestClass]
    public class FrameMergeTest
    {
        [TestMethod]
        public void MergeThumbnails()
        {
            string inputFramesDirectory = "..\\..\\TestData\\1-frames\\";
            string outputMergedFile = "..\\..\\TestData\\thumbnails.thumb";
            string frameWidth = "100";
            string frameHeight = "75";
            string framerate = "4";
            string intDatasetId = "42";

            Assert.IsTrue(Directory.Exists(inputFramesDirectory));

            string[] arguments = new string[] {
                inputFramesDirectory, outputMergedFile, frameWidth, frameHeight, framerate, intDatasetId };
            FrameMerge.Program.Main(arguments);
        }


        [TestMethod]
        public void MergeKeyframes()
        {
            string inputFramesDirectory = "..\\..\\TestData\\1-keyframes\\";
            string outputMergedFile = "..\\..\\TestData\\keyframes.thumb";
            string frameWidth = "100";
            string frameHeight = "75";
            string framerate = "-1";
            string intDatasetId = "42";

            Assert.IsTrue(Directory.Exists(inputFramesDirectory));

            string[] arguments = new string[] {
                inputFramesDirectory, outputMergedFile, frameWidth, frameHeight, framerate, intDatasetId };
            FrameMerge.Program.Main(arguments);
        }
    }
}
