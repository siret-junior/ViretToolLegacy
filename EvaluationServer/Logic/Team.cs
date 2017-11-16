using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VitretTool.EvaluationServer {
    class Team {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public Color Color { get; }

        private VBSTasks.KeyframeEvaluator mEvaluator;

        public Team(long id, string name, VBSTasks.KeyframeEvaluator evaluator, Color c) {
            Id = id;
            Name = name;
            mEvaluator = evaluator;
            Color = c; //Helper.StringToColor(Name);
        }

        public void SubmitVideoFrame(int videoId, int frameId) {
            mEvaluator(Id, videoId, frameId);
        }
    }
}
