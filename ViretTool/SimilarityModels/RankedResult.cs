using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.SimilarityModels
{
    class RankedFrame : IComparable
    {
        public readonly DataModel.Frame Frame;
        public double Rank;

        public RankedFrame(DataModel.Frame frame, double rank)
        {
            Frame = frame;
            Rank = rank;
        }

        public int CompareTo(object obj)
        {
            RankedFrame rf = (RankedFrame)obj;
            return Rank.CompareTo(rf.Rank);
        }

        public static List<RankedFrame> InitializeResultList(DataModel.Dataset dataset)
        {
            List<RankedFrame> result = new List<RankedFrame>();

            foreach (DataModel.Frame frame in dataset.Frames)
                result.Add(new RankedFrame(frame, 0));

            return result;
        }

    }
}
