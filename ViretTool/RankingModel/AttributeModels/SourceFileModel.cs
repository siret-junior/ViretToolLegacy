using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.AttributeModels
{
    class SourceFileModel
    {
        private readonly DataModel.Dataset mDataset;

        /// <summary>
        /// A thumbnail based signature in RGB format, stored as a 1D byte array.
        /// </summary>
        private List<string> mSourceFiles;


        public SourceFileModel(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            mSourceFiles = new List<string>(dataset.Frames.Count);

            string inputFile = dataset.GetFileNameByExtension("-sourceFiles.txt");
            LoadSourceFiles(inputFile);
        }

        private void LoadSourceFiles(string inputFile)
        {
            using (StreamReader reader = new StreamReader(inputFile))
            {
                while (!reader.EndOfStream)
                {
                    mSourceFiles.Add(reader.ReadLine());
                }
            }
        }


        public string GetSourceFile(DataModel.Frame frame)
        {
            return mSourceFiles[frame.ID];
        }
    }
}
