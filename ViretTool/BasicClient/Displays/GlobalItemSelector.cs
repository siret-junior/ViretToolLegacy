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
        public static List<Tuple<IMainDisplay, Button>> Displays = new List<Tuple<IMainDisplay, Button>>();

        public static event SelectedFrameChangedEventHandler SelectedFrameChangedEvent;
        public delegate void SelectedFrameChangedEventHandler(DataModel.Frame selectedFrame);

        private static DataModel.Frame mSelectedFrame = null;
        public static DataModel.Frame SelectedFrame {
            get {
                return mSelectedFrame;
            }

            set {
                mSelectedFrame = value;
                ActiveDisplay.SelectedFrameChanged(SelectedFrame);
                SelectedFrameChangedEvent?.Invoke(SelectedFrame);
            }
        }

        public static IMainDisplay ActiveDisplay { get; set; }

        public static void Activate(IMainDisplay display) {
            foreach (var t in Displays) {
                t.Item1.DisplayHidden();
                t.Item2.Background = System.Windows.Media.Brushes.DodgerBlue;
            }

            foreach (var t in Displays) {
                if (t.Item1 == display) {
                    ActiveDisplay = display;
                    display.DisplaySelected();
                    t.Item2.Background = System.Windows.Media.Brushes.Gray;
                    break;
                }
            }
        }
        
    }
}
