using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.SimilarityModels {
    class KeywordModel {
        private Dictionary<string, KeywordSubModel> mKeywordModels = new Dictionary<string, KeywordSubModel>();

        public KeywordModel(Dataset dataset, Tuple<string, bool>[] sources) {

            foreach (var source in sources) {
                KeywordSubModel model = new KeywordSubModel(dataset, source.Item1, useIDF: source.Item2);
                mKeywordModels.Add(source.Item1, model);
            }
        }

        public List<RankedFrame> RankFramesBasedOnQuery(List<List<int>> queryKeyword, string source) {
            if (queryKeyword == null) return null;
            return mKeywordModels[source].RankFramesBasedOnQuery(queryKeyword);
        }
    }
}
