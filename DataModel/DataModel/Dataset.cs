using FrameIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ViretTool.DataModel
{
    /// <summary>
    /// Holds a tree structure of the input video dataset.
    /// 
    /// Simple tree structure description:
    /// A: The videos are split into sequential shots from which were selected its representative frames.
    /// Dataset 
    ///     -> Videos (sequentially ordered) 
    ///     -> Shots (sequentially ordered)
    ///     -> Frames (sequentially ordered)
    /// B: The representative frames are joined into groups of similar frames.
    /// Dataset 
    ///     -> Videos (sequentially ordered)
    ///     -> Groups 
    ///     -> Frames 
    /// 
    /// Extended tree structure explanation (used in the extraction phase):
    /// The representative frames are a subset of a set of playback frames (k-FramesPerSecond).
    /// The playback frames are a subset of all frames from a video.
    /// Dataset 
    ///     -> Videos 
    ///     -> Shots/Groups
    ///     -> (Representative) Frames (of shots) 
    ///     -[subset]-> Playback Frames (k-FPS)
    ///     -[subset]-> All video file frames (variable FPS per video)
    /// </summary>
    public class Dataset
    {
        /// <summary>
        /// DatasetID represents a unique timestamp associated with 
        /// the actual input video dataset and the extraction time.
        /// </summary>
        public readonly byte[] DatasetId;
        // TODO: datasetId into a separate object + ToString() method?
        // TODO: interpret datasetId (datasetName:string + timestamp:DateTime)?        

        // heirarchy
        public readonly ReadOnlyCollection<Video> Videos;
        public readonly ReadOnlyCollection<Shot> Shots;
        public readonly ReadOnlyCollection<Group> Groups;
        public readonly ReadOnlyCollection<Frame> ReadOnlyFrames;
        public readonly List<Frame> Frames;




        // TODO: temporary legacy code ////////////////////////////////////////////////////
        /// <summary>
        /// Reader used to read all extracted frames from a binary file lazily.
        /// </summary>
        public FrameReader AllExtractedFramesReader { get; set; }
        public FrameReader SelectedFramesReader { get; set; }
        
        public string DatasetDirectory { get; set; }
        public string AllExtractedFramesFilename { get; set; }
        public string SelectedFramesFilename { get; set; }
        public string TopologyFilename { get; set; }

        public int LAST_FRAME_TO_LOAD { get; set; }
        ///////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Create the dataset structure from already preloaded items.
        /// </summary>
        /// <param name="datasetId">
        ///     An unique identifier associated with the actual input video dataset and the extraction time.</param>
        /// <param name="videos">Objects representing dataset videos (sequentially ordered).</param>
        /// <param name="shots">Objects holding individual shots of videos (sequentially ordered).</param>
        /// <param name="groups">Groups of similar representative frames.</param>
        /// <param name="frames">Representative frames of shots (sequentially ordered).</param>
        public Dataset(byte[] datasetId, Video[] videos, Shot[] shots, Group[] groups, Frame[] frames)
        {
            // TODO: validate datasetId
            DatasetId = datasetId;

            // wrap input arrays into readonly collections.
            Videos = new ReadOnlyCollection<Video>(videos);
            Shots = new ReadOnlyCollection<Shot>(shots);
            Groups = new ReadOnlyCollection<Group>(groups);
            ReadOnlyFrames = new ReadOnlyCollection<Frame>(frames);

            // TODO: legacy code
            Frames = ReadOnlyFrames.ToList();
            LAST_FRAME_TO_LOAD = int.MaxValue;

            foreach (Video video in videos)
            {
                video.ParentDataset = this;
            }
        }



        // TODO: temporary legacy code //////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks the header prefix of a file. Moves the cursor to the end of the header.
        /// </summary>
        /// <param name="BR">Binary reader pointing to the beginning of the file stream.</param>
        /// <returns></returns>
        public bool ReadAndCheckFileHeader(System.IO.BinaryReader BR)
        {
            //int headerLength = DatasetFileHeader.Length;
            int headerLength = BR.ReadInt32();

            byte[] header = BR.ReadBytes(headerLength);
            for (int i = 0; i < headerLength; i++)
            {
                if (header[i] != DatasetId[i])
                {
                    return false;
                }
            }

            return true;
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




        public List<Frame> GetAllExtractedFrames(int videoId)
        {
            Video video = Videos[videoId];
            List<Frame> allExtractedFrames = new List<Frame>();

            // open VideoDataset.AllExtractedFramesFilename
            // parse frames from the big file
            // set frame ID = -1
            Tuple<int, int, byte[]>[] videoFrames = AllExtractedFramesReader.ReadVideoFrames(videoId);
            foreach (Tuple<int, int, byte[]> frameData in videoFrames)
            {
                int videoIdOfTheFrame = frameData.Item1;
                int frameNumber = frameData.Item2;
                byte[] jpgThumbnail = frameData.Item3;

                Frame frame = new Frame(-1, null, video, frameNumber, jpgThumbnail);
                allExtractedFrames.Add(frame);
            }

            return allExtractedFrames;
        }



        /// <summary>
        /// Reads all extracted frames for a selected video.
        /// </summary>
        /// <param name="video"></param>
        /// <returns></returns>
        public Frame[] ReadAllVideoFrames(Video video)
        {
            Tuple<int, int, byte[]>[] allFramesRaw = AllExtractedFramesReader.ReadVideoFrames(video.Id);
            Frame[] result = new Frame[allFramesRaw.Length];

            for (int i = 0; i < allFramesRaw.Length; i++)
            {
                int videoId = allFramesRaw[i].Item1;
                int frameNumber = allFramesRaw[i].Item2;
                byte[] jpgThumbnail = allFramesRaw[i].Item3;

                result[i] = new Frame(-1, null, video, frameNumber, jpgThumbnail);
            }

            return result;
        }
    }
}
