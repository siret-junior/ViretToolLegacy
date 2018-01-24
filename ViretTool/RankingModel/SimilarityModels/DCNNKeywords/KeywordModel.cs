﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.SimilarityModels {
    class KeywordModel {
        private Dictionary<string, KeywordSubModel> mKeywordModels = new Dictionary<string, KeywordSubModel>();

        public KeywordModel(Dataset dataset, string[] sources) {

            foreach (var source in sources) {
                string index = dataset.AllExtractedFramesFilename.Split('-')[0] + "-" + source + ".keyword";
                KeywordSubModel model = new KeywordSubModel(dataset, index, useIDF: true);
                mKeywordModels.Add(source, model);
            }
        }

        public List<RankedFrame> RankFramesBasedOnQuery(List<List<int>> queryKeyword, string source) {
            if (queryKeyword == null) return null;
            return mKeywordModels[source].RankFramesBasedOnQuery(queryKeyword);
        }
    }
}
