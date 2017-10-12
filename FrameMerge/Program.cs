using FrameIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameMerge
{
    public class Program
    {
        /// <summary>
        /// TODO: documentation
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("FrameMerge.exe <inputDirectory> <outputFilename> \n"
                + "<frameWidth> <frameHeight> <framerate> <datasetId>");
        }

        public static void Main(string[] args)
        {
            // parse program arguments
            string inputDirectory = Path.GetFullPath(args[0]);
            string outputFilename = Path.GetFullPath(args[1]);

            // parse additional arguments
            int frameWidth, frameHeight, timestamp;
            decimal framerate;
            try
            {
                ParseAdditionalArguments(args, out frameWidth, out frameHeight, out framerate, out timestamp);
            }
            catch
            {
                Console.Error.WriteLine("Error parsing program arguments!");
                PrintUsage();
                return;
            }

            // count files and directories
            int frameCount, videoCount;
            CountFramesAndVideos(inputDirectory, out frameCount, out videoCount);

            // run merging
            MergeFramesToBinaryFile(inputDirectory, outputFilename, 
                frameWidth, frameHeight, timestamp, framerate, frameCount, videoCount);
        }


        private static void MergeFramesToBinaryFile(string inputDirectory, string outputFilename, int frameWidth, int frameHeight, int timestamp, decimal framerate, int frameCount, int videoCount)
        {
            // statistics variables
            Stopwatch stopwatch = Stopwatch.StartNew();
            int processedVideosCount = 0;

            // merge frames
            Console.WriteLine("Merging frame images:");
            try
            {
                using (FrameWriter writer = new FrameWriter(outputFilename,
                    timestamp, frameCount, videoCount, frameWidth, frameHeight, framerate))
                {
                    // load video directories
                    string[] videoDirectories = Directory.GetDirectories(inputDirectory);
                    Array.Sort(videoDirectories);

                    int videoCounter = 0;
                    foreach (string videoDirectory in videoDirectories)
                    {
                        Console.Write("Processing video ID {0}... ", Path.GetFileName(videoDirectory));

                        // load video frames
                        string[] filenames = Directory.GetFiles(videoDirectory);
                        Array.Sort(filenames);

                        // convert image and write to binary file
                        foreach (string filename in filenames)
                        {
                            int videoId = videoCounter;
                            int frameId = ParseFrameId(filename);

                            byte[] originalImageData = File.ReadAllBytes(filename);
                            byte[] resizedImageData = PreprocessImage(originalImageData, frameWidth, frameHeight);
                            writer.AppendFrame(resizedImageData, videoId, frameId);
                        }
                        writer.NewVideo();
                        videoCounter++;
                        Console.WriteLine("DONE!");

                        // compute and print statistics
                        processedVideosCount++;
                        PrintVideoStatistics(stopwatch, processedVideosCount, videoDirectories, videoDirectory);
                    }
                    PrintFinalStatistics(stopwatch, videoDirectories);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return;
            }
        }

        private static void PrintFinalStatistics(Stopwatch stopwatch, string[] videoDirectories)
        {
            int secondsElapsed = (int)(stopwatch.ElapsedMilliseconds * 0.001);
            int hoursElapsed = secondsElapsed / (60 * 60);
            int minutesElapsed = secondsElapsed / 60;
            secondsElapsed = secondsElapsed % 60;
            Console.WriteLine("Merging of {0} files finished in {1}h{2}m{3}s.", videoDirectories.Length,
                hoursElapsed, minutesElapsed, secondsElapsed);
        }

        private static void PrintVideoStatistics(Stopwatch stopwatch, int processedVideosCount, string[] videoDirectories, string videoDirectory)
        {
            double processedPerSecond = processedVideosCount / (stopwatch.ElapsedMilliseconds * 0.001);
            int secondsRemaining = (int)((videoDirectories.Length - processedVideosCount) / processedPerSecond);
            int hoursRemaining = secondsRemaining / (60 * 60);
            int minutesRemaining = secondsRemaining / 60;
            secondsRemaining = secondsRemaining % 60;
            Console.WriteLine("Video ID: {0} processed: {1} of {2}, ({3} per second, {4}h{5}m{6}s remaining).",
                Path.GetFileName(videoDirectory),
                processedVideosCount, videoDirectories.Length, processedPerSecond.ToString("0.000"),
                hoursRemaining, minutesRemaining, secondsRemaining);
        }

        private static void ParseAdditionalArguments(string[] args, out int frameWidth, out int frameHeight, out decimal framerate, out int timestamp)
        {
            frameWidth = int.Parse(args[2]);
            frameHeight = int.Parse(args[3]);
            framerate = decimal.Parse(args[4], CultureInfo.InvariantCulture);
            timestamp = int.Parse(args[5]);
        }

        private static void CountFramesAndVideos(string inputDirectory, out int frameCount, out int videoCount)
        {
            Console.Write("Counting frames and videos... ");
            frameCount = 0;
            videoCount = 0;

            string[] videoDirectories = Directory.GetDirectories(inputDirectory);
            foreach (string videoDirectory in videoDirectories)
            {
                string[] filenames = Directory.GetFiles(videoDirectory);
                frameCount += filenames.Length;
                videoCount++;
            }
            Console.WriteLine("DONE!");
        }


        private static byte[] PreprocessImage(byte[] jpgData, int width, int height)
        {
            byte[] result;
            using (MemoryStream inputStream = new MemoryStream(jpgData))
            using (MemoryStream outputStream = new MemoryStream())
            {
                Image originalImage = Image.FromStream(inputStream);
                Image resizedImage = new Bitmap(width, height);
                using (Graphics gfx = Graphics.FromImage(resizedImage))
                {
                    gfx.SmoothingMode = SmoothingMode.HighQuality;
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gfx.DrawImage(originalImage, new Rectangle(0, 0, width, height));
                }
                resizedImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                result = outputStream.ToArray();
            }
            return result;
        }

        private static int ParseVideoId(string filename)
        {
            string[] tokens = Path.GetFileNameWithoutExtension(filename).Split('_');
            int videoId;
            try
            {
                videoId = int.Parse(tokens[0].Substring(1));
            }
            catch
            {
                throw new IOException("Error parsing video ID: " + Path.GetFileNameWithoutExtension(filename));
            }
            return videoId;
        }

        private static int ParseFrameId(string filename)
        {
            string[] tokens = Path.GetFileNameWithoutExtension(filename).Split('_');
            int frameId;
            try
            {
                frameId = int.Parse(tokens[1].Substring(1));
            }
            catch
            {
                throw new IOException("Error parsing frame ID: " + Path.GetFileNameWithoutExtension(filename));
            }
            return frameId;
        }
    }
}
