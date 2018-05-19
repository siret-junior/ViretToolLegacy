using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ViretTool.DataModel;

namespace ViretTool.BasicClient.Displays
{
    public class GlobalItemSelector {
        public static List<IMainDisplay> Displays = new List<IMainDisplay>();

        private static DataModel.Frame mSelectedFrame = null;
        public static DataModel.Frame SelectedFrame {
            get {
                return mSelectedFrame;
            }

            set {
                mSelectedFrame = value;
                ActiveDisplay.SelectedFrameChanged(SelectedFrame);
                VideoDisplay.SelectedFrameChanged(SelectedFrame);
            }
        }

        public static VideoDisplay VideoDisplay = null;

        public static IMainDisplay ActiveDisplay { get; set; }

        public static void Activate(IMainDisplay display) {
            foreach (var item in Displays) {
                item.DisplayHidden();
            }
            ActiveDisplay = display;
            display.DisplaySelected();
        }
        
    }
}
