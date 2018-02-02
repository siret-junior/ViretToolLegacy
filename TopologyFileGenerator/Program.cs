using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyFileGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputDirectory = Path.GetFullPath(args[0]);
            string groupDirectory = Path.GetFullPath(args[1]);
            string outputFile = Path.GetFullPath(args[2]);

            // TODO: group file


            byte[] header = GenerateFileHeader("TRECVid", new DateTime(2018, 01, 26, 10, 00, 00));

            //if(File.Exists(outputFile))
            //{
            //    Console.WriteLine("Output file already exists!");
            //    return;
            //}

            // count files and directories
            int frameCount, videoCount;
            List<int> videoFrameCounts;
            CountFramesAndVideos(inputDirectory, out frameCount, out videoCount, out videoFrameCounts);

            // load groups
            List<List<List<int>>> groups = LoadGroups(groupDirectory);
            int groupCount = 0;
            for (int iVideo = 0; iVideo < videoCount; iVideo++)
            {
                groupCount += groups[iVideo].Count;
            }

            // load video directories
            string[] videoDirectories = Directory.GetDirectories(inputDirectory);
            Array.Sort(videoDirectories);

            // load video frame counts
            

            using (BinaryWriter writer = new BinaryWriter(File.Create(outputFile)))
            {
                writer.Write(header);

                //**** create global instances  ****************************************
                writer.Write(videoCount);
                writer.Write(groupCount); 
                writer.Write(frameCount);


                //**** create local instances  ****************************************
                // video groups
                for (int i = 0; i < videoCount; i++)
                {
                    writer.Write(groups[i].Count);
                }

                // video frames
                for (int i = 0; i < videoCount; i++)
                {
                    // number of frames in video directory
                    int videoFrameCount = Directory.GetFiles(videoDirectories[i]).Length;
                    writer.Write(videoFrameCount);
                }

                // group frames
                for (int iVideo = 0; iVideo < videoCount; iVideo++)
                {
                    for (int iGroup = 0; iGroup < groups[iVideo].Count; iGroup++)
                    {
                        writer.Write(groups[iVideo][iGroup].Count);
                    }
                }

                //**** load mappings (3 types)  ****************************************
                // video <-> group
                int groupCounter = 0;
                for (int iVideo = 0; iVideo < videoCount; iVideo++)
                {
                    for (int i = 0; i < groups[iVideo].Count; i++)
                    {
                        writer.Write(iVideo);
                        writer.Write(groupCounter++);
                    }
                }

                // video <-> frame
                int frameCounter = 0;
                for (int iVideo = 0; iVideo < videoCount; iVideo++)
                {
                    int videoFrameCount = videoFrameCounts[iVideo];
                    for (int i = 0; i < videoFrameCount; i++)
                    {
                        writer.Write(iVideo);
                        writer.Write(frameCounter++);
                    }
                }

                // group <-> frame
                frameCounter = 0;
                for (int iVideo = 0; iVideo < videoCount; iVideo++)
                {
                    for (int iGroup = 0; iGroup < groups[iVideo].Count; iGroup++)
                    {
                        for (int iFrame = 0; iFrame < groups[iVideo][iGroup].Count; iFrame++)
                        {
                            writer.Write(iGroup);
                            writer.Write(frameCounter++);
                        }
                    }
                }
            }
        }

        private static List<List<List<int>>> LoadGroups(string groupDirectory)
        {
            string[] files = Directory.GetFiles(groupDirectory);
            Array.Sort(files);
            List<List<List<int>>> groups = new List<List<List<int>>>();

            for (int i = 0; i < files.Length; i++)
            {
                List<List<int>> videoGroups = new List<List<int>>();
                groups.Add(videoGroups);

                using (StreamReader reader = new StreamReader(files[i]))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        List<int> group = new List<int>();
                        videoGroups.Add(group);

                        string[] frameIdTokens = line.Split(';');
                        for (int iToken = 0; iToken < frameIdTokens.Length; iToken++)
                        {
                            group.Add(int.Parse(frameIdTokens[iToken]));
                        }
                    }
                }
            }
            return groups;
        }

        static byte[] GenerateFileHeader(string datasetIdAscii16BMax, DateTime timestamp)
        {
            byte[] header = new byte[16 + 20];
            const int TIMESTAMP_INDEX_OFFSET = 16;

            // check dataset ID length
            if (Encoding.ASCII.GetByteCount(datasetIdAscii16BMax) > 16)
            {
                throw new ArgumentException("The dataset ID string is longer than 16 ASCII characters!");
            }

            // write dataset ID
            byte[] datasetIdBytes = Encoding.ASCII.GetBytes(datasetIdAscii16BMax);
            Array.Copy(datasetIdBytes, header, datasetIdBytes.Length);

            // write timestamp
            string pattern = "yyyy-MM-dd HH:mm:ss";
            string timestampString = timestamp.ToString(pattern) + '\n';
            byte[] timestampBytes = Encoding.ASCII.GetBytes(timestampString);
            Array.Copy(timestampBytes, 0, header, TIMESTAMP_INDEX_OFFSET, timestampBytes.Length);

            return header;
        }


        private static void CountFramesAndVideos(string inputDirectory, 
            out int frameCount, out int videoCount, out List<int> videoFrameCounts)
        {
            Console.Write("Counting frames and videos... ");
            frameCount = 0;
            videoCount = 0;
            videoFrameCounts = new List<int>();

            string[] videoDirectories = Directory.GetDirectories(inputDirectory);
            foreach (string videoDirectory in videoDirectories)
            {
                string[] filenames = Directory.GetFiles(videoDirectory);
                int videoFrameCount = filenames.Length;
                videoFrameCounts.Add(videoFrameCount);
                frameCount += videoFrameCount;
                videoCount++;

                if (videoCount % 100 == 0)
                {
                    Console.WriteLine("{0}/{1} done.", videoCount, videoDirectories.Length);
                }
            }
            Console.WriteLine("DONE!");
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
