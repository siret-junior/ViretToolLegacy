using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataModel
{
    class Dataset
    {
        public readonly List<Video> Videos;
        public readonly List<Frame> Frames;

        /// <summary>
        /// The directory, where the dataset file is stored.
        /// </summary>
        public readonly string DatasetDirectory;
        public readonly string AllExtractedFramesFilename;

        /// <summary>
        /// DatasetID represents a unique timestamp associated with the actual set of selected videos and frames.
        /// </summary>
        public readonly int DatasetID;

        public Dataset(string selectedFramesFilename, string allExtractedFramesFilename)
        {
            Videos = new List<Video>();
            Frames = new List<Frame>();
            DatasetDirectory = System.IO.Path.GetDirectoryName(selectedFramesFilename);
            AllExtractedFramesFilename = allExtractedFramesFilename;

            using (System.IO.BinaryReader BR = new System.IO.BinaryReader(System.IO.File.OpenRead(selectedFramesFilename)))
            {
                // TODO - parse DatasetID

                LoadVideosAndFrames(BR);
            }
        }

        private void LoadVideosAndFrames(System.IO.BinaryReader BR)
        {
            // TODO - create videos and frames
        }
    }
}
