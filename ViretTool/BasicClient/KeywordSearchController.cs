//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.BasicClient.Controls;

//namespace ViretTool.BasicClient {

//    class KeywordSearchController {

//        private SuggestionTextBox mSuggestionTextBox;
//        private Dictionary<string, LabelProvider> mLabelProviders = new Dictionary<string, LabelProvider>();
//        private Dictionary<string, SuggestionProvider> mSuggestionProviders = new Dictionary<string, SuggestionProvider>();
//        private Dictionary<string, KeywordModel> mKeywordModels = new Dictionary<string, KeywordModel>();

//        public KeywordSearchController(DataModel.Dataset dataset, SuggestionTextBox suggestionTextBox, string[] annotationSources) {
//            mSuggestionTextBox = suggestionTextBox;
//            mSuggestionTextBox.AnnotationSources = annotationSources;

//            foreach (string source in annotationSources) {

//                var labelProvider = new LabelProvider(source + ".labels");
//                mLabelProviders.Add(source, labelProvider);

//                var suggestionProvider = new SuggestionProvider(labelProvider);
//                mSuggestionProviders.Add(source, suggestionProvider);
//                suggestionProvider.SuggestionResultsReadyEvent += mSuggestionTextBox.OnSuggestionResultsReady;
//                suggestionProvider.ShowSuggestionMessageEvent += mSuggestionTextBox.OnShowSuggestionMessage;

//                var keywordModel = new KeywordModel(dataset, labelProvider, source + ".index");
//                mKeywordModels.Add(source, keywordModel);
//            }

//            mSuggestionTextBox.QueryChangedEvent += MSuggestionTextBox_QueryChangedEvent;
//            mSuggestionTextBox.SuggestionFilterChangedEvent += MSuggestionTextBox_SuggestionFilterChangedEvent;
//            mSuggestionTextBox.SuggestionsNotNeededEvent += MSuggestionTextBox_SuggestionsNotNeededEvent;
//            mSuggestionTextBox.GetSuggestionSubtreeEvent += MSuggestionTextBox_GetSuggestionSubtreeEvent;
//        }

//        public IEnumerable<RankingModel.IRankingModel> GetModels() {
//            foreach (var item in mKeywordModels) {
//                yield return item.Value;
//            }
//        }

//        private void MSuggestionTextBox_QueryChangedEvent(IEnumerable<IQueryPart> query, string annotationSource) {
//            mKeywordModels[annotationSource].RankFramesBasedOnQuery(query);
//        }

//        private void MSuggestionTextBox_SuggestionFilterChangedEvent(string filter, string annotationSource) {
//            mSuggestionProviders[annotationSource].GetSuggestionsAsync(filter);
//        }

//        private void MSuggestionTextBox_SuggestionsNotNeededEvent() {
//            foreach (KeyValuePair<string, SuggestionProvider> provider in mSuggestionProviders) {
//                provider.Value.CancelSuggestions();
//            }
//        }

//        private IEnumerable<IIdentifiable> MSuggestionTextBox_GetSuggestionSubtreeEvent(IEnumerable<int> subtree, string filter, string annotationSource) {
//            return mSuggestionProviders[annotationSource].GetSuggestions(subtree, filter);
//        }

//    }
//}
