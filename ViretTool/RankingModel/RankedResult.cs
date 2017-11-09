using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel
{
    // DONE - Change class to struct if possible
    //        >> RankedFrame will then be considered as value type (not reference type)
    //        >> One pointer jump will be saved for every op with the struct
    //        >> But the Rank value will behave as regular double (thus edit after copy will not effect the value of the original struct)

    // REVERTED because this[].Rank does not work
    public class RankedFrame : IComparable<RankedFrame>
    {
        public DataModel.Frame Frame { get; }
        public double Rank { get; set; }

        public RankedFrame(DataModel.Frame frame, double rank)
        {
            Frame = frame;
            Rank = rank;
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

        public override string ToString()
        {
            return "ID: " + Frame.ID.ToString("000000") 
                + ", rank: " + Rank.ToString("0.000000")
                + ", video: " + Frame.FrameVideo.VideoID.ToString("00000")
                + ", frame: " + Frame.FrameNumber.ToString("000000");
        }
    }
}
