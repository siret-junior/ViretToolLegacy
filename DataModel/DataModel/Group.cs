using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataModel
{
    public class Group
    {
        public readonly int GroupID;

        public readonly Video Video;
        public readonly List<Frame> Frames;
        public Frame GroupFrame { get; protected set; }
        

        public Group(Video video, int groupID/*, Frame groupFrame*/)
        {
            Video = video;
            GroupID = groupID;
            //GroupFrame = groupFrame;

            Frames = new List<Frame>();
        }

        public void AddFrame(Frame frame)
        {
            Frames.Add(frame);

            if (GroupFrame == null)
            {
                GroupFrame = frame;
            }
        }



        public override string ToString()
        {
            return "ID: " + GroupID.ToString()
                + ", Video ID: " + Video.VideoID.ToString("00000")
                + ", frames: " + Frames.Count;
        }
    }
}
