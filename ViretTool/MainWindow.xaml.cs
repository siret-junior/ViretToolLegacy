using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViretTool.BasicClient;
using ViretTool.RankingModel;
using ViretTool.RankingModel.FilterModels;
using ViretTool.RankingModel.SimilarityModels;

namespace ViretTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataModel.Dataset mDataset;
        private RankingEngine mRankingEngine;
        private Cursor mPreviousCursor;

        public MainWindow()
        {
            InitializeComponent();

            // prepare data model
            mDataset = new DataModel.Dataset(
                "..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb",
                "..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb");
            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-KF-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb");


            // initialize ranking engine
            SimilarityManager similarityManager = new SimilarityManager(mDataset);
            FilterManager filterManager = new FilterManager(mDataset);
            mRankingEngine = new RankingEngine(similarityManager, filterManager);

            // TODO filter GUI
            mRankingEngine.VideoAggregateFilterEnabled = true;
            mRankingEngine.VideoAggregateFilterMaxFrames = 2;

            keywordSearchTextBox.Init(mDataset, new string[] {
                "GoogLeNet", "YFCC100M"
            });

            // initialize selection controller
            FrameSelectionController frameSelectionController
                = new FrameSelectionController();

            // ranking model input
            keywordSearchTextBox.KeywordChangedEvent +=
                (query, annotationSource) =>
                {
                    DisableInput();
                    mRankingEngine.UpdateKeywordModelRanking(query, annotationSource);
                    EnableInput();
                };
            sketchCanvas.SketchChangedEvent += 
                (sketch) => 
                {
                    DisableInput();
                    mRankingEngine.UpdateColorModelRanking(sketch);
                    EnableInput();
                };
            frameSelectionController.SelectionSubmittedEvent +=
                (frameSelection) =>
                {
                    DisableInput();
                    semanticModelDisplay.DisplayFrames(frameSelection);
                    mRankingEngine.UpdateVectorModelRanking(frameSelection);
                    EnableInput();
                };

            resultDisplay.DisplayRandomItemsEvent += 
                () =>
                {
                    DisableInput();
                    mRankingEngine.GenerateRandomRanking();
                    EnableInput();
                };
            resultDisplay.DisplaySequentialItemsEvent +=
                () =>
                {
                    DisableInput();
                    mRankingEngine.GenerateSequentialRanking();
                    EnableInput();
                };


            // ranking model output visualization
            mRankingEngine.RankingChangedEvent += 
                (rankedResult) =>
                {
                    resultDisplay.ResultFrames = rankedResult;
                    frameSelectionController.ResetSelection();
                };


            // frame selection
            resultDisplay.AddingToSelectionEvent += frameSelectionController.AddToSelection;
            resultDisplay.RemovingFromSelectionEvent += frameSelectionController.RemoveFromSelection;
            resultDisplay.ResettingSelectionEvent += frameSelectionController.ResetSelection;
            resultDisplay.SubmittingSelectionEvent += frameSelectionController.SubmitSelection;

            videoDisplay.AddingToSelectionEvent += frameSelectionController.AddToSelection;
            videoDisplay.RemovingFromSelectionEvent += frameSelectionController.RemoveFromSelection;
            videoDisplay.ResettingSelectionEvent += frameSelectionController.ResetSelection;
            videoDisplay.SubmittingSelectionEvent += frameSelectionController.SubmitSelection;

            semanticModelDisplay.AddingToSelectionEvent += frameSelectionController.AddToSelection;
            semanticModelDisplay.RemovingFromSelectionEvent += frameSelectionController.RemoveFromSelection;
            semanticModelDisplay.ResettingSelectionEvent += frameSelectionController.ResetSelection;
            semanticModelDisplay.SubmittingSelectionEvent += frameSelectionController.SubmitSelection;

            frameSelectionController.SelectionChangedEvent +=
                (selectedFrames) =>
                {
                    resultDisplay.SelectedFrames = selectedFrames;
                    videoDisplay.SelectedFrames = selectedFrames;
                    semanticModelDisplay.SelectedFrames = selectedFrames;
                };

            // show frame video on video display
            resultDisplay.DisplayingFrameVideoEvent += videoDisplay.DisplayFrameVideo;
            videoDisplay.DisplayingFrameVideoEvent += videoDisplay.DisplayFrameVideo;
            semanticModelDisplay.DisplayingFrameVideoEvent += videoDisplay.DisplayFrameVideo;

            // set first display
            mRankingEngine.GenerateSequentialRanking();
        }

        private void DisableInput()
        {
            // set wait cursor
            mPreviousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            // disable form input
            mainWindow.IsEnabled = false;

            // TODO
            //keywordSearchTextBox.IsEnabled = false;
            //sketchCanvas.IsEnabled = false;
            //semanticModelDisplay.IsEnabled = false;

            //resultDisplay.IsEnabled = false;
            //videoDisplay.IsEnabled = false;
        }

        private void EnableInput()
        {
            // restore previous cursor
            Mouse.OverrideCursor = mPreviousCursor;

            // enable form input
            mainWindow.IsEnabled = true;
            
            // TODO
            //keywordSearchTextBox.IsEnabled = true;
            //sketchCanvas.IsEnabled = true;
            //semanticModelDisplay.IsEnabled = true;

            //resultDisplay.IsEnabled = true;
            //videoDisplay.IsEnabled = true;
        }
    }
}
