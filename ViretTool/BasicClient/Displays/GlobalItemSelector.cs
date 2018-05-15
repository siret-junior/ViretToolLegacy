using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;

namespace ViretTool.BasicClient.Displays
{
    public class GlobalItemSelector
    {
        private static Frame mSelectedFrame = null;
        public static Frame SelectedFrame
        {
            get
            {
                return mSelectedFrame;
            }

            set
            {
                mSelectedFrame = value;
                SelectedFrameChangedEvent?.Invoke(SelectedFrame);
            }
        }

        public delegate void SelectedFrameChangedEventHandler(Frame frame);
        public static event SelectedFrameChangedEventHandler SelectedFrameChangedEvent;
        
    }
}
