using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataModel
{
    class Frame
    {
        public readonly Video FrameVideo;

        /// <summary>
        /// The global identifier of the frame in a given dataset. For the set of selected frames S ranges from 0 to |S| - 1. ID is set to -1 for all extracted but not selected frames.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// The local identifier of the frame in a given video. Corresponds to the timestamp of the frame.
        /// </summary>
        public readonly int FrameNumber;

        private readonly byte[] mJPGThumbnail;

        public Frame(Video frameVideo, int id, int frameNumber, byte[] JPGThumbnail)
        {
            FrameVideo = frameVideo;
            ID = id;
            FrameNumber = frameNumber;
            mJPGThumbnail = JPGThumbnail;
        }

        public System.Windows.Media.Imaging.BitmapSource GetImage()
        {
            return ImageHelper.StreamToImage(mJPGThumbnail);
        }
    }
}
