using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.SimilarityModels {
    struct KeywordSearchFrame {
        public int Id { get; }
        public float Rank { get; set; }

        public KeywordSearchFrame(int id, float rank) {
            Id = id;
            Rank = rank;
        }
    }
}
