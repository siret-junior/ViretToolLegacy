//#define LEGACY

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
#if LEGACY
        public byte[] SemanticDescriptor { get; set; }
#else
        public float[] SemanticDescriptor { get; set; }
#endif

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

        public static List<RankedFrame> InitializeResultList(List<DataModel.Frame> frames)
        {
            List<RankedFrame> result = new List<RankedFrame>(frames.Count);

            foreach (DataModel.Frame frame in frames)
                result.Add(new RankedFrame(frame, 0));

            return result;
        }

        public static double SemanticDistance(RankedFrame x, RankedFrame y)
        {
#if LEGACY
            return ByteVectorModel.ComputeDistance(x.SemanticDescriptor, y.SemanticDescriptor);
#else
            return FloatVectorModel.ComputeDistance(x.SemanticDescriptor, y.SemanticDescriptor);
#endif
        }

    public static double ColorDistance(RankedFrame x, RankedFrame y)
        {
            return ColorSignatureModel.ComputeDistance(x.ColorSignature, y.ColorSignature);
        }

        public override string ToString()
        {
            return "ID: " + Frame.Id.ToString("000000") 
                + ", rank: " + Rank.ToString("0.000000")
                + ", video: " + Frame.ParentVideo.Id.ToString("00000")
                + ", frame: " + Frame.FrameNumber.ToString("000000");
        }

        public RankedFrame Clone() {
            return new RankedFrame(Frame, Rank);
        }
    }
}
