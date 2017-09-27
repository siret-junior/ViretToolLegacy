using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataModel
{
    class Video
    {
        public readonly Dataset VideoDataset;
        public readonly List<Frame> Frames;

        public readonly string Name;
        public readonly int VideoID;

        public Video(Dataset videoDataset, string name, int videoID)
        {
            Frames = new List<Frame>();
            Name = name;
            VideoID = videoID;
        }

        public List<Frame> GetAllExtractedFrames()
        {
            List<Frame> allExtractedFrames = new List<Frame>();

            // open VideoDataset.AllExtractedFramesFilename
            // parse frames from the big file
            // set frame ID = -1

            return allExtractedFrames;
        }
    }
}
