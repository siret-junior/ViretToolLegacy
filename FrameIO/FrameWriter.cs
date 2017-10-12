using System;
using System.IO;

namespace FrameIO
{
    /// <summary>
    /// Writer writing video thumbnails stored in a binary file in the JPEG format.
    /// </summary>
    public class FrameWriter : FrameIOBase
    {
        private readonly BinaryWriter mWriter;

        // total counts
        protected readonly int FrameCount;
        protected readonly int VideoCount;

        // counters
        private int mWrittenFramesCounter = 0;
        private int mWrittenVideosCounter = 0;
        private int mWrittenVideoFramesCounter = 0;
        
        // offsets
        private long mCursorOffset;
        private long mPlaceholderStartOffset;


        /// <summary>
        /// Constructor creating the writer instance. Opens the output file and writes header data.
        /// </summary>
        /// <param name="filename">Filename of the output binary file.</param>
        /// <param name="datasetId">A unique timestamp associated with the actual set of selected videos and frames.</param>
        /// <param name="frameCount">Total number of frames that will be written to the file.</param>
        /// <param name="videoCount">Number of videos that the frames belong to.</param>
        /// <param name="frameWidth">Width of the thumbnail image.</param>
        /// <param name="frameHeight">Width of the thumbnail image. of the thumbnail image.</param>
        /// <param name="framerate">Frequency of the thumbnail extraction in frames per second.</param>
        public FrameWriter(string filename, int datasetId, int frameCount, int videoCount, 
            int frameWidth, int frameHeight, decimal framerate)
        {
            // store count variables
            FrameCount = frameCount;
            VideoCount = videoCount;

            // open binary output file to write
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            mWriter = new BinaryWriter(File.Create(filename));

            // initialize header and placeholder arrays
            mVideoOffsets = new long[videoCount];
            mVideoLengths = new int[videoCount];
            mFrameOffsets = new long[frameCount];
            WriteHeaderPlaceholder(datasetId, frameCount, videoCount, frameWidth, frameHeight, framerate);

            mWriter.Flush();
        }


        /// <summary>
        /// Appends frame data to the output binary stream on the current stream position.
        /// </summary>
        /// <param name="frameData">Image in JPEG format</param>
        /// <param name="videoId">ID of the video this frame belongs to.</param>
        /// <param name="frameNumber">Nuber of the frame in the original video.</param>
        public void AppendFrame(byte[] frameData, int videoId, int frameNumber)
        {
            if (mWrittenFramesCounter >= FrameCount)
            {
                throw new IndexOutOfRangeException("Attempted to write more frames than initially reserved!");
            }

            // update frame offset
            mFrameOffsets[mWrittenFramesCounter] = mCursorOffset;

            // write data
            mWriter.Write(videoId);
            mCursorOffset += sizeof(int);

            mWriter.Write(frameNumber);
            mCursorOffset += sizeof(int);

            mWriter.Write(frameData.Length);
            mCursorOffset += sizeof(int);

            mWriter.Write(frameData);
            mCursorOffset += frameData.Length;

            // increment counters
            mWrittenFramesCounter++;
            mWrittenVideoFramesCounter++;
        }

        /// <summary>
        /// Finishes adding to the current video and initializes writing to the next one.
        /// Similar to initiating a new line using "WriteLine()" in a text writer.
        /// </summary>
        public void NewVideo()
        {
            if (mWrittenVideosCounter >= VideoCount)
            {
                throw new IndexOutOfRangeException("Attempted to write more videos than initially reserved!");
            }
            
            // update placeholder data
            mVideoLengths[mWrittenVideosCounter] = mWrittenVideoFramesCounter;
            if (mWrittenVideosCounter + 1 < VideoCount)
            {
                mVideoOffsets[mWrittenVideosCounter + 1] = mCursorOffset;
            }

            // increment and reset counters
            mWrittenVideosCounter++;
            mWrittenVideoFramesCounter = 0;

            // overwrite the placeholder when all videos are written
            if (mWrittenVideosCounter == VideoCount)
            {
                OverwritePlaceholder();
            }
        }

        /// <summary>
        /// Performs AppendFrame() and NewVideo() in a single method call.
        /// </summary>
        /// <param name="frameData">Image in JPEG format</param>
        /// <param name="videoId">ID of the video this frame belongs to.</param>
        /// <param name="frameNumber">Nuber of the frame in the original video.</param>
        public void AppendFrameNewVideo(byte[] frameData, int videoId, int frameNumber)
        {
            AppendFrame(frameData, videoId, frameNumber);
            NewVideo();
        }
        

        /// <summary>
        /// Disposes the underlying binary writer.
        /// </summary>
        public override void Dispose()
        {
            OverwritePlaceholder();
            mWriter.Dispose();
        }

        
        private void WriteHeaderPlaceholder(int datasetId, int frameCount, int videoCount,
            int frameWidth, int frameHeight, decimal framerate)
        {
            mDataStartOffset = 0;

            mWriter.Write(FILE_DESCRIPTOR);
            mDataStartOffset += FILE_DESCRIPTOR.Length;

            mWriter.Write(datasetId);
            mDataStartOffset += sizeof(int);

            mWriter.Write(frameCount);
            mDataStartOffset += sizeof(int);

            mWriter.Write(videoCount);
            mDataStartOffset += sizeof(int);

            mWriter.Write(frameWidth);
            mDataStartOffset += sizeof(int);

            mWriter.Write(frameHeight);
            mDataStartOffset += sizeof(int);

            mWriter.Write(framerate);
            mDataStartOffset += sizeof(decimal);

            mPlaceholderStartOffset = mDataStartOffset;

            // video offsets placeholder
            mWriter.Write(new byte[videoCount * sizeof(long)]);
            mDataStartOffset += videoCount * sizeof(long);

            // video length (frame count) placeholder
            mWriter.Write(new byte[videoCount * sizeof(int)]);
            mDataStartOffset += videoCount * sizeof(int);

            // frame offsets placeholder
            mWriter.Write(new byte[frameCount * sizeof(long)]);
            mDataStartOffset += frameCount * sizeof(long);

            mCursorOffset = mDataStartOffset;
            mVideoOffsets[0] = mDataStartOffset;
        }

        private void OverwritePlaceholder()
        {
            // seek to the position where placeholder data starts
            mWriter.BaseStream.Seek(mPlaceholderStartOffset, SeekOrigin.Begin);

            // write video offsets
            for (int i = 0; i < VideoCount; i++)
            {
                mWriter.Write(mVideoOffsets[i]);
            }

            // write video lengths (frame counts)
            for (int i = 0; i < VideoCount; i++)
            {
                mWriter.Write(mVideoLengths[i]);
            }

            // write frame offsets
            for (int i = 0; i < FrameCount; i++)
            {
                mWriter.Write(mFrameOffsets[i]);
            }
        }
    }
}
