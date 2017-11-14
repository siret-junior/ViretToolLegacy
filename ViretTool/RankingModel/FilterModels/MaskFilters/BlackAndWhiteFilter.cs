using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.FilterModels.MaskFilters {
    class BlackAndWhiteFilter : MaskFilter {

        public BlackAndWhiteFilter(Dataset dataset) : base(dataset) {
            string filename = dataset.AllExtractedFramesFilename.Split('-')[0] + ".bwfilter";

            LoadFromFile(filename, new bool[dataset.Frames.Count]);
        }

        private void LoadFromFile(string filename, bool[] mask) {

            using (var stream = new BinaryReader(File.OpenRead(filename))) {
                // header = 'BWfilter'
                if (stream.ReadInt64() != 0x7265746c69665742)
                    throw new FileFormatException("Invalid bwfilter file format.");
                // threshold number
                stream.ReadInt32();

                while (stream.BaseStream.Position != stream.BaseStream.Length) {
                    int frameId = stream.ReadInt32();
                    mask[frameId] = true;
                }
            }

            Mask = mask;
        }
    }
}
