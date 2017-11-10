using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataModel {
    public class Frame {
        public Video FrameVideo { get; }

        /// <summary>
        /// The global identifier of the frame in a given dataset. For the set of selected frames S ranges from 0 to |S| - 1. ID is set to -1 for all extracted but not selected frames.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// The local identifier of the frame in a given video. Corresponds to the timestamp of the frame.
        /// </summary>
        public int FrameNumber { get; }

        private byte[] mJPGThumbnail { get; }

        public Frame(Video frameVideo, int id, int frameNumber, byte[] JPGThumbnail) {
            FrameVideo = frameVideo;
            ID = id;
            FrameNumber = frameNumber;
            mJPGThumbnail = JPGThumbnail;
        }

        public System.Windows.Media.Imaging.BitmapSource GetImage()
        {
            return ImageHelper.StreamToImage(mJPGThumbnail);
        }

        public override string ToString()
        {
            return "ID: " + ID.ToString("00000") 
                + ", frame: " + FrameNumber.ToString("00000") 
                + ", Video ID: " + FrameVideo.VideoID.ToString("00000");
        }

        public System.Windows.Media.Imaging.BitmapSource Bitmap {
            get {
                return ImageHelper.StreamToImage(mJPGThumbnail);
            }
        }
    }
}
