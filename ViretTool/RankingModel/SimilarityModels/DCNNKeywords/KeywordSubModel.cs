using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.RankingModel.SimilarityModels {

    /// <summary>
    /// Searches an index file and displays results
    /// </summary>
    class KeywordSubModel {

        private bool mUseIDF;
        private string mIndexFilePath;

        private Dataset mDataset;
        /// <summary>
        /// Maps class ID from query to list of frames containing the class
        /// </summary>
        private Dictionary<int, List<RankedFrame>> mClasses;
        private Task mLoadTask;

        private float[] IDF;

        /// <param name="lp">For class name to class id conversion</param>
        /// <param name="filePath">Relative or absolute path to index file</param>
        public KeywordSubModel(Dataset dataset, string filePath, bool useIDF = false) {
            mIndexFilePath = filePath;
            mDataset = dataset;
            mClasses = new Dictionary<int, List<RankedFrame>>();
            mUseIDF = useIDF;

            mLoadTask = Task.Factory.StartNew(LoadFromFile);
        }

        #region Rank Methods

        public List<RankedFrame> RankFramesBasedOnQuery(List<List<int>> query) {
            if (mLoadTask.IsFaulted) {
                throw mLoadTask.Exception.InnerException;
            }
            if (!mLoadTask.IsCompleted || query == null) {
                return RankedFrame.InitializeResultList(mDataset.Frames);
            }

            return GetRankedFrames(query);
        }

        #endregion


        #region (Private) Index File Loading

        private void LoadFromFile() {
            BinaryReader stream = null;
            Dictionary<int, int> classLocations = new Dictionary<int, int>();

            if (mUseIDF) {
                if (!File.Exists(mIndexFilePath + ".idf")) mUseIDF = false;
                else IDF = DCNNKeywords.IDFLoader.LoadFromFile(mIndexFilePath + ".idf");
            }

            try {
                stream = new BinaryReader(File.OpenRead(mIndexFilePath));

                // header = 'KS INDEX'+(Int64)-1
                if (stream.ReadInt64() != 0x4b5320494e444558 && stream.ReadInt64() != -1)
                    throw new FileFormatException("Invalid index file format.");

                // read offests of each class
                while (true) {
                    int value = stream.ReadInt32();
                    int valueOffset = stream.ReadInt32();

                    if (value != -1) {
                        classLocations.Add(valueOffset, value);
                    } else break;
                }

                while (stream.BaseStream.Position != stream.BaseStream.Length) {

                    // list of class offets does not contain this one
                    if (!classLocations.ContainsKey((int)stream.BaseStream.Position))
                        throw new FileFormatException("Invalid index file format.");

                    int classId = classLocations[(int)stream.BaseStream.Position];
                    mClasses.Add(classId, new List<RankedFrame>());

                    // add all images
                    while (true) {
                        uint imageId = (uint)stream.ReadInt32();
                        float imageProbability = stream.ReadSingle();

                        if (imageId != 0xffffffff) {

                            Frame f = mDataset.Frames[(int)imageId];
                            RankedFrame rf = new RankedFrame(f, imageProbability);
                            mClasses[classId].Add(rf);
                        } else break;
                    }
                }
            } finally {
                stream.Dispose();
            }
        }

        #endregion

        #region (Private) List Unions & Multiplications

        private List<RankedFrame> GetRankedFrames(List<List<int>> ids) {
            List<RankedFrame> res = RankedFrame.InitializeResultList(mDataset.Frames);

            List<Dictionary<int, RankedFrame>> clauses = ResolveClauses(ids);
            Dictionary<int, RankedFrame> query = UniteClauses(clauses);

            foreach (KeyValuePair<int, RankedFrame> pair in query) {
                res[pair.Key] = pair.Value;
            }
            
            return res;
        }


        private Dictionary<int, RankedFrame> UniteClauses(List<Dictionary<int, RankedFrame>> clauses) {
            var result = clauses[clauses.Count - 1];
            clauses.RemoveAt(clauses.Count - 1);

            foreach (Dictionary<int, RankedFrame> clause in clauses) {
                Dictionary<int, RankedFrame> tempResult = new Dictionary<int, RankedFrame>();

                foreach (KeyValuePair<int, RankedFrame> rf in clause) {
                    RankedFrame rfFromResult;
                    if (result.TryGetValue(rf.Value.Frame.ID, out rfFromResult)) {
                        tempResult.Add(rf.Value.Frame.ID, new RankedFrame(rf.Value.Frame, rf.Value.Rank * rfFromResult.Rank));
                    }
                }
                result = tempResult;
            }
            return result;
        }

        private List<Dictionary<int, RankedFrame>> ResolveClauses(List<List<int>> ids) {
            var list = new List<Dictionary<int, RankedFrame>>();

            // should be fast
            // http://alicebobandmallory.com/articles/2012/10/18/merge-collections-without-duplicates-in-c
            foreach (List<int> listOfIds in ids) {
                int i = 0;
                // there can be classes with no frames, skip them
                while (i < listOfIds.Count && !mClasses.ContainsKey(listOfIds[i])) { i++; }

                // no class with a frame found
                if (i == listOfIds.Count) {
                    list.Add(new Dictionary<int, RankedFrame>());
                    continue;
                }

                Dictionary<int, RankedFrame> dict = new Dictionary<int, RankedFrame>(); //= mClasses[listOfIds[i]].ToDictionary(f => f.Frame.ID);
                
                for (; i < listOfIds.Count; i++) {
                    if (!mClasses.ContainsKey(listOfIds[i])) continue;

                    if (mUseIDF) {
                        float idf = IDF[listOfIds[i]];
                        foreach (RankedFrame f in mClasses[listOfIds[i]]) {
                            if (dict.ContainsKey(f.Frame.ID)) {
                                dict[f.Frame.ID].Rank += f.Rank * idf;
                            } else {
                                var frame = f.Clone();
                                frame.Rank *= idf;
                                dict.Add(f.Frame.ID, frame);
                            }
                        }
                    } else {
                        foreach (RankedFrame f in mClasses[listOfIds[i]]) {
                            if (dict.ContainsKey(f.Frame.ID)) {
                                dict[f.Frame.ID].Rank += f.Rank;
                            } else {
                                dict.Add(f.Frame.ID, f.Clone());
                            }
                        }
                    }
                }
                list.Add(dict);
            }
            return list;
        }

        #endregion

    }

}
