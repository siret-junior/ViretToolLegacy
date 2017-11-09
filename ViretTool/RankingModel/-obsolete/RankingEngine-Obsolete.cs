using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModel {
    class RankingEngineObsolete {

        private BasicClient.ImageListController mImageController;
        private readonly DataModel.Dataset mDataset;
        private BasicClient.Controls.ModelSelector mModelSelector;
        private List<IRankingModel> mRankingModels = new List<IRankingModel>();
        
        public RankingEngineObsolete(DataModel.Dataset dataset) {
            mDataset = dataset;
        }

        // only add here your init method
        // it should add your model(s) into mRankingModels list
        // >> Why here? Each model should be initialized separately and pass just reference to an initialized instance.

        public void InitKeywordModel(BasicClient.Controls.SuggestionTextBox box, string[] datasets) {
            var controller = new BasicClient.KeywordSearchController(mDataset, box, datasets);

            var models = controller.GetModels();
            mRankingModels.AddRange(models);
        }

        public void BuildEngine(BasicClient.ImageListController controller, BasicClient.Controls.ModelSelector selector) {
            mImageController = controller;

            mModelSelector = selector;
            mModelSelector.Models = mRankingModels;
            mModelSelector.ModelSelectionChangedEvent += ModelSelector_ModelSelectionChanged; ;

            foreach (var model in mRankingModels) {
                model.RankingChangedEvent += Model_RankingChangedEvent;
                model.RankingInvalidatedEvent += Model_RankingInvalidatedEvent;
                model.MessageReporterEvent += Model_MessageReporterEvent;
            }
        }

        public void UpdateMaxNormalizedAndSortedFinalRanking() {
            List<RankingModel.RankedFrame> result = RankingModel.RankedFrame.InitializeResultList(mDataset);

            foreach (KeyValuePair<IRankingModel, bool> pair in mModelSelector.ModelSelection) {
                if (!pair.Value) continue;

                var modelRanking = pair.Key.LastResult;

                double max = modelRanking.Select(x => x.Rank).Max();
                Parallel.For(0, result.Count, i =>
                    result[i] = new RankingModel.RankedFrame(modelRanking[i].Frame, modelRanking[i].Rank + result[i].Rank));// / max);
            }

            // TODO - use some parallel sorting
            result.Sort();

            // TODO - filter only top K or the most distinct frames from each video

            mImageController.ShowResults(result);
            // TODO - update result in UI
        }

        /// <summary>
        /// Fired when a model changes ranking.
        /// </summary>
        /// <param name="model">A model which fired the event.</param>
        private void Model_RankingChangedEvent(IRankingModel model) {
            UpdateMaxNormalizedAndSortedFinalRanking();
        }

        /// <summary>
        /// Fired when a model starts recalculating results.
        /// </summary>
        /// <param name="model">A model which fired the event.</param>
        private void Model_RankingInvalidatedEvent(IRankingModel model) {
            // use this model automaticaly
            mModelSelector.Select(model);
        }

        /// <summary>
        /// Fired when model selection changes by user input.
        /// </summary>
        private void ModelSelector_ModelSelectionChanged() {
            UpdateMaxNormalizedAndSortedFinalRanking();
        }

        /// <summary>
        /// Show error.
        /// </summary>
        private void Model_MessageReporterEvent(IRankingModel model, MessageType type, string message) {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                switch (type) {
                    case MessageType.Exception:
                        System.Windows.MessageBox.Show(message, model.Name, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                        return;
                    case MessageType.Information:
                        System.Windows.MessageBox.Show(message, model.Name, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        break;
                }
            });
        }

    }
}
