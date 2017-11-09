using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient {
    class ImageListController {
        private ItemsControl mItemsControl;

        public ImageListController(ItemsControl ic) {
            mItemsControl = ic;
        }

        public void ShowResults(List<RankedFrame> results) {
            mItemsControl.ItemsSource = UpdateItemsSource(results, 0, 56);
        }

        public IEnumerable<RankedFrame> UpdateItemsSource(List<RankedFrame> results, int offset, int length) {
            for (int i = offset; i < Math.Min(results.Count, offset + length); i++) {
                yield return results[i];
            }
        }

    }
}
