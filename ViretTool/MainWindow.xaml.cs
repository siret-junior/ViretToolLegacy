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
            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb",
            //    "..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb");

            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-KF-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb");

            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\TRECVid700v\\TRECVid700v-KF-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid700v\\TRECVid700v-4fps-100x75.thumb");

            mDataset = new DataModel.Dataset(
                "TRECVid700v\\TRECVid700v-KF-100x75.thumb",
                "TRECVid700v\\TRECVid700v-4fps-100x75.thumb");


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

                    // log message
                    string message = "Connect request to setver: "
                            + "(IP:" + mSettings.IPAddress
                            + ", Port:" + mSettings.Port
                            + ", TeamName:" + mSettings.TeamName 
                            + "), Is connected: " + mSubmissionClient.IsConnected.ToString();
                    Logger.LogInfo(semanticModelDisplay, message);
                };

            // ranking model input
            keywordSearchTextBox.KeywordChangedEvent +=
                (query, annotationSource) =>
                {
                    DisableInput();
                    mRankingEngine.UpdateKeywordModelRanking(query, annotationSource);
                    EnableInput();

                    // logging
                    // build query objects string
                    string queryObjects = " ";
                    int queryCount;
                    if (query != null)
                    {
                        for (int i = 0; i < query.Count; i++)
                        {
                            queryObjects += "(";
                            for (int j = 0; j < query[i].Count; j++)
                            {
                                queryObjects += query[i][j].ToString() + ", ";
                            }
                            queryObjects += "),";
                        }
                        queryCount = query.Count;
                    }
                    else
                    {
                        queryObjects = "null";
                        queryCount = 0;
                    }

                    // log message
                    string message = "Keyword model changed: "
                        + "annotation source: " + annotationSource
                        + ", " + queryCount + " query objects:" + queryObjects;
                    Logger.LogInfo(keywordSearchTextBox, message);
                };
            sketchCanvas.SketchChangedEvent += 
                (sketch) => 
                {
                    DisableInput();
                    mRankingEngine.UpdateColorModelRanking(sketch);
                    EnableInput();

                    // logging
                    // build query objects string
                    string querySketch = " ";
                    int queryCount = 0;
                    if (sketch != null)
                    {
                        for (int i = 0; i < sketch.Count; i++)
                        {
                            Point point = sketch[i].Item1;
                            Color color = sketch[i].Item2;
                            querySketch += "{XY[" + point.X + "," + point.Y
                            + "], RGB[" + color.R + "," + color.G + "," + color.B + "]}, ";
                            queryCount++;
                        }
                    }
                    else
                    {
                        querySketch = "null";
                        queryCount = 0;
                    }

                    // log message
                    string message = "Color sketch model changed: "
                        + queryCount + " color points:" + querySketch;
                    Logger.LogInfo(sketchCanvas, message);
                };
            mFrameSelectionController.SelectionSubmittedColorModelEvent +=
                (frameSelection) =>
                {
                    DisableInput();
                    // TODO:
                    //semanticModelDisplay.DisplayFrames(frameSelection);
                    mRankingEngine.UpdateColorModelRanking(frameSelection);
                    EnableInput();

                    // logging
                    // build query objects string
                    string querySelection = " ";
                    int queryCount = 0;
                    if (frameSelection != null)
                    {
                        for (int i = 0; i < frameSelection.Count; i++)
                        {
                            DataModel.Frame frame = frameSelection[i];
                            querySelection += "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + "), ";
                            queryCount++;
                        }
                    }
                    else
                    {
                        querySelection = "null";
                        queryCount = 0;
                    }

                    // log message
                    string message = "Color model changed: "
                        + queryCount + " example frames:" + querySelection;
                    Logger.LogInfo(mFrameSelectionController, message);
                };
            mFrameSelectionController.SelectionSubmittedSemanticModelEvent +=
                (frameSelection) =>
                {
                    DisableInput();
                    semanticModelDisplay.DisplayFrames(frameSelection);
                    mRankingEngine.UpdateVectorModelRanking(frameSelection);
                    EnableInput();

                    // build query objects string
                    string querySelection = " ";
                    int queryCount = 0;
                    if (frameSelection != null)
                    {
                        for (int i = 0; i < frameSelection.Count; i++)
                        {
                            DataModel.Frame frame = frameSelection[i];
                            querySelection += "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + "), ";
                            queryCount++;
                        }
                    }
                    else
                    {
                        querySelection = "null";
                        queryCount = 0;
                    }

                    // log message
                    string message = "Semantic model changed: "
                        + queryCount + " example frames:" + querySelection;
                    Logger.LogInfo(mFrameSelectionController, message);
                };

            resultDisplay.DisplayRandomItemsRequestedEvent += 
                () =>
                {
                    DisableInput();
                    mRankingEngine.GenerateRandomRanking();
                    EnableInput();

                    // log message
                    string message = "User selected random display.";
                    Logger.LogInfo(resultDisplay, message);
                };
            resultDisplay.DisplaySequentialItemsRequestedEvent +=
                () =>
                {
                    DisableInput();
                    mRankingEngine.GenerateSequentialRanking();
                    EnableInput();

                    // log message
                    string message = "User selected sequential display.";
                    Logger.LogInfo(resultDisplay, message);
                };


            // ranking model output visualization
            mRankingEngine.RankingChangedEvent += 
                (rankedResult) =>
                {
                    resultDisplay.ResultFrames = rankedResult;
                    mFrameSelectionController.ResetSelection();

                    // build query objects string
                    int resultSize = rankedResult != null ? rankedResult.Count : 0;

                    // log message
                    string message = "Ranking updated, "
                         + resultSize + " ranked frames returned.";
                    Logger.LogInfo(mRankingEngine, message);
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

                    // logging
                    string currentTask = GetCurrentTaskId();

                    // log message
                    string message = currentTask + ", frame submitted: "
                            + "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + ")";
                    Logger.LogInfo(resultDisplay, message);
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

                    // logging
                    string currentTask = GetCurrentTaskId();

                    // log message
                    string message = currentTask + ", frame submitted: "
                            + "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + ")";
                    Logger.LogInfo(videoDisplay, message);
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

                    // logging
                    string currentTask = GetCurrentTaskId();

                    // log message
                    string message = currentTask + ", frame submitted: "
                            + "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + ")";
                    Logger.LogInfo(semanticModelDisplay, message);
                };

            mFrameSelectionController.SelectionChangedEvent +=
                (selectedFrames) =>
                {
                    resultDisplay.SelectedFrames = selectedFrames;
                    videoDisplay.SelectedFrames = selectedFrames;
                    semanticModelDisplay.SelectedFrames = selectedFrames;
                };

            // show frame video on video display
            resultDisplay.DisplayingFrameVideoEvent +=
                (frame) =>
                {
                    videoDisplay.DisplayFrameVideo(frame);
                    
                    // log message
                    string message = "Video displayed: "
                            + "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + ")";
                    Logger.LogInfo(resultDisplay, message);
                };
            videoDisplay.DisplayingFrameVideoEvent +=
                (frame) =>
                {
                    videoDisplay.DisplayFrameVideo(frame);

                    // log message
                    string message = "Video displayed: "
                            + "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + ")";
                    Logger.LogInfo(videoDisplay, message);
                };
            semanticModelDisplay.DisplayingFrameVideoEvent +=
                (frame) =>
                {
                    videoDisplay.DisplayFrameVideo(frame);

                    // log message
                    string message = "Video displayed: "
                            + "(Frame ID:" + frame.ID
                            + ", Video:" + frame.FrameVideo.VideoID
                            + ", Number:" + frame.FrameNumber + ")";
                    Logger.LogInfo(semanticModelDisplay, message);
                };

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

        private string GetCurrentTaskId()
        {
            TimeSpan taskTimeout = TimeSpan.FromMilliseconds(250);
            Task<int> asyncTask = mSubmissionClient.GetCurrentTaskId();
            string currentTask = "Task ID: ";
            if (asyncTask.Wait(taskTimeout))
            {
                currentTask += asyncTask.Result.ToString();
            }
            else
            {
                currentTask += "null";
            }
            return currentTask;
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

            // log message
            string message = "Reset of all models and their controls.";
            Logger.LogInfo(semanticModelDisplay, message);
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

            // log message
            string message = "Settings changed: "
                    + "(IP:" + mSettings.IPAddress
                    + ", Port:" + mSettings.Port
                    + ", TeamName:" + mSettings.TeamName
                    + "), Is connected: " + mSubmissionClient.IsConnected.ToString();
            Logger.LogInfo(semanticModelDisplay, message);
        }
    }
}
