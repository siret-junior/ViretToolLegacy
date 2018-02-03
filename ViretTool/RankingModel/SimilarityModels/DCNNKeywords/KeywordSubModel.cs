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

        private int LastId;
        private BinaryReader mReader;
        private Dictionary<int, int> mClassLocations;
        //private Task mLoadTask;

        private float[] IDF;

        private Random mRandom = new Random();
        private Dictionary<int, List<KeywordSearchFrame>> mClassCache;
        private const int CACHE_DELETE = 10;
        private const int MAX_CACHE_SIZE = 100;
        private const int LIST_DEFAULT_SIZE = 32768;

        private const int MAX_CLAUSE_CACHE_SIZE = 10;
        private Dictionary<List<int>, Dictionary<int, float>> mClauseCache;

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

        public Tuple<int, List<RankedFrame>> RankFramesBasedOnQuery(List<List<int>> query) {
            //if (mLoadTask.IsFaulted) {
            //    throw mLoadTask.Exception.InnerException;
            //}
            if (query == null) {
                //if (!mLoadTask.IsCompleted || query == null) {
                return new Tuple<int, List<RankedFrame>>(mDataset.Frames.Count, null);
                //return null;
            }

            return GetRankedFrames(query);
        }

        #endregion


        #region (Private) Index File Loading

        private void LoadFromFile() {
            mClassLocations = new Dictionary<int, int>();
            mClassCache = new Dictionary<int, List<KeywordSearchFrame>>();
            mClauseCache = new Dictionary<List<int>, Dictionary<int, float>>(new DCNNKeywords.ListComparer());

            if (mUseIDF) {
                string idfFilename = mDataset.GetFileNameByExtension($"-{mSource}.keyword.idf");
                IDF = DCNNKeywords.IDFLoader.LoadFromFile(idfFilename);
            }

            LastId = mDataset.LAST_FRAME_TO_LOAD;
            string indexFilename = mDataset.GetFileNameByExtension($"-{mSource}.keyword");

            mReader = new BinaryReader(File.Open(indexFilename, FileMode.Open, FileAccess.Read, FileShare.Read));

            // header = 'KS INDEX'+(Int64)-1
            if (mReader.ReadInt64() != 0x4b5320494e444558 && mReader.ReadInt64() != -1)
                throw new FileFormatException("Invalid index file format.");

            // read offests of each class
            while (true) {
                int value = mReader.ReadInt32();
                int valueOffset = mReader.ReadInt32();

                if (value != -1) {
                    mClassLocations.Add(value, valueOffset);
                } else break;
            }
        }

        private List<KeywordSearchFrame> ReadClassFromFile(int classId) {
            if (mClassCache.ContainsKey(classId))
                return mClassCache[classId];

            if (!mClassLocations.ContainsKey(classId)) {
                //throw new FileFormatException("Class ID is incorrect.");
                return new List<KeywordSearchFrame>();
            }

            var list = new List<KeywordSearchFrame>(LIST_DEFAULT_SIZE);

            mReader.BaseStream.Seek(mClassLocations[classId], SeekOrigin.Begin);

            // add all images
            while (true) {
                int imageId = mReader.ReadInt32();
                float imageProbability = mReader.ReadSingle();

                if (imageId != -1) {
                    if (imageId > LastId) continue;

                    list.Add(new KeywordSearchFrame(imageId, imageProbability));
                } else break;
            }

            if (mClassCache.Count == MAX_CACHE_SIZE) {
                for (int i = 0; i < CACHE_DELETE; i++) {
                    var randClass = mClassCache.Keys.ToList()[mRandom.Next(mClassCache.Count)];
                    mClassCache.Remove(randClass);
                }
            }
            mClassCache.Add(classId, list);

            return list;
        }

        #endregion

        #region (Private) List Unions & Multiplications

        private Tuple<int, List<RankedFrame>> GetRankedFrames(List<List<int>> ids) {
            List<RankedFrame> res = RankedFrame.InitializeResultList(mDataset.Frames);

            List<Dictionary<int, float>> clauses = ResolveClauses(ids);
            Dictionary<int, float> query = UniteClauses(clauses);

            foreach (KeyValuePair<int, float> pair in query) {
                res[pair.Key] = new RankedFrame(mDataset.Frames[pair.Key], pair.Value);
            }
            
            return new Tuple<int, List<RankedFrame>>(query.Count, res);
        }


        private Dictionary<int, float> UniteClauses(List<Dictionary<int, float>> clauses) {
            var result = clauses[clauses.Count - 1];
            clauses.RemoveAt(clauses.Count - 1);

            foreach (Dictionary<int, float> clause in clauses) {
                Dictionary<int, float> tempResult = new Dictionary<int, float>();

                foreach (KeyValuePair<int, float> rf in clause) {
                    float rfFromResult;
                    if (result.TryGetValue(rf.Key, out rfFromResult)) {
                        tempResult.Add(rf.Key, rf.Value * rfFromResult);
                    }
                }
                result = tempResult;
            }
            return result;
        }

        private List<Dictionary<int, float>> ResolveClauses(List<List<int>> ids) {
            var list = new List<Dictionary<int, float>>();

            // should be fast
            // http://alicebobandmallory.com/articles/2012/10/18/merge-collections-without-duplicates-in-c
            foreach (List<int> listOfIds in ids) {
                if (mClauseCache.ContainsKey(listOfIds)) {
                    list.Add(mClauseCache[listOfIds]);
                    continue;
                }

                Dictionary<int, float> dict = new Dictionary<int, float>(); //= mClasses[listOfIds[i]].ToDictionary(f => f.Frame.ID);
                
                for (int i = 0; i < listOfIds.Count; i++) {
                    var classFrames = ReadClassFromFile(listOfIds[i]);
                    if (classFrames.Count == 0) continue;

                    if (mUseIDF) {
                        float idf = IDF[listOfIds[i]];
                        foreach (KeywordSearchFrame f in classFrames) {
                            if (dict.ContainsKey(f.Id)) {
                                dict[f.Id] += f.Rank * idf;
                            } else {
                                dict.Add(f.Id, f.Rank * idf);
                            }
                        }
                    } else {
                        foreach (KeywordSearchFrame f in classFrames) {
                            if (dict.ContainsKey(f.Id)) {
                                dict[f.Id] += f.Rank;
                            } else {
                                dict.Add(f.Id, f.Rank);
                            }
                        }
                    }
                }

                if (MAX_CLAUSE_CACHE_SIZE == mClauseCache.Count) {
                    var randClass = mClauseCache.Keys.ToList()[mRandom.Next(mClauseCache.Count)];
                    mClauseCache.Remove(randClass);
                }
                mClauseCache.Add(listOfIds, dict);

                list.Add(dict);
            }
            return list;
        }

        #endregion

    }

}
