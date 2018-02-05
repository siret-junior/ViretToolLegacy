using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViretTool.BasicClient.Controls;

namespace ViretTool.BasicClient {
    /// <summary>
    /// Interaction logic for KeywordSearchControl.xaml
    /// </summary>
    public partial class KeywordSearchControl : UserControl {

        public delegate void KeywordChangedHandler(List<List<int>> query, string annotationSource);

        /// <summary>
        /// KeywordChangedEvent is raised whenever users changes the input textbox.
        /// </summary>
        public event KeywordChangedHandler KeywordChangedEvent;


        private Dictionary<string, LabelProvider> mLabelProviders = new Dictionary<string, LabelProvider>();
        private Dictionary<string, SuggestionProvider> mSuggestionProviders = new Dictionary<string, SuggestionProvider>();

        public KeywordSearchControl() {
            InitializeComponent();
        }

        public void Clear()
        {
            suggestionTextBox.ClearQuery();
            VBSLogger.AppendActionIncludeTimeParameter('K', false);
        }

        private void textClearButton_Click(object sender, RoutedEventArgs e) {
            suggestionTextBox.ClearQuery();
            VBSLogger.AppendActionIncludeTimeParameter('K', false);
        }

        public void Init(DataModel.Dataset dataset, string[] annotationSources) {
            suggestionTextBox.AnnotationSources = annotationSources;

            foreach (string source in annotationSources) {
                string labels = dataset.GetFileNameByExtension($"-{source}.label");

                var labelProvider = new LabelProvider(labels);
                mLabelProviders.Add(source, labelProvider);

                var suggestionProvider = new SuggestionProvider(labelProvider);
                mSuggestionProviders.Add(source, suggestionProvider);
                suggestionProvider.SuggestionResultsReadyEvent += suggestionTextBox.OnSuggestionResultsReady;
                suggestionProvider.ShowSuggestionMessageEvent += suggestionTextBox.OnShowSuggestionMessage;
            }

            suggestionTextBox.QueryChangedEvent += SuggestionTextBox_QueryChangedEvent;
            suggestionTextBox.SuggestionFilterChangedEvent += SuggestionTextBox_SuggestionFilterChangedEvent;
            suggestionTextBox.SuggestionsNotNeededEvent += SuggestionTextBox_SuggestionsNotNeededEvent;
            suggestionTextBox.GetSuggestionSubtreeEvent += SuggestionTextBox_GetSuggestionSubtreeEvent;
        }

        private void SuggestionTextBox_QueryChangedEvent(IEnumerable<IQueryPart> query, string annotationSource) {
            if (annotationSource == null) return;

            var sb = new StringBuilder();
            sb.Append("QueryChangedEvent {source:");
            sb.Append(annotationSource);
            sb.Append(",wordnet_query:");
            foreach (IQueryPart q in query) {
                switch (q.Type) {
                    case TextBlockType.Class:
                        sb.Append(q.Id);
                        sb.Append(q.UseChildren ? ",1" : ",0");
                        break;
                    case TextBlockType.OR:
                        sb.Append("or");
                        break;
                    case TextBlockType.AND:
                        sb.Append("and");
                        break;
                    default:
                        break;
                }
            }
            sb.Append("}");

            Logger.Log(this, Severity.Debug, sb.ToString());

            List<List<int>> expanded = ExpandQuery(query, mLabelProviders[annotationSource]);
            KeywordChangedEvent?.Invoke(expanded, annotationSource);
        }
        
        private IEnumerable<IIdentifiable> SuggestionTextBox_GetSuggestionSubtreeEvent(IEnumerable<int> subtree, string filter, string annotationSource) {
            return mSuggestionProviders[annotationSource].GetSuggestions(subtree, filter);
        }

        private void SuggestionTextBox_SuggestionsNotNeededEvent() {
            foreach (KeyValuePair<string, SuggestionProvider> provider in mSuggestionProviders) {
                provider.Value.CancelSuggestions();
            }
        }

        private void SuggestionTextBox_SuggestionFilterChangedEvent(string filter, string annotationSource) {
            mSuggestionProviders[annotationSource].GetSuggestionsAsync(filter);
        }


        #region Parse query to list of ints

        private List<List<int>> ExpandQuery(IEnumerable<IQueryPart> query, LabelProvider lp) {
            var list = new List<List<int>>();
            list.Add(new List<int>());

            foreach (var item in query) {
                if (item.Type == TextBlockType.Class) {
                    if (item.UseChildren) {
                        IEnumerable<int> synsetIds = ExpandLabel(new int[] { item.Id }, lp);

                        foreach (int synId in synsetIds) {
                            int id = lp.Labels[synId].Id;

                            list[list.Count - 1].Add(id);
                        }
                    } else {
                        int id = lp.Labels[item.Id].Id;

                        list[list.Count - 1].Add(id);
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

        private List<int> ExpandLabel(IEnumerable<int> ids, LabelProvider lp) {
            var list = new List<int>();
            foreach (var item in ids) {
                var label =lp.Labels[item];

                if (label.Id != -1) list.Add(label.SynsetId);
                if (label.Hyponyms != null) list.AddRange(ExpandLabel(label.Hyponyms, lp));
            }
            return list;
        }

        #endregion

    }
}
