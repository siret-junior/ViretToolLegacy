using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViretTool.BasicClient.Utils;
using ViretTool.DataModel;
using ViretTool.RankingModels;

namespace ViretTool.SimilarityModels.DCNNKeywords {
    
    /// <summary>
    /// Searches an index file and displays results
    /// </summary>
    class KeywordModel : IRankingModel {

        private string mIndexFilePath;

        private Dataset mDataset;
        private LabelProvider mLabelProvider;
        private Dictionary<int, List<RankedFrame>> mClasses;
        private Task mLoadTask;

        private List<RankedFrame> mLastResult;
        public List<RankedFrame> LastResult {
            get {
                return mLastResult;
            }
            private set {
                mLastResult = value;
                RankingChangedEvent?.Invoke(this);
            }
        }

        public string Name => "Keyword Model " + Path.GetFileNameWithoutExtension(mIndexFilePath);

        public event MessageReporterHandler MessageReporterEvent;
        public event RankingChangedHandler RankingChangedEvent;
        public event RankingInvalidatedHandler RankingInvalidatedEvent;

        /// <param name="lp">For class name to class id conversion</param>
        /// <param name="filePath">Relative or absolute path to index file</param>
        public KeywordModel(Dataset dataset, LabelProvider lp, string filePath) {
            mLabelProvider = lp;
            mIndexFilePath = filePath;
            mDataset = dataset;
            mClasses = new Dictionary<int, List<RankedFrame>>();

            mLastResult = RankedFrame.InitializeResultList(mDataset);

            mLoadTask = Task.Factory.StartNew(LoadFromFile);
        }

        #region (Private) Index File Loading

        private void LoadFromFile() {
            BufferedByteStream stream = null;
            Dictionary<int, int> classLocations = new Dictionary<int, int>();

            try {
                stream = new BufferedByteStream(mIndexFilePath);

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

                while (true) {
                    if (stream.IsEndOfStream()) break;

                    // list of class offets does not contain this one
                    if (!classLocations.ContainsKey(stream.Pointer))
                        throw new FileFormatException("Invalid index file format.");

                    int classId = classLocations[stream.Pointer];
                    mClasses.Add(classId, new List<RankedFrame>());

                    // add all images
                    while (true) {
                        uint imageId = (uint)stream.ReadInt32();
                        float imageProbability = stream.ReadFloat();

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

        #region Rank Methods

        public void RankFramesBasedOnQuery(IEnumerable<IQueryPart> query) {
            RankingInvalidatedEvent?.Invoke(this);

            if (mLabelProvider.LoadTask.IsFaulted || mLoadTask.IsFaulted) {
                MessageReporterEvent?.Invoke(this, MessageType.Exception,
                    mLabelProvider.LoadTask.IsFaulted ? mLabelProvider.LoadTask.Exception.InnerException.Message : mLoadTask.Exception.InnerException.Message);
                return;
            } else if (!mLabelProvider.LoadTask.IsCompleted || !mLoadTask.IsCompleted) {
                MessageReporterEvent?.Invoke(this, MessageType.Information, "Index file or label file is not yet loaded.");
                return;
            }

            // parse the search phrase
            List<List<int>> ids = ExpandQuery(query);
            if (ids == null) {
                LastResult = RankedFrame.InitializeResultList(mDataset);
                return;
            }

            LastResult = GetRankedFrames(ids);
        }

        #endregion

        #region (Private) List Unions & Multiplications

        private List<RankedFrame> GetRankedFrames(List<List<int>> ids) {
            List<RankedFrame> res = RankedFrame.InitializeResultList(mDataset);

            List<Dictionary<int, RankedFrame>> clauses = UniteClauses(ids);
            foreach (KeyValuePair<int, RankedFrame> pair in clauses[0]) {
                res[pair.Key].Rank = pair.Value.Rank;
            }
            for (int i = 1; i < clauses.Count; i++) {
                foreach (KeyValuePair<int, RankedFrame> pair in clauses[i]) {
                    res[pair.Key].Rank *= pair.Value.Rank;
                }
            }

            return res;
        }

        private List<Dictionary<int, RankedFrame>> UniteClauses(List<List<int>> ids) {
            var list = new List<Dictionary<int, RankedFrame>>();

            // should be fast
            // http://alicebobandmallory.com/articles/2012/10/18/merge-collections-without-duplicates-in-c
            RankedFrame fIn;
            foreach (List<int> listOfIds in ids) {
                Dictionary<int, RankedFrame> dict = mClasses[listOfIds[0]].ToDictionary(f => f.Frame.ID);

                for (int i = 1; i < listOfIds.Count; i++) {
                    foreach (RankedFrame f in mClasses[listOfIds[i]]) {
                        if (dict.TryGetValue(f.Frame.ID, out fIn)) {
                            fIn.Rank += f.Rank;
                            dict[f.Frame.ID] = fIn;
                        } else {
                            dict.Add(f.Frame.ID, f);
                        }
                    }
                }
                list.Add(dict);
            }
            return list;
        }

        #endregion

        #region (Private) Parse Search Term

        private List<List<int>> ExpandQuery(IEnumerable<IQueryPart> query) {
            var list = new List<List<int>>();
            list.Add(new List<int>());

            foreach (var item in query) {
                if (item.Type == TextBlockType.Class) {
                    if (item.UseChildren) {
                        IEnumerable<int> synsetIds = ExpandLabel(new int[] { item.Id });

                        foreach (int synId in synsetIds) {
                            int id = mLabelProvider.Labels[synId].Id;

                            if (mClasses.ContainsKey(id)) {
                                list[list.Count - 1].Add(id);
                            }
                        }
                    } else {
                        int id = mLabelProvider.Labels[item.Id].Id;

                        if (mClasses.ContainsKey(id)) {
                            list[list.Count - 1].Add(id);
                        }
                    }
                } else if (item.Type == TextBlockType.AND) {
                    list.Add(new List<int>());
                }
            }

            for (int i = 0; i < list.Count; i++) {
                if (list[i].Count == 0) {
                    return null;
                }
                list[i] = list[i].Distinct().ToList();
            }
            return list;
        }

        private List<int> ExpandLabel(IEnumerable<int> ids) {
            var list = new List<int>();
            foreach (var item in ids) {
                Label label = mLabelProvider.Labels[item];

                if (label.Id != -1) list.Add(label.SynsetId);
                if (label.Hyponyms != null) list.AddRange(ExpandLabel(label.Hyponyms));
            }
            return list;
        }

        #endregion

    }


}
