using System;
using System.IO;
//using System.Windows.Media.Imaging;
//using System.Drawing;

namespace FrameIO
{
    /// <summary>
    /// Reader reading video thumbnails stored in a binary file in the JPEG format.
    /// </summary>
    public class FrameReader : FrameIOBase
    {
        private BinaryReader mReader;

        private object mLock = new object();

        /// <summary>
        /// A unique timestamp associated with the actual set of selected videos and frames.
        /// </summary>
        public readonly int DatasetId;

        /// <summary>
        /// Total count of all frames stored in the binary file.
        /// </summary>
        public readonly int FrameCount;

        /// <summary>
        /// Number of stored videos. Video is a continuous sequence of frames.
        /// </summary>
        public readonly int VideoCount;

        /// <summary>
        /// Width of stored thumbnail images.
        /// </summary>
        public readonly int FrameWidth;

        /// <summary>
        /// Height of stored thumbnail images.
        /// </summary>
        public readonly int FrameHeight;

        /// <summary>
        /// Framerate of stored videos (frames per second).
        /// </summary>
        public readonly decimal Framerate;


        /// <summary>
        /// Opens the binary file and reads its header to populate the public fields.
        /// </summary>
        /// <param name="filename">Filename to read binary data from.</param>
        public FrameReader(string filename)
        {
            // open the binary file as a shared stream to allow multiple readers access it.
            mReader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
            mDataStartOffset = 0;

            // check file descriptor
            char[] fileDescriptor = mReader.ReadChars(FILE_DESCRIPTOR.Length);
            mDataStartOffset += FILE_DESCRIPTOR.Length;
            for (int i = 0; i < fileDescriptor.Length; i++)
            {
                if (fileDescriptor[i] != FILE_DESCRIPTOR[i])
                {
                    throw new IOException("Incorrect file descriptor! Not a merged thumbnails file?");
                }
            }

            // read file info
            DatasetId = mReader.ReadInt32();
            mDataStartOffset += sizeof(int);

            FrameCount = mReader.ReadInt32();
            mDataStartOffset += sizeof(int);

            VideoCount = mReader.ReadInt32();
            mDataStartOffset += sizeof(int);

            FrameWidth = mReader.ReadInt32();
            mDataStartOffset += sizeof(int);

            FrameHeight = mReader.ReadInt32();
            mDataStartOffset += sizeof(int);

            Framerate = mReader.ReadDecimal();
            mDataStartOffset += sizeof(decimal);

            mVideoOffsets = new long[VideoCount];
            for (int i = 0; i < VideoCount; i++)
            {
                mVideoOffsets[i] = mReader.ReadInt64();
            }
            mDataStartOffset += VideoCount * sizeof(long);

            mVideoLengths = new int[VideoCount];
            for (int i = 0; i < VideoCount; i++)
            {
                mVideoLengths[i] = mReader.ReadInt32();
            }
            mDataStartOffset += VideoCount * sizeof(int);

            mFrameOffsets = new long[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                mFrameOffsets[i] = mReader.ReadInt64();
            }
            mDataStartOffset += FrameCount * sizeof(long);
        }


        /// <summary>
        /// Access a frame by its global ID.
        /// </summary>
        /// <param name="globalId">The global identifier of the frame in a given dataset. 
        /// For the set of selected frames S ranges from 0 to |S| - 1.</param>
        /// <returns></returns>
        public Tuple<int, int, byte[]> this[int globalId]
        {
            get
            {
                return ReadFrameAt(globalId);
            }
        }

        /// <summary>
        /// Reads the frame from a defined position in the binary file.
        /// </summary>
        /// <param name="globalId">The global identifier of the frame in a given dataset. 
        /// For the set of selected frames S ranges from 0 to |S| - 1.</param>
        /// <returns>A triple of values: 
        /// Video ID the frame belongs to, 
        /// number of the frame in the original video,
        /// thumbnail encoded as a JPEG image.</returns
        public Tuple<int, int, byte[]> ReadFrameAt(int globalId)
        {
            lock (mLock)
            {
                long frameOffset = mFrameOffsets[globalId];
                mReader.BaseStream.Seek(frameOffset, SeekOrigin.Begin);

                return ReadNextFrame();
            }
        }

        /// <summary>
        /// Reads the frame from current position in the binary file.
        /// Returns a triple of values: 
        /// Video ID the frame belongs to, 
        /// number of the frame in the original video,
        /// thumbnail encoded as a JPEG image.
        /// </summary>
        /// <returns>A triple of values: 
        /// Video ID the frame belongs to, 
        /// number of the frame in the original video,
        /// thumbnail encoded as a JPEG image.</returns>
        public Tuple<int, int, byte[]> ReadNextFrame()
        {
            lock (mLock)
            {
                int videoId = mReader.ReadInt32();
                int frameNumber = mReader.ReadInt32();
                int dataLength = mReader.ReadInt32();
                byte[] jpgData = mReader.ReadBytes(dataLength);

                return new Tuple<int, int, byte[]>(videoId, frameNumber, jpgData);
            }
        }

        /// <summary>
        /// Reads all frames of a video.
        /// Returns a triple of values for each video: 
        /// Video ID the frame belongs to, 
        /// number of the frame in the original video,
        /// thumbnail encoded as a JPEG image.
        /// </summary>
        /// <param name="videoId">Identifier of the video in a dataset.
        /// For the set of selected videos S ranges from 0 to |S| - 1.</param>
        /// <returns>A triple of values for each video: 
        /// Video ID the frame belongs to, 
        /// number of the frame in the original video,
        /// thumbnail encoded as a JPEG image.</returns>
        public Tuple<int, int, byte[]>[] ReadVideoFrames(int videoId)
        {
            lock (mLock)
            {
                // seek the stream to the video start position
                long videoOffset = mVideoOffsets[videoId];
                mReader.BaseStream.Seek(videoOffset, SeekOrigin.Begin);

                // prepare result array
                int frameCount = mVideoLengths[videoId];
                Tuple<int, int, byte[]>[] result = new Tuple<int, int, byte[]>[frameCount];

                // fill result array
                for (int i = 0; i < frameCount; i++)
                {
                    result[i] = ReadNextFrame();
                }

                return result;
            }
        }


        ///// <summary>
        ///// Converts the JPEG data into an Image bitmap.
        ///// </summary>
        ///// <param name="jpgData">Image stored in JPEG format.</param>
        ///// <returns>Converted Image bitmap.</returns>
        //public static Image ConvertToImage(byte[] jpgData)
        //{
        //    using (MemoryStream stream = new MemoryStream(jpgData))
        //    {
        //        Image result = Image.FromStream(stream);
        //        return result;
        //    }
        //}

        ///// <summary>
        ///// Converts the JPEG data into a BitmapSource bitmap.
        ///// </summary>
        ///// <param name="jpgData">Image stored in JPEG format.</param>
        ///// <returns>Converted BitmapSource bitmap.</returns>
        //public static BitmapSource ConvertToBitmapSource(byte[] jpgData)
        //{
        //    using (MemoryStream stream = new MemoryStream(jpgData))
        //    {
        //        JpegBitmapDecoder decoder 
        //            = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //        BitmapSource bitmapSource = decoder.Frames[0];
        //        return bitmapSource;
        //    }
        //}


        /// <summary>
        /// Disposes the underlying binary reader.
        /// </summary>
        public override void Dispose()
        {
            mReader.Dispose();
        }
    }
}
