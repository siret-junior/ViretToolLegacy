using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.BasicClient.Displays {
    public interface IMainDisplay {

        void DisplaySelected();
        void DisplayHidden();
        void SelectedFrameChanged(Frame selectedFrame);
        void IncrementDisplay(int pages);
        void GoToPage(int page);
    }
}
