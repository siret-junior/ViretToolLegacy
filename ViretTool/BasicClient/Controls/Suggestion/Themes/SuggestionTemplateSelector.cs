using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ViretTool.RankingModel.DCNNKeywords;

namespace ViretTool.BasicClient.Controls {
    class SuggestionTemplateSelector : DataTemplateSelector {
        public DataTemplate OnlyChildren { get; set; }
        public DataTemplate WithChildren { get; set; }
        public DataTemplate Base { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (((SuggestionResultItem)item).HasOnlyChildren)
                return OnlyChildren;
            else if (((SuggestionResultItem)item).Hyponyms != null)
                return WithChildren;
            return Base;
        }
    }
}
