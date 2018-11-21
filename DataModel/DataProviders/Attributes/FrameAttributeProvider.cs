using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace DataModel.DataProviders.Attributes
{
    public class FrameAttributeProvider
    {
        public List<int> FrameNumbers = new List<int>();

        public FrameAttributeProvider(string inputFile)
        {
            using (StreamReader reader = new StreamReader(
                File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                while (!reader.EndOfStream)
                {
                    FrameNumbers.Add(int.Parse(reader.ReadLine()));
                }
            }
        }

        public void FillFrameNumbers(Dataset dataset)
        {
            for (int i = 0; i < FrameNumbers.Count; i++)
            {
                Frame frame = dataset.Frames[i];
                frame.FrameNumber = FrameNumbers[i];
            }
        }
    }
}
