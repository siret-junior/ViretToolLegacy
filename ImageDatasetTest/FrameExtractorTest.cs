using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace ImageDatasetTest
{
    [TestClass]
    public class FrameExtractorTest
    {
        const string FFPROBE_EXE = "..\\..\\..\\FrameExtractor\\ffmpeg\\ffprobe.exe";
        const string FFMPEG_EXE = "..\\..\\..\\FrameExtractor\\ffmpeg\\ffmpeg.exe";


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
        public void ExtractFPS()
        {
            string inputVideoDirectory = "..\\..\\TestData\\0-videos\\";
            string outputFile = "..\\..\\TestData\\fps.txt";
            
            Assert.IsTrue(Directory.Exists(inputVideoDirectory),
                "Input directory does not exist: " + inputVideoDirectory);
            
            Assert.IsTrue(File.Exists(FFPROBE_EXE),
                "ffprobe.exe not found: " + FFPROBE_EXE);

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            string[] videoFiles = Directory.GetFiles(inputVideoDirectory);
            Array.Sort(videoFiles);

            for (int i = 0; i < videoFiles.Length; i++)
            {
                string videoPath = videoFiles[i];
                string videoFilename = Path.GetFileNameWithoutExtension(videoPath);

                // write filename (ID)
                using (StreamWriter writer = new StreamWriter(outputFile, true))
                {
                    writer.Write(videoFilename + " ");
                }

                // launch ffprobe
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = FFPROBE_EXE,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        Arguments = "-v 0 -of compact=p=0 -select_streams 0 -show_entries stream=r_frame_rate "
                            + videoPath
                    }
                };
                process.EnableRaisingEvents = true;
                process.Start();

                // compute and write the FPS value
                string output = process.StandardOutput.ReadToEnd();
                string fps = output.Split('=')[1];
                using (StreamWriter writer = new StreamWriter(outputFile, true))
                {
                    writer.Write(fps);
                }

                // check for errors
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        "Process failed to execute properly: ffmpeg.exe " + process.StartInfo.Arguments);
                }
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
                "Input directory does not exist: " + inputVideoDirectory);
            Assert.IsTrue(File.Exists(fpsFile),
                "FPS file missing: " + fpsFile);

            string[] arguments = new string[] { FFMPEG_EXE, inputVideoDirectory, outputDirectory, fpsFile, outputFPS };
            FrameExtractor.Program.Main(arguments);
        }


        [TestMethod]
        public void SelectEveryNthFrame()
        {
            string inputImageDirectory = "..\\..\\TestData\\1-frames\\";
            string outputDirectory = "..\\..\\TestData\\1-keyframes\\";
            int nthFrame = 12; // 3sec x 4fps

            Assert.IsTrue(Directory.Exists(inputImageDirectory),
                "Input directory does not exist: " + inputImageDirectory);
            Directory.CreateDirectory(outputDirectory);

            string[] videoDirectories = Directory.GetDirectories(inputImageDirectory);
            foreach (string videoDirectory in videoDirectories)
            {
                string videoId = Path.GetFileName(videoDirectory);
                string outputVideoDirectory = Path.Combine(outputDirectory, videoId);
                Directory.CreateDirectory(outputVideoDirectory);

                string[] filePaths = Directory.GetFiles(videoDirectory);
                for (int i = 0; i < filePaths.Length; i += nthFrame)
                {
                    string filePath = filePaths[i];
                    string filename = Path.GetFileName(filePath);
                    string copyTo = Path.Combine(outputVideoDirectory, filename);
                    File.Copy(filePaths[i], copyTo, true);
                }
            }
        }
    }
}
