using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient {
    public class RankedTimeFrame {
        public RankedFrame RankedFrame;
        public List<Tuple<Frame, int>> LeftFrames;
        public List<Tuple<Frame, int>> RightFrames;
        public int Length;

        public RankedTimeFrame(RankedFrame rankedFrame, List<Tuple<Frame, int>> lf, List<Tuple<Frame, int>> rf, int length) {
            this.RankedFrame = rankedFrame;
            this.LeftFrames = lf;
            this.RightFrames = rf;
            this.Length = length;
        }
    }
}
