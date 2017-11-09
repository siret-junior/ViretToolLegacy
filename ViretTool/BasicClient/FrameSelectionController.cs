using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    public class FrameSelectionController
    {
        private List<DataModel.Frame> mSelectedFrames;
        public List<DataModel.Frame> SelectedFrames
        {
            get
            { return mSelectedFrames; }
        }
        
        // used to redraw displays
        public delegate void SelectionChangedEventHandler();
        public event SelectionChangedEventHandler SelectionChangedEvent;

        private RankingModel.RankingEngine mRankingEngine;
        private ResultDisplay mResultDisplay;
        private VideoDisplay mVideoDisplay;
        private SemanticModelDisplay mSemanticModelDisplay;

        public FrameSelectionController(
            RankingEngine rankingEngine, 
            ResultDisplay resultDisplay,
            VideoDisplay videoDisplay,
            SemanticModelDisplay semanticModelDisplay)
        {
            mSelectedFrames = new List<DataModel.Frame>();
            
            mRankingEngine = rankingEngine;
            mResultDisplay = resultDisplay;
            mVideoDisplay = videoDisplay;
            mSemanticModelDisplay = semanticModelDisplay;
        }

        public void AddToSelection(DataModel.Frame selectedFrame)
        {
            if (!mSelectedFrames.Contains(selectedFrame))
            {
                mSelectedFrames.Add(selectedFrame);
            }
            else { /* TODO: log warning */}

            // update displays
            SelectionChangedEvent?.Invoke();
        }

        public void RemoveFromSelection(DataModel.Frame deselectedFrame)
        {
            if (mSelectedFrames.Contains(deselectedFrame))
            {
                mSelectedFrames.Remove(deselectedFrame);
            }
            else { /* TODO: log warning */}
            
            // update displays
            SelectionChangedEvent?.Invoke();
        }

        public void ResetSelection()
        {
            mSelectedFrames.Clear();
            SelectionChangedEvent?.Invoke();
        }

        public void SubmitSelection()
        {
            mResultDisplay.ResultFrames = mRankingEngine.UpdateVectorModelRanking(mSelectedFrames);
            mSemanticModelDisplay.DisplayFrames(mSelectedFrames);

            ResetSelection();
        }
    }
}
