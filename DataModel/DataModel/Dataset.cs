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
        public readonly int DatasetID;

        /// <summary>
        /// Reader used to read all extracted frames from a binary file lazily.
        /// </summary>
        public readonly FrameReader AllExtractedFramesReader;

        /// <summary>
        /// Loads selected frames into memory and initializes reader of all extracted frames.
        /// </summary>
        /// <param name="selectedFramesFilename"></param>
        /// <param name="allExtractedFramesFilename"></param>
        public Dataset(string selectedFramesFilename, string allExtractedFramesFilename)
        {
            DatasetDirectory = System.IO.Path.GetDirectoryName(selectedFramesFilename);
            SelectedFramesFilename = selectedFramesFilename;
            AllExtractedFramesFilename = allExtractedFramesFilename;
            AllExtractedFramesReader = new FrameReader(allExtractedFramesFilename);

            using (FrameReader selectedFramesReader = new FrameReader(selectedFramesFilename))
            {
                DatasetID = selectedFramesReader.DatasetId;

                Videos = new List<Video>(selectedFramesReader.VideoCount);
                Frames = new List<Frame>(selectedFramesReader.FrameCount);

                CheckFileConsistency(AllExtractedFramesReader, selectedFramesReader);
                LoadVideosAndFrames(selectedFramesReader);
            }
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
        /// Populates the Video and Frame collections of the dataset.
        /// </summary>
        /// <param name="reader"></param>
        private void LoadVideosAndFrames(FrameReader reader)
        {
            int frameCounter = 0;
            for (int i = 0; i < reader.VideoCount; i++)
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
