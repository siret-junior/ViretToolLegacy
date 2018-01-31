using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameIO;

namespace ViretTool.DataModel
{
    public class Dataset : IDisposable
    {
        // TRECVid dataset specific (TODO: remove, use ID -> filename mapping stored in a text file)
        private const int TRECVID_VIDEO_ID_OFFSET = 35345;

        public readonly List<Video> Videos;
        public readonly List<Group> Groups;
        public readonly List<Frame> Frames;

        /// <summary>
        /// The directory, where the dataset file is stored.
        /// </summary>
        public readonly string DatasetDirectory;
        public readonly string AllExtractedFramesFilename;  // TODO: rename
        public readonly string SelectedFramesFilename;
        public readonly string TopologyFilename;

        /// <summary>
        /// DatasetID represents a unique timestamp associated with the actual set of selected videos and frames.
        /// </summary>
        public readonly byte[] DatasetFileHeader;
        public readonly int DatasetID;
        public readonly bool UseOldDatasetID = true; // for compatibility with old DatasetID        

        /// <summary>
        /// Reader used to read all extracted frames from a binary file lazily.
        /// </summary>
        public readonly FrameReader AllExtractedFramesReader;
  
        public int LAST_FRAME_TO_LOAD { get {
                Video lastVideo = Videos[Videos.Count - 1];
                return lastVideo.Frames[lastVideo.Frames.Count - 1].ID;
            }
        }

