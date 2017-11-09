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
        DataModel.Dataset mDataset;
        RankingEngine mRankingEngine;
        
        public MainWindow()
        {
            InitializeComponent();

            // prepare data model
            mDataset = new DataModel.Dataset(
                "..\\..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb",
                "..\\..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb");
            //mDataset = new DataModel.Dataset(
            //    "..\\..\\..\\..\\TestData\\TRECVid\\TRECVid-KF-100x75.thumb",
            //    "..\\..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb");

            // prepare ranking engine
            SimilarityManager similarityManager = new SimilarityManager(mDataset);
            FilterManager filterManager = new FilterManager(mDataset);
            mRankingEngine = new RankingEngine(similarityManager, filterManager);
            mRankingEngine.VideoAggregateFilterEnabled = true;
            mRankingEngine.VideoAggregateFilterMaxFrames = 2;

            // initialize selection controller
            FrameSelectionController frameSelectionController
                = new FrameSelectionController(mRankingEngine, resultDisplay, videoDisplay, semanticModelDisplay);

            // initialize videoDisplay
            ((System.ComponentModel.ISupportInitialize)(videoDisplay)).BeginInit();
            videoDisplay.FrameSelectionController = frameSelectionController;
            ((System.ComponentModel.ISupportInitialize)(videoDisplay)).EndInit();

            // initialize resultDisplay
            ((System.ComponentModel.ISupportInitialize)(resultDisplay)).BeginInit();
            resultDisplay.Dataset = mDataset;
            resultDisplay.RankingEngine = mRankingEngine;
            resultDisplay.FrameSelectionController = frameSelectionController;
            resultDisplay.VideoDisplay = videoDisplay;
            ((System.ComponentModel.ISupportInitialize)(resultDisplay)).EndInit();

            // initialize semanticModelDisplay
            ((System.ComponentModel.ISupportInitialize)(semanticModelDisplay)).BeginInit();
            semanticModelDisplay.FrameSelectionController = frameSelectionController;
            semanticModelDisplay.VideoDisplay = videoDisplay;
            ((System.ComponentModel.ISupportInitialize)(semanticModelDisplay)).EndInit();


            keywordSearchTextBox.Init(mDataset, new string[] {
                "GoogLeNet", "YFCC100M"
            });

            keywordSearchTextBox.KeywordChangedEvent += Keyword;
            // TODO: debug
            sketchCanvas.SketchChangedEvent += Sketch;

            List<RankedFrame> debugRankedFrames = new List<RankedFrame>();
            for (int i = 0; i < 500; i++)
            {
                debugRankedFrames.Add(new RankedFrame(mDataset.Frames[i], 0));
            }
            resultDisplay.ResultFrames = debugRankedFrames;
        }

        private void Keyword(List<List<int>> query, string annotationSource) {
            List<RankedFrame> result = mRankingEngine.UpdateKeywordModelRanking(query, annotationSource);
            resultDisplay.ResultFrames = result;
        }

        private void Sketch(List<Tuple<Point, Color>> colorSketch)
        {
            List<RankedFrame> result = mRankingEngine.UpdateColorModelRanking(colorSketch);
            resultDisplay.ResultFrames = result;
        }
    }
}
