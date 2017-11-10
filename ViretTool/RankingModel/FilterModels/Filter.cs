using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.FilterModels
{
    abstract class Filter
    {
        protected DataModel.Dataset mDataset;

        public bool Enabled { get; set; }


        public Filter(DataModel.Dataset dataset)
        {
            mDataset = dataset;
            Enabled = false;
        }
    }
}
