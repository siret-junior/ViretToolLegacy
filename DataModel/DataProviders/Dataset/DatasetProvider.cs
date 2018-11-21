using DataModel.DataProviders.Attributes;
using FrameIO;
using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.DatasetIO;

namespace ViretTool.DataModel
{
    /// <summary>
    /// Loads the Dataset structure from a file.
    /// </summary>
    public class DatasetProvider
    {
        public static Dataset FromBinaryFile(string inputFilePath)
        {
            using (FileStream inputStream = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Dataset dataset = DatasetBinaryFormatter.Instance.Deserialize(inputStream);
                return dataset;
            }
            
//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }

        public static void ToBinaryFile(Dataset dataset, string outputFilePath)
        {
            using (FileStream outputStream = File.Open(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                DatasetBinaryFormatter.Instance.Serialize(outputStream, dataset);
            }

            //#if DEBUG
            //          TestDataset(Videos, Groups, Frames);
            //#endif
        }


        public static Dataset FromFilelist(string inputFilelistPath, string datasetName)
        {
            using (StreamReader inputStream = new StreamReader(inputFilelistPath))
            {
                DatasetFilelistFormatter formatter = new DatasetFilelistFormatter();
                Dataset dataset = formatter.Deserialize(inputStream, datasetName);

                //using (StreamWriter fnWriter = new StreamWriter("V3C1.framenumbers"))
                //{
                //    foreach (DataModel.Frame frame in dataset.Frames)
                //    {
                //        fnWriter.WriteLine(frame.FrameNumber);
                //    }
                //}

                return dataset;
            }

//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }








        // TODO: temporary legacy code
        // TODO: legacy code:
        public readonly byte[] DatasetFileHeader;
        /// <summary>
        /// Reader used to read all extracted frames from a binary file lazily.
        /// </summary>
        //public FrameReader AllExtractedFramesReader { get; private set; }
        //public FrameReader SelectedFramesReader { get; private set; }


        //public string DatasetDirectory { get; private set; }
        //public string AllExtractedFramesFilename { get; private set; }
        //public string SelectedFramesFilename { get; private set; }
        //public string TopologyFilename { get; private set; }

            

        public static Dataset ConstructDataset(
            string allExtractedFramesFilename,
            string selectedFramesFilename,
            string topologyFilename,
            int maxVideosToLoad = int.MaxValue)
        {
            // load file and directory paths
            string DatasetDirectory = System.IO.Path.GetDirectoryName(selectedFramesFilename);
            string AllExtractedFramesFilename = allExtractedFramesFilename;
            string SelectedFramesFilename = selectedFramesFilename;

            // prepare all frames thumbnail reader
            FrameReader AllExtractedFramesReader = new FrameReader(allExtractedFramesFilename);
            FrameReader SelectedFramesReader = new FrameReader(selectedFramesFilename);

            Dataset dataset = FromBinaryFile(topologyFilename);
            dataset.DatasetDirectory = DatasetDirectory;
            dataset.AllExtractedFramesFilename = AllExtractedFramesFilename;
            dataset.SelectedFramesFilename = SelectedFramesFilename;
            dataset.AllExtractedFramesReader = AllExtractedFramesReader;
            dataset.SelectedFramesReader = SelectedFramesReader;
            maxVideosToLoad = (dataset.Videos.Count < maxVideosToLoad) ? dataset.Videos.Count : maxVideosToLoad;
            dataset.LAST_FRAME_TO_LOAD = dataset.Videos[maxVideosToLoad - 1].Frames.Last().Id;

            FrameAttributeProvider frameAttributeProvider
                = new FrameAttributeProvider(dataset.GetFileNameByExtension(".framenumbers"));
            frameAttributeProvider.FillFrameNumbers(dataset);

            return dataset;
        }

        
    }
}
