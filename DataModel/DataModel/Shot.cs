using System.Collections.ObjectModel;

namespace ViretTool.DataModel
{
    /// <summary>
    /// Represents a single video shot of a video (a sequential set of frames).
    /// </summary>
    public class Shot
    {
        public readonly int Id;

        public Video ParentVideo { get; private set; }
        public int IdInVideo { get; private set; }

        public ReadOnlyCollection<Frame> Frames { get; private set; }
        
        public int StartFrameNumber { get; internal set; }
        public int EndFrameNumber { get; internal set; }


        public Shot(int globalId, int idInVideo = -1, int startFrameNumber = -1, int endFrameNumber = -1)
        {
            Id = globalId;
            IdInVideo = idInVideo;
            StartFrameNumber = startFrameNumber;
            EndFrameNumber = endFrameNumber;
        }


        public override string ToString()
        {
            return "ShotId: " + Id.ToString("00000")
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
                frame.SetParentShotMapping(this, i);
            }

            // TODO: check attributes (frame count) equals frame count?
        }

        
        internal Shot WithStartFrameNumber(int startFrameNumber)
        {
            StartFrameNumber = startFrameNumber;
            return this;
        }

        internal Shot WithEndFrameNumber(int endFrameNumber)
        {
            EndFrameNumber = endFrameNumber;
            return this;
        }

    }
}
