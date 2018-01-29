using System;
using System.Collections.Generic;
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
        public readonly List<Frame> Frames;

        /// <summary>
        /// The directory, where the dataset file is stored.
        /// </summary>
        public readonly string DatasetDirectory;
        public readonly string AllExtractedFramesFilename;  // TODO: rename
        public readonly string SelectedFramesFilename;

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

        /// <summary>
        /// Loads selected frames into memory and initializes reader of all extracted frames.
        /// </summary>
        /// <param name="selectedFramesFilename"></param>
        /// <param name="allExtractedFramesFilename"></param>
        public Dataset(string selectedFramesFilename, string allExtractedFramesFilename, int videoCount = int.MaxValue)
        {
            DatasetDirectory = System.IO.Path.GetDirectoryName(selectedFramesFilename);
            SelectedFramesFilename = selectedFramesFilename;
            AllExtractedFramesFilename = allExtractedFramesFilename;
            AllExtractedFramesReader = new FrameReader(allExtractedFramesFilename);

            using (FrameReader selectedFramesReader = new FrameReader(selectedFramesFilename))
            {
                Videos = new List<Video>(selectedFramesReader.VideoCount);
                Frames = new List<Frame>(selectedFramesReader.FrameCount);

                DatasetID = selectedFramesReader.DatasetId;

                CheckFileConsistency(AllExtractedFramesReader, selectedFramesReader);

                // TODO - return DatasetFileHeader from LoadVideosAndFrames
                LoadVideosAndFrames(selectedFramesReader, videoCount);
                DatasetFileHeader = GenerateDatasetFileHeader("TRECVid", new DateTime(2018, 01, 26, 10, 00, 00));
            }            
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
            if (UseOldDatasetID)
            {
                int datasetID = BR.ReadInt32();
                return DatasetID == datasetID;
            }
            else
            {
                int headerLength = DatasetFileHeader.Length;
                byte[] header = BR.ReadBytes(headerLength);
                for (int i = 0; i < headerLength; i++)
                    if (header[i] != DatasetFileHeader[i])
                        return false;
            }

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

                result[i] = new Frame(video, -1, frameNumber, jpgThumbnail);
            }

            return result;
        }


        /// <summary>
        /// Populates the Video and Frame collections of the dataset.
        /// </summary>
        /// <param name="reader"></param>
        private void LoadVideosAndFrames(FrameReader reader, int videoCount)
        {
            videoCount = Math.Min(videoCount, reader.VideoCount);

            int frameCounter = 0;
            for (int i = 0; i < videoCount; i++)
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

                    Frame frame = new Frame(video, frameCounter++, frameNumber, jpgThumbnail);
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
    }
}
