using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    abstract class FlowFilter : Filter
    {
        public FlowFilter(DataModel.Dataset dataset) : base(dataset)
        {
        }

        public abstract List<RankedFrame> ApplyFilter(List<RankedFrame> sortedRankedFrames);
    }
}
