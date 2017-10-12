using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ImageDatasetTest
{
    [TestClass]
    public class FrameExtractorTest
    {
        const string FFMPEG_EXE = "..\\..\\..\\FrameExtractor\\ffmpeg\\bin\\ffmpeg.exe";

        
        [TestMethod]
        public void TestFFMPEG()
        {
            Assert.IsTrue(File.Exists(FFMPEG_EXE),
                "ffmpeg.exe not found: " + FFMPEG_EXE);

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFMPEG_EXE,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = "-h"
                }
            };

            process.EnableRaisingEvents = true;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "Process failed to execute properly: ffmpeg.exe " + process.StartInfo.Arguments);
            }
        }

        [TestMethod]
        public void ExtractFrames()
        {
            string inputVideoDirectory = "..\\..\\TestData\\0-videos\\";
            string outputDirectory = "..\\..\\TestData\\1-frames\\";
            string fpsFile = "..\\..\\TestData\\fps.txt";
            string outputFPS = "4";

            Assert.IsTrue(Directory.Exists(inputVideoDirectory), 
                //"Input directory does not exist: " + inputVideoDirectory);
            "Input directory does not exist: " + Directory.GetCurrentDirectory());
            Assert.IsTrue(File.Exists(fpsFile),
                "FPS file missing: " + fpsFile);

            string[] arguments = new string[] { FFMPEG_EXE, inputVideoDirectory, outputDirectory, fpsFile, outputFPS };
            FrameExtractor.Program.Main(arguments);
        }
    }
}
