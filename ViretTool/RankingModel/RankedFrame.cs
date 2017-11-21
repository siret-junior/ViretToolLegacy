using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.RankingModel.SimilarityModels;

namespace ViretTool.RankingModel
{
    public class RankedFrame : IComparable<RankedFrame>
    {
        public DataModel.Frame Frame { get; }
        public double Rank { get; set; }
        public byte[] ColorSignature { get; set; }
        public byte[] SemanticDescriptor { get; set; }

        public RankedFrame(DataModel.Frame frame, double rank)
        {
            Frame = frame;
            Rank = rank;
            ColorSignature = null;
            SemanticDescriptor = null;
        }

        public int CompareTo(RankedFrame other)
        {
            return -Rank.CompareTo(other.Rank);
        }

        public static List<RankedFrame> InitializeResultList(DataModel.Dataset dataset)
        {
            List<RankedFrame> result = new List<RankedFrame>();
            result.Capacity = dataset.Frames.Count;

            foreach (DataModel.Frame frame in dataset.Frames)
                result.Add(new RankedFrame(frame, 0));

            return result;
        }

        public static double SemanticDistance(RankedFrame x, RankedFrame y)
        {
            return ByteVectorModel.ComputeDistance(x.SemanticDescriptor, y.SemanticDescriptor);
        }

        public static double ColorDistance(RankedFrame x, RankedFrame y)
        {
            return ColorSignatureModel.ComputeDistance(x.ColorSignature, y.ColorSignature);
        }

        public override string ToString()
        {
            return "ID: " + Frame.ID.ToString("000000") 
                + ", rank: " + Rank.ToString("0.000000")
                + ", video: " + Frame.FrameVideo.VideoID.ToString("00000")
                + ", frame: " + Frame.FrameNumber.ToString("000000");
        }

        public RankedFrame Clone() {
            return new RankedFrame(Frame, Rank);
        }
    }
}
