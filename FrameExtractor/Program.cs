#define PARALLEL
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameExtractor
{
    /// <summary>
    /// TODO: documentation
    /// </summary>
    public class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("FrameExtractor.exe <ffmpeg.exe> <inputVideoDirectory> <outputDirectory> \n"
                + "<framerateFile> <extractionFramerate> [maxParallelism]");
        }

        public static void Main(string[] args)
        {
            // load input arguments
            string ffmpegExe = Path.GetFullPath(args[0]);
            string inputVideoDirectory = Path.GetFullPath(args[1]);
            string outputDirectory = Path.GetFullPath(args[2]);
            string framerateFile = Path.GetFullPath(args[3]);

            // parse additional arguments
            decimal frameEveryNthSecond;
            int maxParallelism;
            if (!ParseAdditionalArguments(args, out frameEveryNthSecond, out maxParallelism))
            {
                PrintUsage();
                return;
            }

            // check ffmpeg
            if (!File.Exists(ffmpegExe))
            {
                Console.Error.WriteLine("ffmpeg.exe does not exist: {0}", ffmpegExe);
                return;
            }

            // parse video framerates
            Dictionary<int, decimal> videoFramerates = ParseVideoFramerates(framerateFile);

            // process videos
            ProcessVideos(ffmpegExe, inputVideoDirectory, outputDirectory, 
                videoFramerates, frameEveryNthSecond, maxParallelism);
        }


        private static void ProcessVideos(string ffmpegExe,
            string inputDirectory, string outputDirectory,
            Dictionary<int, decimal> videoFramerates, decimal frameEveryNthSecond, int maxParallelism)
        {
            // load all frame filenames
            string[] videoFilePaths = Directory.GetFiles(inputDirectory);
            Array.Sort(videoFilePaths);

            Console.WriteLine("Processing {0} video files:", videoFilePaths.Length);
            Console.WriteLine("Maximal degree of parallelism: {0}", maxParallelism);

            // statistics variables
            Stopwatch stopwatch = Stopwatch.StartNew();
            int processedCount = 0;

#if PARALLEL && !DEBUG
            Parallel.For(0, videoFilePaths.Length,
                new ParallelOptions { MaxDegreeOfParallelism = maxParallelism },
                index =>
#else
            for (int index = 0; index < videoFilePaths.Length; index++)
#endif
            {
                string videoFilePath = videoFilePaths[index];
                // create output subdirectory for each video
                string videoFilenameWithoutExtension = Path.GetFileNameWithoutExtension(videoFilePath);
                string videoSubdirectory = Path.Combine(outputDirectory, videoFilenameWithoutExtension);
                Directory.CreateDirectory(videoSubdirectory);

                // parse video ID
                int videoId = ParseVideoId(videoFilenameWithoutExtension);

                // create temp directory for each task
                string extractionTempDir = Path.GetFullPath("ffmpeg_extraction_temp_" + index.ToString("00000"));
                Directory.CreateDirectory(extractionTempDir);

                // extract all frames using ffmpeg binary
                ExtractAllFrames(ffmpegExe, videoFilePath, extractionTempDir);

                // select a subset of frames and move/rename into the output subdirectory
                CopyEveryNthSecondFrame(extractionTempDir, videoSubdirectory,
                    videoId, videoFramerates[videoId], frameEveryNthSecond);

                // cleanup
                Directory.Delete(extractionTempDir, true);

                // compute and print statistics
                Interlocked.Increment(ref processedCount);
                PrintStatistics(videoFilePaths, stopwatch, processedCount, videoId);
            }
#if PARALLEL && !DEBUG
            );
#endif



        }

        private static bool ParseAdditionalArguments(string[] args, out decimal frameEveryNthSecond, out int maxParallelism)
        {
            frameEveryNthSecond = -1;
            maxParallelism = -1;

            // parse frameEveryNthSecond
            try
            {
                frameEveryNthSecond = 1 / decimal.Parse(args[4], CultureInfo.InvariantCulture);
            }
            catch
            {
                Console.Error.WriteLine("Error parsing frameEveryNthSecond (decimal)!");
                return false;
            }

            // parse maxParallelism if necessary
            if (args.Length == 6)
            {
                try
                {
                    maxParallelism = int.Parse(args[5], CultureInfo.InvariantCulture);
                }
                catch
                {
                    Console.Error.WriteLine("Error parsing maxParallelism (int)!");
                    return false;
                }
            }
            else
            {
                maxParallelism = Environment.ProcessorCount;
            }
            return true;
        }

        private static Dictionary<int, decimal> ParseVideoFramerates(string videoFPSFile)
        {
            Console.Write("Parsing video framerates... ");

            Dictionary<int, decimal> videoFramerates = new Dictionary<int, decimal>();
            using (StreamReader reader = new StreamReader(videoFPSFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] tokens = line.Split();
                    int videoID = int.Parse(tokens[0]);

                    string[] division = tokens[1].Split('/');
                    decimal num0 = int.Parse(division[0]);
                    decimal num1 = int.Parse(division[1]);
                    decimal framerateValue = num0 / num1;
                    videoFramerates.Add(videoID, framerateValue);
                }
            }
            Console.WriteLine("DONE!");
            return videoFramerates;
        }

        private static void PrintStatistics(string[] videoFilePaths, Stopwatch stopwatch, int processedCount, int videoId)
        {
            double processedPerSecond = processedCount / (stopwatch.ElapsedMilliseconds * 0.001);
            int secondsRemaining = (int)((videoFilePaths.Length - processedCount) / processedPerSecond);
            int hoursRemaining = secondsRemaining / (60 * 60);
            int minutesRemaining = secondsRemaining / 60 % 60;
            secondsRemaining = secondsRemaining % 60;

            Console.WriteLine("Video ID:{0} processed. {1} of {2}, ({3} per second, {4}h {5}m {6}s remaining).", videoId,
                processedCount, videoFilePaths.Length, processedPerSecond.ToString("0.000"),
                hoursRemaining.ToString("00"), minutesRemaining.ToString("00"), secondsRemaining.ToString("00"));
        }

        private static int ParseVideoId(string videoFilenameWithoutExtension)
        {
            int videoId;
            try
            {
                videoId = int.Parse(videoFilenameWithoutExtension);
            }
            catch
            {
                throw new ArgumentException("Video filename is not an integer: " + videoFilenameWithoutExtension);
            }

            return videoId;
        }

        private static void ExtractAllFrames(string ffmpegExe, 
            string inputFile, string outputDirectory, string extension = "jpg")
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegExe,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = "-i \"" + inputFile + "\""
                    + " -start_number 0"
                    + " \"" + outputDirectory + "\\%08d." + extension + "\""
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
        
        private static void CopyEveryNthSecondFrame(string copyFromDirectory, string copyToDirectory, int videoId,
            decimal framerate, decimal frameEveryNthSecond, string extension = "jpg")
        {
            string[] frameFiles = Directory.GetFiles(copyFromDirectory);
            Array.Sort(frameFiles);

            if (frameFiles.Length == 0)
            {
                throw new InvalidOperationException("No frames were extracted from video ID: " + videoId);
            }

            decimal frameNumberAccumulator = 0;
            decimal second = 0;
            while (frameNumberAccumulator < frameFiles.Length)
            {
                int frameId = (int)frameNumberAccumulator;
                

                string fileFrom = Path.Combine(copyFromDirectory,
                    frameId.ToString("00000000") 
                    + "." + extension);
                string fileTo = Path.Combine(copyToDirectory,
                    "v" + videoId.ToString("00000") 
                    + "_f" + frameId.ToString("00000")
                    + "_" + second.ToString("0000.00") + "sec"
                    + "." + extension);

                // ffmpeg -start_number 0 not working fix
                if (!File.Exists(fileFrom))
                {
                    Directory.Delete(copyToDirectory, true);
                    Console.Error.WriteLine("Frame {0} in video {1} is missing! Skipping whole video...", 
                        frameId, videoId);
                    return;
                }

                File.Copy(fileFrom, fileTo, true);

                frameNumberAccumulator += (frameEveryNthSecond * framerate);
                second += frameEveryNthSecond;
            }

            // FPS 30000/1001: 
            // numberOfFractions = (framenumber * 1001) % 30000; (where % is modulus)
            // totalseconds = (framenumber * 1001 - numberOfFractions) / 30000;
            // framenumber = (int)(totalseconds * 30000 + numberOfFractions) / 1001;
            
        }
    }
}
