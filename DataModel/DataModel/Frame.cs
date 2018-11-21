namespace ViretTool.DataModel
{
    /// <summary>
    /// A representative frame selected from a shot in a video.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// The global identifier of the representative frame in a given dataset.
        /// </summary>
        public readonly int Id;
        
        /// <summary>
        /// Number of the frame in the source video.
        /// </summary>
        public int FrameNumber { get; internal set; }


        // Parent mappings
        public Video ParentVideo { get; private set; }
        public int IdInVideo { get; private set; }

        public Shot ParentShot { get; private set; }
        public int IdInShot { get; private set; }

        public Group ParentGroup { get; private set; } 
        public int IdInGroup { get; private set; }

        
        public Frame(int globalId, int frameNumber = -1)
        {
            Id = globalId;
            FrameNumber = frameNumber;
        }


        public override string ToString()
        {
            return "FrameId: " + Id.ToString()
                + ", Video: " + ParentVideo.Id.ToString("00000")
                + ", Shot: " + ParentShot.Id.ToString("00000")
                + ", Group: " + ParentGroup.Id.ToString("00000");
        }


        internal void SetParentVideoMapping(Video parentVideo, int idInVideo)
        {
            ParentVideo = parentVideo;
            IdInVideo = idInVideo;
        }

        internal void SetParentShotMapping(Shot parentShot, int idInShot)
        {
            ParentShot = parentShot;
            IdInShot = idInShot;
        }

        internal void SetParentGroupMapping(Group parentGroup, int idInGroup)
        {
            ParentGroup = parentGroup;
            IdInGroup = idInGroup;
        }


        internal void WithFrameNumber(int frameNumber)
        {
            FrameNumber = frameNumber;
        }


        // TODO: legacy code //////////////////////////////////////////////////////////////////////
        public byte[] JPGThumbnail { get; set; }

        public Frame(int id, Group frameGroup, Video frameVideo, int frameNumber, byte[] jpgThumbnail)
        {
            Id = id;
            ParentGroup = frameGroup;
            ParentVideo = frameVideo;
            FrameNumber = frameNumber;
            JPGThumbnail = jpgThumbnail;
        }


        public System.Windows.Media.Imaging.BitmapSource Bitmap
        {
            get
            {
                if (JPGThumbnail == null)
                {
                    JPGThumbnail = ParentVideo.ParentDataset.SelectedFramesReader.ReadFrameAt(Id).Item3;
                }
                return ImageHelper.StreamToImage(JPGThumbnail);
            }
        }

        public System.Drawing.Bitmap ActualBitmap
        {
            get
            {
                if (JPGThumbnail == null)
                {
                    JPGThumbnail = ParentVideo.ParentDataset.SelectedFramesReader.ReadFrameAt(Id).Item3;
                }
                return new System.Drawing.Bitmap(new System.IO.MemoryStream(JPGThumbnail));
            }
        }
        
        public System.Windows.Media.Imaging.BitmapSource GetImage()
        {
            if (JPGThumbnail == null)
            {
                JPGThumbnail = ParentVideo.ParentDataset.SelectedFramesReader.ReadFrameAt(Id).Item3;
            }
            return ImageHelper.StreamToImage(JPGThumbnail);
        }

    }
}
