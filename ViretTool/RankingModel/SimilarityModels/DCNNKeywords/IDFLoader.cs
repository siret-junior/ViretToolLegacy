using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel.SimilarityModels.DCNNKeywords {
    class IDFLoader {

        public static float[] LoadFromFile(string filePath) {
            float[] IDF = null;

            using (BinaryReader stream = new BinaryReader(File.OpenRead(filePath))) {
                int dimension = stream.ReadInt32();
                IDF = new float[dimension];

                float max = float.MinValue;
                for (int i = 0; i < dimension; i++) {
                    IDF[i] = stream.ReadSingle();

                    if (max < IDF[i]) max = IDF[i];
                }

                for (int i = 0; i < dimension; i++) {
                    IDF[i] = (float)Math.Log(max / IDF[i]) + 1;
                }
            }

            return IDF;
        }

    }
}
