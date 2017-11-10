using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.FilterModels
{
    class AspectRatioFilter : MaskFilter
    {
        private Dictionary<double, List<Frame>> aspectRatios = new Dictionary<double, List<Frame>>();

        public AspectRatioFilter(Dataset dataset) : base(dataset)
        {
            // TODO:
        }
    }
}