        /// <summary>
        /// Loads selected frames into memory and initializes reader of all extracted frames.
        /// </summary>
        /// <param name="allExtractedFramesFilename"></param>
        /// <param name="selectedFramesFilename"></param>
        /// <param name="topologyFilename"></param>
        public Dataset(
            string allExtractedFramesFilename, 
            string selectedFramesFilename, 
            string topologyFilename,
            int maxVideoCount = int.MaxValue)
        {
            // load file and directory paths
            DatasetDirectory = System.IO.Path.GetDirectoryName(selectedFramesFilename);
            AllExtractedFramesFilename = allExtractedFramesFilename;
            SelectedFramesFilename = selectedFramesFilename;

            // prepare all frames thumbnail reader
            AllExtractedFramesReader = new FrameReader(allExtractedFramesFilename);

            if (UseOldDatasetID)
            {
                using (FrameReader selectedFramesReader = new FrameReader(selectedFramesFilename))
                {
                    Videos = new List<Video>(selectedFramesReader.VideoCount);
                    Frames = new List<Frame>(selectedFramesReader.FrameCount);

                    DatasetID = selectedFramesReader.DatasetId;

                    CheckFileConsistency(AllExtractedFramesReader, selectedFramesReader);

                    // TODO - return DatasetFileHeader from LoadVideosAndFrames
                    LoadVideosAndFrames(selectedFramesReader, maxVideoCount);
                    DatasetFileHeader = GenerateDatasetFileHeader("TRECVid", new DateTime(2018, 01, 26, 10, 00, 00));
                }
            }
            else
            {
                using (FrameReader selectedFramesReader = new FrameReader(selectedFramesFilename))
                using (BinaryReader topologyReader = new BinaryReader(
                    File.Open(topologyFilename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    DatasetFileHeader = topologyReader.ReadBytes(36);

                    //**** create global instances  ****************************************
                    int videoCount = topologyReader.ReadInt32();
                    int groupCount = topologyReader.ReadInt32();
                    int frameCount = topologyReader.ReadInt32();


                    //**** create local instances  ****************************************
                    // video groups
                    int[] videoGroupCounts = new int[videoCount];
                    for (int i = 0; i < groupCount; i++)
                    {
                        videoGroupCounts[i] = topologyReader.ReadInt32();
                    }

                    // video frames
                    int[] videoFrameCounts = new int[videoCount];
                    for (int i = 0; i < videoCount; i++)
                    {
                        videoFrameCounts[i] = topologyReader.ReadInt32();
                    }

                    // group frames TODO
                    int[] groupFrameCounts = new int[groupCount];
                    for (int i = 0; i < groupCount; i++)
                    {
                        groupFrameCounts[i] = topologyReader.ReadInt32();
                    }


                    //**** load mappings (3 types)  ****************************************
                    // video <-> group
                    Tuple<int, int>[] videoGroupMappings = new Tuple<int, int>[groupCount];
                    for (int i = 0; i < groupCount; i++)
                    {
                        int videoId = topologyReader.ReadInt32();
                        int groupId = topologyReader.ReadInt32();
                        videoGroupMappings[i] = new Tuple<int, int>(videoId, groupId);
                    }

                    // video <-> frame
                    Tuple<int, int>[] videoFrameMappings = new Tuple<int, int>[frameCount];
                    for (int iVideo = 0; iVideo < videoCount; iVideo++)
                    {
                        int videoFrameCount = videoFrameCounts[iVideo];
                        for (int i = 0; i < videoFrameCount; i++)
                        {
                            int videoId = topologyReader.ReadInt32();
                            int frameId = topologyReader.ReadInt32();
                            videoFrameMappings[frameId] = new Tuple<int, int>(videoId, frameId);
                        }
                    }

                    // group <-> frame TODO
                    Tuple<int, int>[] groupFrameMappings = new Tuple<int, int>[frameCount];
                    for (int iVideo = 0; iVideo < videoCount; iVideo++)
                    {
                        int videoFrameCount = videoFrameCounts[iVideo];
                        for (int i = 0; i < videoFrameCount; i++)
                        {
                            int groupId = topologyReader.ReadInt32();
                            int frameId = topologyReader.ReadInt32();
                            groupFrameMappings[frameId] = new Tuple<int, int>(groupId, frameId);
                        }
                    }



                    //**** create instances  ****************************************
                    Videos = new List<Video>(videoCount);
                    Groups = new List<Group>(groupCount);
                    Frames = new List<Frame>(frameCount);

                    int globalGroupId = 0;
                    int globalFrameId = 0;
                    // each video
                    for (int iVideo = 0; iVideo < videoCount; iVideo++)
                    {
                        Video video = new Video(this, (iVideo + TRECVID_VIDEO_ID_OFFSET).ToString("00000") + ".mp4", iVideo);
                        Videos.Add(video);

                        // each group in the video
                        for (int iGroup = 0; iGroup < videoGroupCounts[iVideo]; iGroup++)
                        {
                            Group group = new Group(video, globalGroupId);
                            video.AddGroup(group);
                            Groups.Add(group);

                            // each frame in the group
                            for (int iFrame = 0; iFrame < groupFrameCounts[globalGroupId]; iFrame++)
                            {
                                // load raw frame data from separate thumbnail file
                                Tuple<int, int, byte[]> thumbnailFrame = selectedFramesReader.ReadFrameAt(globalFrameId);
                                int videoId = thumbnailFrame.Item1;
                                int frameNumber = thumbnailFrame.Item2;
                                byte[] jpgData = thumbnailFrame.Item3;

                                // check for consistency
                                if (videoId != video.VideoID)
                                {
                                    throw new IOException("Video ID mismatch!");
                                }

                                // create frame instances
                                Frame frame = new Frame(video, group, globalFrameId, frameNumber, jpgData);
                                group.AddFrame(frame);
                                video.AddFrame(frame);
                                Frames.Add(frame);

                                globalFrameId++;
                            }
                            globalGroupId++;
                        }
                    }



                }
            }
            
        }


        public static byte[] GenerateDatasetFileHeader(string datasetIdAscii16BMax, DateTime timestamp)
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

        /// <summary>
        /// Returns the implicit filename for a file with descriptors or filters.
        /// </summary>
        /// <param name="extension">Filename extension, for example ".color"</param>
        /// <returns></returns>
        public string GetFileNameByExtension(string extension)
        {
            string stripFilename = System.IO.Path.GetFileNameWithoutExtension(AllExtractedFramesFilename);
            string modelFilename = stripFilename.Split('-')[0] + extension;    // TODO: find better solution
            string parentDirectory = System.IO.Directory.GetParent(AllExtractedFramesFilename).ToString();

            return System.IO.Path.Combine(parentDirectory, modelFilename);
        }

        /// <summary>
        /// Checks the header prefix of a file. Moves the cursor to the end of the header.
        /// </summary>
        /// <param name="BR">Binary reader pointing to the beginning of the file stream.</param>
        /// <returns></returns>
        public bool ReadAndCheckFileHeader(System.IO.BinaryReader BR)
        {
            //if (UseOldDatasetID)
            //{
            //    int datasetID = BR.ReadInt32();
            //    return DatasetID == datasetID;
            //}
            //else
            //{
                int headerLength = DatasetFileHeader.Length;
                byte[] header = BR.ReadBytes(headerLength);
                for (int i = 0; i < headerLength; i++)
                    if (header[i] != DatasetFileHeader[i])
                        return false;
                //CheckFileConsistency(AllExtractedFramesReader, selectedFramesReader);
                //LoadVideosAndFrames(selectedFramesReader);
            //}

            return true;
        }

        /// <summary>
        /// Dataset accessor providing direct access to the selected frames.
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public Frame this[int frameId]
        {
            get
            {
                return Frames[frameId];
            }

            set
            {
                Frames[frameId] = value;
            }
        }


        /// <summary>
        /// Reads all extracted frames for a selected video.
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public Frame[] ReadAllVideoFrames(Video video)
        {
            Tuple<int, int, byte[]>[] allFramesRaw = AllExtractedFramesReader.ReadVideoFrames(video.VideoID);
            Frame[] result = new Frame[allFramesRaw.Length];

            for (int i = 0; i < allFramesRaw.Length; i++)
            {
                int videoId = allFramesRaw[i].Item1;
                int frameNumber = allFramesRaw[i].Item2;
                byte[] jpgThumbnail = allFramesRaw[i].Item3;

                result[i] = new Frame(video, null, -1, frameNumber, jpgThumbnail);
            }

            return result;
        }


        /// <summary>
        /// Populates the Video and Frame collections of the dataset.
        /// </summary>
        /// <param name="reader"></param>
        private void LoadVideosAndFrames(FrameReader reader, int maxVideoCount)
        {
            maxVideoCount = Math.Min(maxVideoCount, reader.VideoCount);

            int frameCounter = 0;
            for (int i = 0; i < maxVideoCount; i++)
            {
                // create video and add to the video collection
                Video video = new Video(this, (i + TRECVID_VIDEO_ID_OFFSET).ToString("00000") + ".mp4", i);
                Videos.Add(video);

                // read video frames and add them to the video and the frame collection
                Tuple<int, int, byte[]>[] videoFrames = reader.ReadVideoFrames(i);
                foreach (Tuple<int, int, byte[]> frameData in videoFrames)
                {
                    int videoId = frameData.Item1;
                    int frameNumber = frameData.Item2;
                    byte[] jpgThumbnail = frameData.Item3;

                    Frame frame = new Frame(video, null, frameCounter++, frameNumber, jpgThumbnail);
                    video.AddFrame(frame);
                    Frames.Add(frame);
                }
            }
        }

        private void CheckFileConsistency(FrameReader fileA, FrameReader fileB)
        {
            // TODO - use new header

            if (fileA.DatasetId != fileB.DatasetId)
            {
                throw new FormatException("Dataset IDs do not match: " 
                    + fileA.DatasetId + " vs. " + fileB.DatasetId);
            }

            if (fileA.VideoCount != fileB.VideoCount)
            {
                throw new FormatException("Video counts do not match: "
                    + fileA.VideoCount + " vs. " + fileB.VideoCount);
            }
        }

        public void Dispose()
        {
            if (AllExtractedFramesReader != null)
            {
                AllExtractedFramesReader.Dispose();
            }
        }

    }
}
