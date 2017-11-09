using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    abstract class FilterBase
    {
        protected DataModel.Dataset mDataset;

        public bool Enabled { get; set; }

        public FilterBase(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            Enabled = false;
        }

        //public abstract bool[] GetFilterMask(List<RankedFrame> rankedFrames);
        public abstract List<RankedFrame> ApplyFilter(List<RankedFrame> rankedFrames);
    }
}
