using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ViretTool.BasicClient.Controls;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient {
    class ImageListController {
        private ItemsControl mItemsControl;
        private List<RankedFrame> mSource;

        //private Paging mPaging;

        public int ImagesPerPage { get; private set; }

        //public ImageListController(ItemsControl ic, Paging paging) {
        //    mItemsControl = ic;
        //    ImagesPerPage = 56;
        //    mPaging = paging;
        //    mPaging.CurrentPageChangedEvent += MPaging_CurrentPageChangedEvent;
        //}

        private void MPaging_CurrentPageChangedEvent(int page) {
            mItemsControl.ItemsSource = UpdateItemsSource(mSource, (page - 1) * ImagesPerPage, ImagesPerPage);
        }
        
        public void ShowResults(List<RankedFrame> results) {
            mSource = results;
            //mPaging.SetCurrentPage(1, (int)Math.Ceiling(results.Count / (float)ImagesPerPage));
            mItemsControl.ItemsSource = UpdateItemsSource(results, 0, ImagesPerPage);
        }

        public IEnumerable<RankedFrame> UpdateItemsSource(List<RankedFrame> results, int offset, int length) {
            for (int i = offset; i < Math.Min(results.Count, offset + length); i++) {
                yield return results[i];
            }
        }

    }
}
