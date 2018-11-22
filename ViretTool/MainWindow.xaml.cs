using System;
using System.Collections.Generic;
using System.IO;
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
using ViretTool.DataModel;
using ViretTool.InteractionLogging;
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

        private RandomScenePlayer mRandomScenePlayer;
        private VBSLogger mVBSLogger;

        private Cursor mPreviousCursor;

        private Video selectedVideo = null;

        public MainWindow()
        {
            InitializeComponent();

            // TODO - use unique toolID
            mVBSLogger = new VBSLogger("1");
            // prepare data model
            //mDataset = new DataModel.Dataset("..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb", "..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb");

            //mDataset = new DataModel.Dataset("..\\..\\..\\TestData\\TRECVid\\TRECVid-KF-100x75.thumb", "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb");

            //StringBuilder SB = new StringBuilder(); int c = 0;
            //foreach (DataModel.Frame f in mDataset.Frames)
            //{
            //    BitmapSource bs = f.Bitmap;
            //    byte[] pixels = ImageHelper.ResizeAndStoreImageToRGBByteArray(bs, (int)bs.Width, (int)bs.Height);
            //    double pc = pixels.Length / 3, black = 0, white = 0, intensity = 0;
            //    for (int i = 0; i < pc; i++)
            //    {
            //        int offset = i * 3;
            //        if (pixels[offset] < 32 && pixels[offset + 1] < 32 && pixels[offset + 2] < 32) black++;
            //        if (pixels[offset] > 224 && pixels[offset + 1] > 224 && pixels[offset + 2] > 224) white++;
            //        intensity += 0.2126 * pixels[offset] + 0.7152 * pixels[offset + 1] + 0.0722 * pixels[offset + 2];
            //    }
            //    SB.AppendLine((black / pc).ToString("0.00") + " " + (white / pc).ToString("0.00") + " " + (intensity / pc).ToString("0.00"));
            //    if (c++ > 50000) break;
            //}
            //Clipboard.SetText(SB.ToString());

            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\TRECVid700v\\TRECVid700v-KF-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid700v\\TRECVid700v-4fps-100x75.thumb");

            //mDataset = new DataModel.Dataset(
            //    "TRECVid700v\\TRECVid700v-KF-100x75.thumb",
            //    "TRECVid700v\\TRECVid700v-4fps-100x75.thumb");

            //const int MAX_VIDEO_COUNT = 700;
            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-selected-100x75.thumb",
            //    "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-selected.topology",
            //    MAX_VIDEO_COUNT);

            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\TestData\\CT24\\CT24-4fps-100x75.thumb",
            //    "..\\..\\..\\TestData\\CT24\\CT24-4fps-selected-100x75.thumb",
            //    "..\\..\\..\\TestData\\CT24\\CT24-4fps-selected.topology");

            //mDataset = new DataModel.Dataset(
            //        "..\\..\\..\\TestData\\Trailers\\Trailers-8fps-100x75.thumb",
            //        "..\\..\\..\\TestData\\Trailers\\Trailers-8fps-selected-100x75.thumb",
            //        "..\\..\\..\\TestData\\Trailers\\Trailers-8fps-selected.topology");

            mDataset = FromConfigFile("ViretToolConfig.txt");
            

            // initialize ranking engine
            SimilarityManager similarityManager = new SimilarityManager(mDataset);
            FilterManager filterManager = new FilterManager(mDataset);
            mRankingEngine = new RankingEngine(similarityManager, filterManager);

            // initialize selection controller
            mFrameSelectionController = new FrameSelectionController();


            #region --[ Filter events ]--

            // filter changed events
            // default value can be set like...
            //      filterBW.DefaultValue = 0.5;
            filterBW.DefaultValue = 90;
            filterBW.FilterChangedEvent += (state, value) => {
                DisableInput();
                mRankingEngine.SetBlackAndWhiteFilter(state != FilterControl.FilterState.Off, state == FilterControl.FilterState.N);
                mRankingEngine.SetBlackAndWhiteFilterMask((float)value);
                EnableInput();
            };

            filterPercentageOfBlack.DefaultValue = 65;
            filterPercentageOfBlack.FilterChangedEvent += (state, value) => {
                DisableInput();
                mRankingEngine.SetPercentageOfBlackColorFilter(state != FilterControl.FilterState.Off, state == FilterControl.FilterState.N);
                mRankingEngine.SetPercentageOfBlackColorFilterMask((float)value);
                EnableInput();
            };


            // model filters
            keywordSearchControlBar.DefaultValue = 50;
            keywordSearchControlBar.ModelSettingChangedEvent += (value, useForSorting) => {
                mRankingEngine.ComputeResult = false;
                mRankingEngine.SortByKeyword = useForSorting;
                mRankingEngine.ComputeResult = true;
                mRankingEngine.SetFilterThresholdForKeywordModel(value);
            };
            keywordSearchControlBar.ModelClearedEvent += () => {
                keywordSearchTextBox.Clear();
            };

            sketchCanvasControlBar.DefaultValue = 90;
            sketchCanvasControlBar.ModelSettingChangedEvent += (value, useForSorting) => {
                mRankingEngine.ComputeResult = false;
                mRankingEngine.SortByColor = useForSorting;
                mRankingEngine.ComputeResult = true;
                mRankingEngine.SetFilterThresholdForColorModel(value);
            };
            sketchCanvasControlBar.ModelClearedEvent += () => {
                sketchCanvas.Clear();
            };

            semanticModelControlBar.DefaultValue = 70;
            semanticModelControlBar.ModelSettingChangedEvent += (value, useForSorting) => {
                mRankingEngine.ComputeResult = false;
                mRankingEngine.SortBySemantic = useForSorting;
                mRankingEngine.ComputeResult = true;
                mRankingEngine.SetFilterThresholdForSemanticModel(value);
            };
            semanticModelControlBar.ModelClearedEvent += () => {
                mFrameSelectionController.ResetSelection();
                mFrameSelectionController.SubmitSelectionSemanticModel();
            };

            

            // TODO filter GUI
            mRankingEngine.VideoAggregateFilterEnabled = true;
            mRankingEngine.VideoAggregateFilterMaxVideoFrames = 15;
            mRankingEngine.VideoAggregateFilterMaxShotFrames = 1;

            keywordSearchTextBox.Init(mDataset, new string[] {
                "GoogLeNet"//, "YFCC100M", "Audio"
            });


            #endregion


            
            // initialize submission client
            mSubmissionClient = new Submission();
            mSubmissionClient.Connect("dummy", 80, "dummy");
            //mSubmissionClient.Connect(mSettings.IPAddress, mSettings.Port, mSettings.TeamName);
            mSettings.SettingsChangedEvent +=
                (settings) =>
                {
                    mSubmissionClient.Connect(settings.IPAddress, settings.Port, settings.TeamName);
                    InteractionLogger.Instance.SetTeamName(settings.TeamName);

                    // log message
                    string message = "Connect request to server: "
                            + "(IP:" + mSettings.IPAddress
                            + ", Port:" + mSettings.Port
                            + ", TeamName:" + mSettings.TeamName 
                            + "), Is connected: " + mSubmissionClient.IsConnected.ToString();
                    Logger.LogInfo(mSettings, message);
                };


            #region --[ Ranking model input ]--

            // ranking model input
            keywordSearchTextBox.KeywordChangedEvent +=
                (query, annotationSource) =>
                {
                    if (query != null)
                    {
                        VBSLogger.AppendActionIncludeTimeParameter('K', true);
                        mRankingEngine.ComputeResult = false;
                        semanticModelControlBar.UncheckMe();
                        sketchCanvasControlBar.UncheckMe();
                        keywordSearchControlBar.CheckMe();
                        mRankingEngine.ComputeResult = true;
                    }
                    else
                    {
                        VBSLogger.AppendActionIncludeTimeParameter('K', false);
                    }
                    DisableInput();

                    mRankingEngine.UpdateKeywordModelRankingAndFilterMask(query, annotationSource);
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
            //sketchCanvas.SketchChangingEvent += colorModelDisplay.Clear;
            sketchCanvas.SketchChangedEvent += 
                (sketch) => 
                {
                    if (sketch.Count > 0)
                    {
                        VBSLogger.AppendActionIncludeTimeParameter('C', true);
                        mRankingEngine.ComputeResult = false;
                        keywordSearchControlBar.UncheckMe();
                        semanticModelControlBar.UncheckMe();
                        sketchCanvasControlBar.CheckMe();
                        mRankingEngine.ComputeResult = true;
                    }
                    else
                    {
                        VBSLogger.AppendActionIncludeTimeParameter('C', false);
                    }
                    DisableInput();
                    mRankingEngine.UpdateColorModelRankingAndFilterMask(sketch);
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

                    InteractionLogger.Instance.LogInteraction("sketch", "color",
                        string.Join(", ", sketch.Select(x =>
                        "[P(" + x.Item1.X + "; " + x.Item1.Y + "), "
                        + "C(" + x.Item2.R + "; " + x.Item2.G + "; " + x.Item2.B + "), "
                        + "E(" + x.Item3.X + "; " + x.Item3.Y + "), "
                        + (x.Item4 ? "all" : "any")
                        + "]"
                        )));

                };
            //mFrameSelectionController.SelectionSubmittedColorModelEvent +=
            //    (frameSelection) =>
            //    {
            //        DisableInput();
            //        // TODO:
            //        colorModelDisplay.DisplayFrames(frameSelection);
            //        //mRankingEngine.UpdateColorModelRanking(frameSelection);
            //        EnableInput();

            //        // logging
            //        // build query objects string
            //        string querySelection = " ";
            //        int queryCount = 0;
            //        if (frameSelection != null)
            //        {
            //            for (int i = 0; i < frameSelection.Count; i++)
            //            {
            //                DataModel.Frame frame = frameSelection[i];
            //                querySelection += "(Frame ID:" + frame.ID
            //                + ", Video:" + frame.FrameVideo.VideoID
            //                + ", Number:" + frame.FrameNumber + "), ";
            //                queryCount++;
            //            }
            //        }
            //        else
            //        {
            //            querySelection = "null";
            //            queryCount = 0;
            //        }

            //        // log message
            //        string message = "Color model changed: "
            //            + queryCount + " example frames:" + querySelection;
            //        Logger.LogInfo(mFrameSelectionController, message);
            //    };
            mFrameSelectionController.SelectionSubmittedSemanticModelEvent +=
                (frameSelection) =>
                {
                    // TODO: Nekde je chyba... 
                    // Zavolanim ModelSettingChangedEvent na semanticModelControlBar to nezobrazi vysledek
                    if (frameSelection.Count > 0)
                    {
                        VBSLogger.AppendActionIncludeTimeParameter('S', true);
                        mRankingEngine.ComputeResult = false;
                        keywordSearchControlBar.UncheckMe();
                        sketchCanvasControlBar.UncheckMe();
                        semanticModelControlBar.CheckMe();
                        mRankingEngine.ComputeResult = true;

                        InteractionLogger.Instance.LogInteraction("image", "dataset",
                            string.Join(", ", frameSelection.Select(x => 
                            "[V(" + x.ParentVideo.Id + "), F(" + x.FrameNumber + ")]")), 
                            "select");
                    }
                    else
                    {
                        VBSLogger.AppendActionIncludeTimeParameter('S', false);

                        InteractionLogger.Instance.LogInteraction("image", "dataset",
                            string.Join(", ", frameSelection.Select(x =>
                            "[V(" + x.ParentVideo.Id + "), F(" + x.FrameNumber + ")]")),
                            "deselect");
                    }
                    DisableInput();
                    semanticModelDisplay.DisplayFrames(frameSelection);
                    semanticModelDisplay.SelectedFrames = frameSelection;
                    mRankingEngine.UpdateVectorModelRankingAndFilterMask(frameSelection, false);
                    EnableInput();

                    // build query objects string
                    string querySelection = " ";
                    int queryCount = 0;
                    if (frameSelection != null)
                    {
                        for (int i = 0; i < frameSelection.Count; i++)
                        {
                            DataModel.Frame frame = frameSelection[i];
                            querySelection += "(Frame ID:" + frame.Id
                            + ", Video:" + frame.ParentVideo.Id
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
                    VBSLogger.AppendActionIncludeTimeParameter('B', true);
                    InteractionLogger.Instance.LogInteraction("browsing", "toolLayout", "random");

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
                    VBSLogger.AppendActionIncludeTimeParameter('B', true);
                    InteractionLogger.Instance.LogInteraction("browsing", "toolLayout", "sequential");

                    DisableInput();
                    mRankingEngine.GenerateSequentialRanking();
                    EnableInput();

                    // log message
                    string message = "User selected sequential display.";
                    Logger.LogInfo(resultDisplay, message);
                };

            #endregion



            // TODO - show mRandomScenePlayer.ReturnSearchedItemPosition
            mRandomScenePlayer = new RandomScenePlayer(mDataset, TestButton, 200);


            // ranking model output visualization
            mRankingEngine.RankingChangedEvent += 
                (rankedResult) =>
                {
                    //ShowRank(rankedResult);
                    if (rankedResult != null && rankedResult.Count > 0)
                    {
                        string trainingSearchString = mRandomScenePlayer.ReturnSearchedItemPosition(rankedResult);
                        trainingSearchLabel.Content = trainingSearchString;
                    }
                    else
                    {
                        trainingSearchLabel.Content = "";
                    }

                    resultDisplay.ResultFrames = rankedResult;
                    //mFrameSelectionController.ResetSelection();

                    // build query objects string
                    int resultSize = rankedResult != null ? rankedResult.Count : 0;

                    // log message
                    string message = "Ranking updated, "
                         + resultSize + " ranked frames returned.";
                    Logger.LogInfo(mRankingEngine, message);
                };


            #region --[ Frame selection ]--
            
            // frame selection events
            resultDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            resultDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            //resultDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            //resultDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            resultDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            resultDisplay.SubmittingToServerEvent += OpenSubmitWindow;

            videoDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            videoDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            //videoDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            //videoDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            videoDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            videoDisplay.SubmittingToServerEvent += OpenSubmitWindow;

            //colorModelDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            //colorModelDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            ////colorModelDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            ////colorModelDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            //colorModelDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            //colorModelDisplay.SubmittingToServerEvent += OpenSubmitWindow;
            //colorModelDisplay.ColorExampleChangingEvent += sketchCanvas.DeletePoints;


            semanticModelDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            semanticModelDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            //semanticModelDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            //semanticModelDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            semanticModelDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            semanticModelDisplay.SubmittingToServerEvent += OpenSubmitWindow;

            mFrameSelectionController.SelectionChangedEvent +=
                (selectedFrames) =>
                {
                    resultDisplay.SelectedFrames = selectedFrames;
                    videoDisplay.SelectedFrames = selectedFrames;
                    //colorModelDisplay.SelectedFrames = selectedFrames;
                    semanticModelDisplay.DisplayFrames(selectedFrames);
                    semanticModelDisplay.SelectedFrames = selectedFrames;
                };

            #endregion


            #region --[ Video display ]--

            // show frame video on video display
            resultDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;
            videoDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;
            //colorModelDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;
            semanticModelDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;


            #endregion

            resultDisplay.ShowFilteredVideosEnabledEvent +=
                () =>
                {
                    mRankingEngine.DisableVideoFilter();
                };
            resultDisplay.ShowFilteredVideosDisabledEvent +=
                () =>
                {
                    mRankingEngine.EnableVideoFilter();
                };
            resultDisplay.FilterSelectedVideoEvent +=
                () =>
                {
                    if (selectedVideo != null)
                    {
                        mRankingEngine.AddVideoToFilterList(selectedVideo);
                    }
                };
            resultDisplay.Max3FromVideoEnabledEvent +=
                () =>
                {
                    mRankingEngine.VideoAggregateFilterMaxVideoFrames = 3;

                };
            resultDisplay.Max3FromVideoDisabledEvent +=
                () =>
                {
                    mRankingEngine.VideoAggregateFilterMaxVideoFrames = 15;
                };

            // set first display
            mRankingEngine.GenerateSequentialRanking();

            
            TestButton.Height = 350;
            InteractionLogger.Instance.ResetLog();

        }


        private Dataset FromConfigFile(string configFile)
        {
            string thumbnailsAllFile;
            string thumbnailsSelectedFile;
            string topologyFile;

            // load config file
            try
            {
                using (StreamReader reader = new StreamReader(configFile))
                {
                    thumbnailsAllFile = reader.ReadLine();
                    thumbnailsSelectedFile = reader.ReadLine();
                    topologyFile = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                string message = "Error reading config file: " + configFile;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new IOException(message);
            }

            // load database object
            try
            {
                // TODO: new code integration
                //Dataset dataset = DatasetProvider.FromFilelist(topologyFile, "V3C1-first750");
                //Dataset dataset = DatasetProvider.FromFilelist(@"c:\Datasets\V3C1-first750\KeyFrames\filelist.txt", "V3C1-first750");
                //DatasetProvider.ToBinaryFile(dataset, topologyFile + ".topology");
                //Dataset dataset = DatasetProvider.FromBinaryFile(topologyFile);
                Dataset dataset = DatasetProvider.ConstructDataset(thumbnailsAllFile, thumbnailsSelectedFile, topologyFile);

                //return new Dataset(thumbnailsAllFile, thumbnailsSelectedFile, topologyFile);
                return dataset;
            }
            catch (Exception ex)
            {
                string message = "Error creating database file using config: " + configFile;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new IOException(message);
            }
        }

        //private void ShowRank(List<RankedFrame> result)
        //{
        //    if (mSearchedFrame != null)
        //    {
        //        TestLabel.Content = "";

        //        for (int i = 0; i < result.Count; i++)
        //            if (result[i].Frame.FrameVideo.VideoID == mSearchedFrame.FrameVideo.VideoID)
        //            {
        //                TestLabel.Content = "video: " + i; break;
        //            }          

        //        for (int i = 0; i < result.Count; i++)
        //            if (result[i].Frame.ID == mSearchedFrame.ID)
        //            {
        //                TestLabel.Content += ", frame:" + i; return;
        //            }

        //        TestLabel.Content += " frame filtered";
        //    }
        //}

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


        #region --[ Submission ]--

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

        private void OpenSubmitWindow(DataModel.Frame frame)
        {
            SubmitWindow window = new SubmitWindow(mSubmissionClient, frame);
            window.ShowDialog();
        }


        private void SubmitToServer(DataModel.Frame frame)
        {
            mSubmissionClient.Send(frame.ParentVideo.Id, frame.FrameNumber);

            // logging
            //string currentTask = GetCurrentTaskId();

            // log message
            string message = /*currentTask + */", frame submitted: "
                    + "(Frame ID:" + frame.Id
                    + ", Video:" + frame.ParentVideo.Id
                    + ", Number:" + frame.FrameNumber + ")";
            Logger.LogInfo(this, message);

            InteractionLogger.Instance.SubmitLog();
        }

        #endregion


        private void LogVideoDisplayed(DataModel.Frame frame)
        {
            videoDisplay.DisplayFrameVideo(frame);
            selectedVideo = frame.ParentVideo;

            // log message
            string message = "Video displayed: "
                    + "(Frame ID:" + frame.Id
                    + ", Video:" + frame.ParentVideo.Id
                    + ", Number:" + frame.FrameNumber + ")";
            Logger.LogInfo(this, message);
            InteractionLogger.Instance.LogInteraction(
                "browsing", "video", "V(" + frame.ParentVideo.Id + "), F(" + frame.FrameNumber +  ")", "show video");
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            resultDisplay.UpdateDisplayGrid();
        }

        private void clearAllButton_Click(object sender, RoutedEventArgs e)
        {
            //VBSLogger.AppendActionIncludeTimeParameter('X', true);
            // TODO: without reranking in between
            mRankingEngine.ComputeResult = false;
            keywordSearchTextBox.Clear();
            sketchCanvas.Clear();
            mFrameSelectionController.ResetSelection();
            mFrameSelectionController.SubmitSelectionSemanticModel();

            filterBW.Reset();
            filterPercentageOfBlack.Reset();

            keywordSearchControlBar.Clear();
            sketchCanvasControlBar.Clear();
            mRankingEngine.ComputeResult = true;

            semanticModelControlBar.Clear();

            mRankingEngine.ResetVideoFilter();

            // TODO - is it OK/enough to set to null??
            //TestButton.Background = Brushes.LightGray;
            mRandomScenePlayer.Reset();

            // log message
            string message = "Reset of all models and their controls.";
            Logger.LogInfo(semanticModelDisplay, message);

            InteractionLogger.Instance.LogInteraction("resetall");
        }

        private void filtersClearButton_Click(object sender, RoutedEventArgs e) {
            // TODO: without reranking in between
            filterBW.Reset();
            filterPercentageOfBlack.Reset();
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

        //private void filterVideoButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (selectedVideo != null)
        //    {
        //        mRankingEngine.AddVideoToFilterList(selectedVideo);
        //    }
        //}

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            clearAllButton_Click(this, null);
            VBSLogger.ResetLog();
            InteractionLogger.Instance.ResetLog();
        }
    }

    class SubmitWindow : Window
    {
        private Submission mSubmissionClient;
        private DataModel.Frame mFrame;

        public SubmitWindow(Submission submissionClient, DataModel.Frame frame)
        {
            mSubmissionClient = submissionClient;
            mFrame = frame;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Background = Brushes.DarkGray;

            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Center;
            grid.Margin = new Thickness(32);

            ColumnDefinition gridCol1 = new ColumnDefinition();
            gridCol1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition gridCol2 = new ColumnDefinition();
            gridCol2.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(gridCol1);
            grid.ColumnDefinitions.Add(gridCol2);

            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = GridLength.Auto;
            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = GridLength.Auto;
            RowDefinition gridRow3 = new RowDefinition();
            gridRow3.Height = new GridLength(32);
            grid.RowDefinitions.Add(gridRow1);
            grid.RowDefinitions.Add(gridRow2);
            grid.RowDefinitions.Add(gridRow3);
            this.AddChild(grid);


            Label question = new Label();
            question.Content = "Are you sure you want to submit this frame?";
            question.FontSize = 32;
            Grid.SetColumn(question, 0);
            Grid.SetRow(question, 0);
            question.HorizontalAlignment = HorizontalAlignment.Center;
            question.VerticalAlignment = VerticalAlignment.Center;
            question.SetValue(Grid.ColumnSpanProperty, 2);
            grid.Children.Add(question);

            Image image = new Image();
            image.Source = frame.Bitmap;
            image.Width = 320;
            image.Height = 240;
            image.Margin = new Thickness(32);
            Grid.SetColumn(image, 0);
            Grid.SetRow(image, 1);
            image.SetValue(Grid.ColumnSpanProperty, 2);
            grid.Children.Add(image);


            Button cancel = new Button();
            cancel.Content = "No";
            cancel.Background = Brushes.LightCoral;
            Grid.SetColumn(cancel, 0);
            Grid.SetRow(cancel, 2);
            cancel.Click += cancel_Click;
            grid.Children.Add(cancel);

            Button submit = new Button();
            submit.Content = "Yes";
            submit.Background = Brushes.DarkSeaGreen;
            Grid.SetColumn(submit, 1);
            Grid.SetRow(submit, 2);
            submit.Click += submit_Click;
            grid.Children.Add(submit);

        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            SubmitToServer(mFrame);
            Close();
        }

        private void SubmitToServer(DataModel.Frame frame)
        {
            mSubmissionClient.Send(frame.ParentVideo.Id, frame.FrameNumber);

            // logging
            string currentTask = GetCurrentTaskId();

            // log message
            string message = currentTask + ", frame submitted: "
                    + "(Frame ID:" + frame.Id
                    + ", Video:" + frame.ParentVideo.Id
                    + ", Number:" + frame.FrameNumber + ")";
            Logger.LogInfo(this, message);
            InteractionLogger.Instance.SubmitLog();
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
    }
}
