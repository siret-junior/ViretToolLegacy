using System;

namespace FrameIO
{
    /// <summary>
    /// Base abstract class for the operations on a binary file storing thumbnails for multiple videos.
    /// </summary>
    public abstract class FrameIOBase : IDisposable
    {
        /// <summary>
        /// A file descriptor written at the beginning of the binary file.
        /// </summary>
        protected readonly char[] FILE_DESCRIPTOR = "Video thumbnails".ToCharArray();
        
        /// <summary>
        /// Offset where the header ends and frame data starts.
        /// </summary>
        protected long mDataStartOffset;

        /// <summary>
        /// Additional header arrays storing the video-frame tree structure.
        /// </summary>
        protected long[] mVideoOffsets;
        protected int[] mVideoLengths;
        protected long[] mFrameOffsets;

        /// <summary>
        /// Enforcing of the IDisposable interface.
        /// </summary>
        public abstract void Dispose();
    }
}
