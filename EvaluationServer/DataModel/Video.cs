using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataModel
{
    public class Video
    {
        public readonly Dataset VideoDataset;
        public readonly List<Frame> Frames;

        public readonly string Name;
        public readonly int VideoID;

        public Video(Dataset videoDataset, string name, int videoID)
        {
            VideoDataset = videoDataset;
            Name = name;
            VideoID = videoID;

            Frames = new List<Frame>();
        }

        public void AddFrame(Frame frame)
        {
            Frames.Add(frame);
        }

        public List<Frame> GetAllExtractedFrames()
        {
            List<Frame> allExtractedFrames = new List<Frame>();

            // open VideoDataset.AllExtractedFramesFilename
            // parse frames from the big file
            // set frame ID = -1
            Tuple<int, int, byte[]>[] videoFrames = VideoDataset.AllExtractedFramesReader.ReadVideoFrames(VideoID);
            foreach (Tuple<int, int, byte[]> frameData in videoFrames)
            {
                int videoId = frameData.Item1;
                int frameNumber = frameData.Item2;
                byte[] jpgThumbnail = frameData.Item3;

                Frame frame = new Frame(this, -1, frameNumber, jpgThumbnail);
                allExtractedFrames.Add(frame);
            }

            return allExtractedFrames;
        }


        public override string ToString()
        {
            return "ID: " + VideoID.ToString("00000") + ", frames: " + Frames.Count + ", (" + Name + ")";
        }
    }
}
