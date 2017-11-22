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
using ViretTool.Utils;

namespace ViretTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataModel.Dataset mDataset;
        private RankingEngine mRankingEngine;
        private FrameSelectionController mFrameSelectionController;
        private Submission mSubmissionClient;
        private Settings mSettings = Settings.LoadSettings();

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

            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\TRECVid700v\\TRECVid700v-KF-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid700v\\TRECVid700v-4fps-100x75.thumb");


            // initialize ranking engine
            SimilarityManager similarityManager = new SimilarityManager(mDataset);
            FilterManager filterManager = new FilterManager(mDataset);
            mRankingEngine = new RankingEngine(similarityManager, filterManager);

            // TODO filter GUI
            mRankingEngine.VideoAggregateFilterEnabled = true;
            mRankingEngine.VideoAggregateFilterMaxFrames = 10;

            keywordSearchTextBox.Init(mDataset, new string[] {
                "GoogLeNet", "YFCC100M"
            });

            // initialize selection controller
            mFrameSelectionController = new FrameSelectionController();

            // initialize submission client
            mSubmissionClient = new Submission();
            mSubmissionClient.Connect(mSettings.IPAddress, mSettings.Port, mSettings.TeamName);
            mSettings.SettingsChangedEvent +=
                (settings) =>
                {
                    mSubmissionClient.Connect(settings.IPAddress, settings.Port, settings.TeamName);
                };

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
                    
                    // TODO:
                    //mRankingEngine.UpdateColorModelRanking(new List<Tuple<Point, Color>>());

                    EnableInput();
                };
            mFrameSelectionController.SelectionSubmittedColorModelEvent +=
                (frameSelection) =>
                {
                    DisableInput();
                    // TODO:
                    //semanticModelDisplay.DisplayFrames(frameSelection);
                    mRankingEngine.UpdateColorModelRanking(frameSelection);
                    EnableInput();
                };
            mFrameSelectionController.SelectionSubmittedSemanticModelEvent +=
                (frameSelection) =>
                {
                    DisableInput();
                    semanticModelDisplay.DisplayFrames(frameSelection);
                    mRankingEngine.UpdateVectorModelRanking(frameSelection);
                    EnableInput();
                };

            resultDisplay.DisplayRandomItemsRequestedEvent += 
                () =>
                {
                    DisableInput();
                    mRankingEngine.GenerateRandomRanking();
                    EnableInput();
                };
            resultDisplay.DisplaySequentialItemsRequestedEvent +=
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
                    mFrameSelectionController.ResetSelection();
                };


            // frame selection
            resultDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            resultDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            resultDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            resultDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            resultDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            resultDisplay.SubmittingToServerEvent +=
                (frame) =>
                {
                    mSubmissionClient.Send(frame.FrameVideo.VideoID, frame.FrameNumber);
                };

            videoDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            videoDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            videoDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            videoDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            videoDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            videoDisplay.SubmittingToServerEvent +=
                (frame) =>
                {
                    mSubmissionClient.Send(frame.FrameVideo.VideoID, frame.FrameNumber);
                };

            semanticModelDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            semanticModelDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            semanticModelDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            semanticModelDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            semanticModelDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            semanticModelDisplay.SubmittingToServerEvent +=
                (frame) =>
                {
                    mSubmissionClient.Send(frame.FrameVideo.VideoID, frame.FrameNumber);
                };

            mFrameSelectionController.SelectionChangedEvent +=
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

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            resultDisplay.UpdateDisplayGrid();
        }

        private void clearAllButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: without reranking in between
            keywordSearchTextBox.Clear();
            sketchCanvas.Clear();
            mFrameSelectionController.ResetSelection();
            mFrameSelectionController.SubmitSelectionSemanticModel();
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Home:
                    resultDisplay.DisplayPage(0);
                    break;
                //case Key.End: // TODO
                //    resultDisplay.DisplayPage(0);
                //    break;
                case Key.PageDown:
                    resultDisplay.IncrementDisplay(10);
                    break;
                case Key.PageUp:
                    resultDisplay.IncrementDisplay(-10);
                    break;
                case Key.Right:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        resultDisplay.IncrementDisplay(10);
                    }
                    else
                    {
                        resultDisplay.IncrementDisplay(1);
                    }
                    break;
                case Key.Left:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        resultDisplay.IncrementDisplay(-10);
                    }
                    else
                    {
                        resultDisplay.IncrementDisplay(-1);
                    }
                    break;
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            mSettings.OpenSettingsWindow();
        }
    }
}
