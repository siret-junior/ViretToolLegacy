using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.SimilarityModels {
    class RankedFrameKW {
        public DataModel.Frame Frame { get; }
        public double Rank { get; set; }

        public RankedFrameKW(DataModel.Frame frame, double rank) {
            Frame = frame;
            Rank = rank;
        }

        public RankedFrameKW Clone() {
            return new RankedFrameKW(Frame, Rank);
        }
    }
}
