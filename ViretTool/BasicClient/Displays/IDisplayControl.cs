using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BasicClient {
    public delegate void FrameSelectionEventHandler(DataModel.Frame selectedFrame);
    public delegate void SubmitSelectionEventHandler();

    public interface IDisplayControl {

        event FrameSelectionEventHandler AddingToSelectionEvent;
        void RaiseAddingToSelectionEvent(DataModel.Frame selectedFrame);

        event FrameSelectionEventHandler RemovingFromSelectionEvent;
        void RaiseRemovingFromSelectionEvent(DataModel.Frame selectedFrame);

        event SubmitSelectionEventHandler ResettingSelectionEvent;
        void RaiseResettingSelectionEvent();

        //public event SubmitSelectionEventHandler SelectionColorSearchEvent;
        //public void RaiseSelectionColorSearchEvent()
        //{
        //    SelectionColorSearchEvent?.Invoke();
        //}

        event SubmitSelectionEventHandler SelectionSemanticSearchEvent;
        void RaiseSelectionSemanticSearchEvent();

        event FrameSelectionEventHandler DisplayingFrameVideoEvent;
        void RaiseDisplayingFrameVideoEvent(DataModel.Frame selectedFrame);


        event FrameSelectionEventHandler SubmittingToServerEvent;
        void RaiseSubmittingToServerEvent(DataModel.Frame submittedFrame);
    }
}
