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
            string outputFile = Path.GetFullPath(args[1]);

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

            // load video directories
            string[] videoDirectories = Directory.GetDirectories(inputDirectory);
            Array.Sort(videoDirectories);

            // load video frame counts
            

            using (BinaryWriter writer = new BinaryWriter(File.Create(outputFile)))
            {
                writer.Write(header);

                //**** create global instances  ****************************************
                writer.Write(videoCount);
                int groupCount = videoCount;// TODO group count
                writer.Write(groupCount); 
                writer.Write(frameCount);


                //**** create local instances  ****************************************
                // video groups
                for (int i = 0; i < groupCount; i++)
                {
                    writer.Write(1); // TODO
                }

                // video frames
                for (int i = 0; i < videoCount; i++)
                {
                    writer.Write(Directory.GetFiles(videoDirectories[i]).Length);
                }

                // group frames TODO
                for (int i = 0; i < groupCount; i++)
                {
                    writer.Write(Directory.GetFiles(videoDirectories[i]).Length);
                }


                //**** load mappings (3 types)  ****************************************
                // video <-> group
                for (int i = 0; i < groupCount; i++)
                {
                    writer.Write(i);    // TODO
                    writer.Write(i);
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

                // group <-> frame TODO
                frameCounter = 0;
                for (int iVideo = 0; iVideo < videoCount; iVideo++)
                {
                    int videoFrameCount = videoFrameCounts[iVideo];
                    for (int i = 0; i < videoFrameCount; i++)
                    {
                        writer.Write(iVideo);
                        writer.Write(frameCounter++);
                    }
                }
            }
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
