using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ViretTool.BasicClient
{
    public class DisplayControl : UserControl, IDisplayControl
    {
        protected int mDisplayCols;
        protected int mDisplayRows;

        private DisplayFrame[] mDisplayedFrames;
        public DisplayFrame[] DisplayedFrames
        {
            get
            { return mDisplayedFrames; }
            set
            {
                mDisplayedFrames = value;
                UpdateSelectionVisualization();
            }
        }
        
        private List<DataModel.Frame> mSelectedFrames;
        public List<DataModel.Frame> SelectedFrames
        {
            get
            { return mSelectedFrames; }
            set
            {
                mSelectedFrames = value;
                UpdateSelectionVisualization();
            }
        }

        protected void UpdateSelectionVisualization()
        {
            if (SelectedFrames != null && DisplayedFrames != null)
            {
                for (int i = 0; i < DisplayedFrames.Length; i++)
                {
                    DisplayedFrames[i].IsSelected = SelectedFrames.Contains(DisplayedFrames[i].Frame);
                }
            }
        }

        protected void ResizeDisplay(int nRows, int nCols, UniformGrid displayGrid)
        {
            if (mDisplayRows == nRows && mDisplayCols == nCols)
            {
                return;
            }

            // setup display grid
            displayGrid.Columns = mDisplayCols = nCols;
            displayGrid.Rows = mDisplayRows = nRows;
            int displaySize = nRows * nCols;

            // create and fill new displayed frames
            DisplayFrame[] newDisplayFrames = new DisplayFrame[displaySize];
            displayGrid.Children.Clear();
            for (int i = 0; i < displaySize; i++)
            {
                DisplayFrame displayedFrame = new DisplayFrame(this);
                newDisplayFrames[i] = displayedFrame;
                displayGrid.Children.Add(displayedFrame);
            }
            DisplayedFrames = newDisplayFrames;
        }

        #region --[ Events ]--
        
        public event FrameSelectionEventHandler AddingToSelectionEvent;
        public void RaiseAddingToSelectionEvent(DataModel.Frame selectedFrame)
        {
            AddingToSelectionEvent?.Invoke(selectedFrame);
        }

        public event FrameSelectionEventHandler RemovingFromSelectionEvent;
        public void RaiseRemovingFromSelectionEvent(DataModel.Frame selectedFrame)
        {
            RemovingFromSelectionEvent?.Invoke(selectedFrame);
        }

        public event SubmitSelectionEventHandler ResettingSelectionEvent;
        public void RaiseResettingSelectionEvent()
        {
            ResettingSelectionEvent?.Invoke();
        }

        //public event SubmitSelectionEventHandler SelectionColorSearchEvent;
        //public void RaiseSelectionColorSearchEvent()
        //{
        //    SelectionColorSearchEvent?.Invoke();
        //}

        public event SubmitSelectionEventHandler SelectionSemanticSearchEvent;
        public void RaiseSelectionSemanticSearchEvent()
        {
            SelectionSemanticSearchEvent?.Invoke();
        }

        public event FrameSelectionEventHandler DisplayingFrameVideoEvent;
        public void RaiseDisplayingFrameVideoEvent(DataModel.Frame selectedFrame)
        {
            DisplayingFrameVideoEvent?.Invoke(selectedFrame);
        }


        public event FrameSelectionEventHandler SubmittingToServerEvent;
        public void RaiseSubmittingToServerEvent(DataModel.Frame submittedFrame)
        {
            SubmittingToServerEvent?.Invoke(submittedFrame);
        }


        #endregion

    }
}
