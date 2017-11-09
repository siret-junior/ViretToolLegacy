using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.DCNNKeywords {
    interface IQueryPart {
        int Id { get; }
        bool UseChildren { get; }
        TextBlockType Type { get; }
    }
    enum TextBlockType { Class, OR, AND }
}
