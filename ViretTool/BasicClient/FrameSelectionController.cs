using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataModel;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    // TODO: remove and use SemanticModelDisplay?
    public class FrameSelectionController
    {
        private List<Frame> mSelectedFrames;
        
        // used to redraw displays
        public delegate void SelectionEventHandler(List<Frame> selectedFrames);
        public event SelectionEventHandler SelectionChangedEvent;
        public event SelectionEventHandler SelectionSubmittedEvent;
        
        public FrameSelectionController()
        {
            mSelectedFrames = new List<Frame>();
        }

        public void AddToSelection(Frame selectedFrame)
        {
            if (selectedFrame == null)
            {
                throw new ArgumentNullException("Selected frame is null!");
            }

            if (!mSelectedFrames.Contains(selectedFrame))
            {
                mSelectedFrames.Add(selectedFrame);
            }
            else { /* TODO: log warning */}

            // update displays
            SelectionChangedEvent?.Invoke(mSelectedFrames);
        }

        public void RemoveFromSelection(Frame deselectedFrame)
        {
            if (deselectedFrame == null)
            {
                throw new ArgumentNullException("Deselected frame is null!");
            }

            if (mSelectedFrames.Contains(deselectedFrame))
            {
                mSelectedFrames.Remove(deselectedFrame);
            }
            else { /* TODO: log warning */}
            
            // update displays
            SelectionChangedEvent?.Invoke(mSelectedFrames);
        }

        public void ResetSelection()
        {
            mSelectedFrames.Clear();
            SelectionChangedEvent?.Invoke(mSelectedFrames);
        }

        public void SubmitSelection()
        {
            SelectionSubmittedEvent?.Invoke(mSelectedFrames);
        }
    }
}
