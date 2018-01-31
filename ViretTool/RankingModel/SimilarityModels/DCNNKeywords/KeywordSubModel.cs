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
        private string mSource;
        private Dataset mDataset;
        /// <summary>
        /// Maps class ID from query to list of frames containing the class
        /// </summary>
        private Dictionary<int, List<RankedFrameKW>> mClasses = new Dictionary<int, List<RankedFrameKW>>();
        //private Task mLoadTask;

        private float[] IDF;

        /// <param name="lp">For class name to class id conversion</param>
        /// <param name="filePath">Relative or absolute path to index file</param>
        public KeywordSubModel(Dataset dataset, string source, bool useIDF = false) {
            mDataset = dataset;
            mSource = source;
            mUseIDF = useIDF;

            //mLoadTask = Task.Factory.StartNew(LoadFromFile);
            LoadFromFile();
        }

        #region Rank Methods

        public List<RankedFrame> RankFramesBasedOnQuery(List<List<int>> query) {
            //if (mLoadTask.IsFaulted) {
            //    throw mLoadTask.Exception.InnerException;
            //}
            if (query == null) {
                //if (!mLoadTask.IsCompleted || query == null) {
                return RankedFrame.InitializeResultList(mDataset.Frames);
            }

            return GetRankedFrames(query);
        }

        #endregion


        #region (Private) Index File Loading

        private void LoadFromFile() {
            Dictionary<int, int> classLocations = new Dictionary<int, int>();

            if (mUseIDF) {
                string idfFilename = mDataset.GetFileNameByExtension($"-{mSource}.keyword.idf");
                IDF = DCNNKeywords.IDFLoader.LoadFromFile(idfFilename);
            }

            int lastId = mDataset.LAST_FRAME_TO_LOAD;
            string indexFilename = mDataset.GetFileNameByExtension($"-{mSource}.keyword");

            using (DCNNKeywords.BufferedByteStream stream = new DCNNKeywords.BufferedByteStream(indexFilename)) { 

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

                while (!stream.IsEndOfStream()) {

                    // list of class offets does not contain this one
                    if (!classLocations.ContainsKey(stream.Pointer))
                        throw new FileFormatException("Invalid index file format.");

                    int classId = classLocations[stream.Pointer];
                    mClasses.Add(classId, new List<RankedFrameKW>());

                    // add all images
                    while (true) {
                        int imageId = stream.ReadInt32();
                        float imageProbability = stream.ReadFloat();

                        if (imageId != -1) {
                            if (imageId > lastId) continue;

                            Frame f = mDataset.Frames[imageId];
                            RankedFrameKW rf = new RankedFrameKW(f, imageProbability);
                            mClasses[classId].Add(rf);
                        } else break;
                    }
                }
            }
        }

        #endregion

        #region (Private) List Unions & Multiplications

        private List<RankedFrame> GetRankedFrames(List<List<int>> ids) {
            List<RankedFrame> res = RankedFrame.InitializeResultList(mDataset.Frames);

            List<Dictionary<int, RankedFrameKW>> clauses = ResolveClauses(ids);
            Dictionary<int, RankedFrameKW> query = UniteClauses(clauses);

            foreach (KeyValuePair<int, RankedFrameKW> pair in query) {
                res[pair.Key] = new RankedFrame(pair.Value.Frame, pair.Value.Rank);
            }
            
            return res;
        }


        private Dictionary<int, RankedFrameKW> UniteClauses(List<Dictionary<int, RankedFrameKW>> clauses) {
            var result = clauses[clauses.Count - 1];
            clauses.RemoveAt(clauses.Count - 1);

            foreach (Dictionary<int, RankedFrameKW> clause in clauses) {
                Dictionary<int, RankedFrameKW> tempResult = new Dictionary<int, RankedFrameKW>();

                foreach (KeyValuePair<int, RankedFrameKW> rf in clause) {
                    RankedFrameKW rfFromResult;
                    if (result.TryGetValue(rf.Value.Frame.ID, out rfFromResult)) {
                        tempResult.Add(rf.Value.Frame.ID, new RankedFrameKW(rf.Value.Frame, rf.Value.Rank * rfFromResult.Rank));
                    }
                }
                result = tempResult;
            }
            return result;
        }

        private List<Dictionary<int, RankedFrameKW>> ResolveClauses(List<List<int>> ids) {
            var list = new List<Dictionary<int, RankedFrameKW>>();

            // should be fast
            // http://alicebobandmallory.com/articles/2012/10/18/merge-collections-without-duplicates-in-c
            foreach (List<int> listOfIds in ids) {
                int i = 0;
                // there can be classes with no frames, skip them
                while (i < listOfIds.Count && !mClasses.ContainsKey(listOfIds[i])) { i++; }

                // no class with a frame found
                if (i == listOfIds.Count) {
                    list.Add(new Dictionary<int, RankedFrameKW>());
                    continue;
                }

                Dictionary<int, RankedFrameKW> dict = new Dictionary<int, RankedFrameKW>(); //= mClasses[listOfIds[i]].ToDictionary(f => f.Frame.ID);
                
                for (; i < listOfIds.Count; i++) {
                    if (!mClasses.ContainsKey(listOfIds[i])) continue;

                    if (mUseIDF) {
                        float idf = IDF[listOfIds[i]];
                        foreach (RankedFrameKW f in mClasses[listOfIds[i]]) {
                            if (dict.ContainsKey(f.Frame.ID)) {
                                dict[f.Frame.ID].Rank += f.Rank * idf;
                            } else {
                                var frame = f.Clone();
                                frame.Rank *= idf;
                                dict.Add(f.Frame.ID, frame);
                            }
                        }
                    } else {
                        foreach (RankedFrameKW f in mClasses[listOfIds[i]]) {
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
