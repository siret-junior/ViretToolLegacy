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

            mDataset = new DataModel.Dataset(
                "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb",
                "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-selected-100x75.thumb",
                "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-selected.topology");


            // initialize ranking engine
            SimilarityManager similarityManager = new SimilarityManager(mDataset);
            FilterManager filterManager = new FilterManager(mDataset);
            mRankingEngine = new RankingEngine(similarityManager, filterManager);

            // filter changed events
            // default value can be set like...
            //      filterBW.DefaultValue = 0.5;
            filterBW.DefaultValue = 0.2;
            filterBW.FilterChangedEvent += (state, value) => {
                DisableInput();
                mRankingEngine.SetBlackAndWhiteFilter(state != FilterControl.FilterState.Off, state == FilterControl.FilterState.N);
                mRankingEngine.SetBlackAndWhiteFilterMask((float)value);
                EnableInput();
            };

            filterPercentageOfBlack.FilterChangedEvent += (state, value) => {
                DisableInput();
                mRankingEngine.SetPercentageOfBlackColorFilter(state != FilterControl.FilterState.Off, state == FilterControl.FilterState.N);
                mRankingEngine.SetPercentageOfBlackColorFilterMask((float)value);
                EnableInput();
            };


            // model filters
            keywordSearchControlBar.DefaultValue = 0.5;
            keywordSearchControlBar.ModelSettingChangedEvent += (value, useForSorting) => {
                mRankingEngine.SortByKeyword = useForSorting;
                mRankingEngine.SetFilterThresholdForKeywordModel(value);
            };
            keywordSearchControlBar.ModelClearedEvent += () => {
                keywordSearchTextBox.Clear();
            };

            sketchCanvasControlBar.DefaultValue = 0.95;
            sketchCanvasControlBar.ModelSettingChangedEvent += (value, useForSorting) => {
                mRankingEngine.SortByColor = useForSorting;
                mRankingEngine.SetFilterThresholdForColorModel(value);
            };
            sketchCanvasControlBar.ModelClearedEvent += () => {
                sketchCanvas.Clear();
            };

            semanticModelControlBar.DefaultValue = 0.70;
            semanticModelControlBar.ModelSettingChangedEvent += (value, useForSorting) => {
                mRankingEngine.SortBySemantic = useForSorting;
                mRankingEngine.SetFilterThresholdForSemanticModel(value);
            };
            semanticModelControlBar.ModelClearedEvent += () => {
                mFrameSelectionController.ResetSelection();
                mFrameSelectionController.SubmitSelectionSemanticModel();
            };




            // TODO filter GUI
            mRankingEngine.VideoAggregateFilterEnabled = true;
            mRankingEngine.VideoAggregateFilterMaxFrames = 15;

            keywordSearchTextBox.Init(mDataset, new string[] {
                "GoogLeNet", "YFCC100M"
            });

            // initialize selection controller
            mFrameSelectionController = new FrameSelectionController();

            // initialize submission client
            mSubmissionClient = new Submission();
            //mSubmissionClient.Connect(mSettings.IPAddress, mSettings.Port, mSettings.TeamName);
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
                    Logger.LogInfo(mSettings, message);
                };

            // ranking model input
            keywordSearchTextBox.KeywordChangedEvent +=
                (query, annotationSource) =>
                {
                    if (!keywordSearchControlBar.UseForSorting && !sketchCanvasControlBar.UseForSorting && !semanticModelControlBar.UseForSorting && query != null) {
                        keywordSearchControlBar.CheckMe();
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
            sketchCanvas.SketchChangingEvent += colorModelDisplay.Clear;
            sketchCanvas.SketchChangedEvent += 
                (sketch) => 
                {
                    if (!keywordSearchControlBar.UseForSorting && !sketchCanvasControlBar.UseForSorting && !semanticModelControlBar.UseForSorting && sketch.Count > 0) {
                        sketchCanvasControlBar.CheckMe();
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
                };
            mFrameSelectionController.SelectionSubmittedColorModelEvent +=
                (frameSelection) =>
                {
                    DisableInput();
                    // TODO:
                    colorModelDisplay.DisplayFrames(frameSelection);
                    //mRankingEngine.UpdateColorModelRanking(frameSelection);
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
                    // TODO: Nekde je chyba... 
                    // Zavolanim ModelSettingChangedEvent na semanticModelControlBar to nezobrazi vysledek
                    //if (!keywordSearchControlBar.UseForSorting && !sketchCanvasControlBar.UseForSorting && !semanticModelControlBar.UseForSorting && frameSelection.Count > 0) {
                    //    semanticModelControlBar.CheckMe();
                    //}
                    DisableInput();
                    semanticModelDisplay.DisplayFrames(frameSelection);
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
                    ShowRank(rankedResult);

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
            resultDisplay.SubmittingToServerEvent += OpenSubmitWindow;

            videoDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            videoDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            videoDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            videoDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            videoDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            videoDisplay.SubmittingToServerEvent += OpenSubmitWindow;

            colorModelDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            colorModelDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            colorModelDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            colorModelDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            colorModelDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            colorModelDisplay.SubmittingToServerEvent += OpenSubmitWindow;
            colorModelDisplay.ColorExampleChangingEvent += sketchCanvas.DeletePoints;


            semanticModelDisplay.AddingToSelectionEvent += mFrameSelectionController.AddToSelection;
            semanticModelDisplay.RemovingFromSelectionEvent += mFrameSelectionController.RemoveFromSelection;
            semanticModelDisplay.ResettingSelectionEvent += mFrameSelectionController.ResetSelection;
            semanticModelDisplay.SelectionColorSearchEvent += mFrameSelectionController.SubmitSelectionColorModel;
            semanticModelDisplay.SelectionSemanticSearchEvent += mFrameSelectionController.SubmitSelectionSemanticModel;
            semanticModelDisplay.SubmittingToServerEvent += OpenSubmitWindow;

            mFrameSelectionController.SelectionChangedEvent +=
                (selectedFrames) =>
                {
                    resultDisplay.SelectedFrames = selectedFrames;
                    videoDisplay.SelectedFrames = selectedFrames;
                    colorModelDisplay.SelectedFrames = selectedFrames;
                    semanticModelDisplay.SelectedFrames = selectedFrames;
                };

            // show frame video on video display
            resultDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;
            videoDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;
            colorModelDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;
            semanticModelDisplay.DisplayingFrameVideoEvent += LogVideoDisplayed;

            // set first display
            mRankingEngine.GenerateSequentialRanking();

            TestButton.Click += TestButton_Click;
            TestButton.Height = 350;
            
        }

        private DataModel.Frame mSearchedFrame = null;
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            mSearchedFrame = mDataset.Frames[r.Next() % mDataset.Frames.Count];
            TestLabel.Content = mSearchedFrame.ID.ToString();
            TestButton.Content = new Image
            {
                Source = mSearchedFrame.Bitmap,
                VerticalAlignment = VerticalAlignment.Top
            };
        }

        private void ShowRank(List<RankedFrame> result)
        {
            if (mSearchedFrame != null)
            {
                TestLabel.Content = "";

                for (int i = 0; i < result.Count; i++)
                    if (result[i].Frame.FrameVideo.VideoID == mSearchedFrame.FrameVideo.VideoID)
                    {
                        TestLabel.Content = "video: " + i; break;
                    }          

                for (int i = 0; i < result.Count; i++)
                    if (result[i].Frame.ID == mSearchedFrame.ID)
                    {
                        TestLabel.Content += ", frame:" + i; return;
                    }

                TestLabel.Content += " frame filtered";
            }
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

        private void OpenSubmitWindow(DataModel.Frame frame)
        {
            SubmitWindow window = new SubmitWindow(mSubmissionClient, frame);
            window.ShowDialog();
        }


        private void SubmitToServer(DataModel.Frame frame)
        {
            mSubmissionClient.Send(frame.FrameVideo.VideoID, frame.FrameNumber);

            // logging
            string currentTask = GetCurrentTaskId();

            // log message
            string message = currentTask + ", frame submitted: "
                    + "(Frame ID:" + frame.ID
                    + ", Video:" + frame.FrameVideo.VideoID
                    + ", Number:" + frame.FrameNumber + ")";
            Logger.LogInfo(this, message);
        }
        

        private void LogVideoDisplayed(DataModel.Frame frame)
        {
            videoDisplay.DisplayFrameVideo(frame);

            // log message
            string message = "Video displayed: "
                    + "(Frame ID:" + frame.ID
                    + ", Video:" + frame.FrameVideo.VideoID
                    + ", Number:" + frame.FrameNumber + ")";
            Logger.LogInfo(this, message);
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

            filterBW.Reset();
            filterPercentageOfBlack.Reset();

            keywordSearchControlBar.Clear();
            sketchCanvasControlBar.Clear();
            semanticModelControlBar.Clear();

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
            mSubmissionClient.Send(frame.FrameVideo.VideoID, frame.FrameNumber);

            // logging
            string currentTask = GetCurrentTaskId();

            // log message
            string message = currentTask + ", frame submitted: "
                    + "(Frame ID:" + frame.ID
                    + ", Video:" + frame.FrameVideo.VideoID
                    + ", Number:" + frame.FrameNumber + ")";
            Logger.LogInfo(this, message);
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
