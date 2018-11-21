using System.Collections.ObjectModel;

namespace ViretTool.DataModel
{
    /// <summary>
    /// Represents a group of similar representative frames from a video.
    /// </summary>
    public class Group
    {
        public readonly int Id;
        
        public Video ParentVideo { get; private set; }
        public int IdInVideo { get; private set; }

        public ReadOnlyCollection<Frame> Frames { get; private set; }


        public Group(int groupId)
        {
            Id = groupId;
        }


        public override string ToString()
        {
            return "GroupId: " + Id.ToString("00000")
                + ", Video: " + ParentVideo.Id.ToString("00000");
        }


        internal void SetParentVideoMapping(Video parentVideo, int idInVideo)
        {
            ParentVideo = parentVideo;
            IdInVideo = idInVideo;
        }

        internal void SetFrameMappings(Frame[] frames)
        {
            Frames = new ReadOnlyCollection<Frame>(frames);
            for (int i = 0; i < frames.Length; i++)
            {
                Frame frame = frames[i];
                frame.SetParentGroupMapping(this, i);
            }
        }
    }
}
