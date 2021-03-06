using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class ResultDisplay : DisplayControl, INotifyPropertyChanged
    {
        const double DESIRED_ASPECT_RATIO = 3.0 / 4.0;

        int nColumns = 10;
        const int SMALL_DISPLAY_COLUMNS = 10;
        const int LARGE_DISPLAY_COLUMNS = 16;

        bool mSortDisplay = false;

        // TODO: add context frames

        private List<RankedFrame> mResultFrames = null;
        public List<RankedFrame> ResultFrames
        {
            get
            { return mResultFrames; }
            set
            {
                int itemCount = value != null ? value.Count : 0;
                string message = "Result display received " + itemCount + " items.";
                Logger.LogInfo(this, message);
                if (value == null || value.Count == 0)
                {
                    return;
                }
                mResultFrames = value;
                DisplayPage(0);
                UpdateSelectionVisualization();
            }
        }

        
        private int mPage = 0;
        public string PageNumberLabel
        {
            get
            { return mPage.ToString(); }
            set
            {
                NotifyPropertyChanged("PageNumberLabel");
            }
        }
        

        public ResultDisplay()
        {
            InitializeComponent();
            DataContext = this;
            ResizeDisplay(1, 1, displayGrid);
        }


        #region --[ Display control ]--

        public void UpdateDisplayGrid()
        {
            FitDisplayToGridDimensions();
            UpdateSelectionVisualization();
        }

        private void FitDisplayToGridDimensions()
        {
            // TODO: custom nColumns from the GUI
            double desiredFrameHeight = displayGrid.ActualWidth / nColumns * DESIRED_ASPECT_RATIO;
            int nRows = (int)((displayGrid.ActualHeight + desiredFrameHeight / 2) / desiredFrameHeight);

            if (nRows != mDisplayRows || nColumns != mDisplayCols)
            {
                ResizeDisplay(nRows, nColumns, displayGrid);

                // TODO: recompute correct page
                DisplayPage(0);
            }

            string message = "Result display was resized to " + nColumns * nRows + "items (" 
                + nColumns + " columns, " + nRows + " rows).";
            Logger.LogInfo(this, message);
        }
        
        private void ClearDisplay()
        {
            for (int i = 0; i < DisplayedFrames.Length; i++)
            {
                DisplayedFrames[i].Frame = null;
            }

            string message = "Result display was cleared.";
            Logger.LogInfo(this, message);
        }

        public void DisplayPage(int page)
        {
            // display check
            int displaySize = DisplayedFrames.Length;
            if (displaySize == 0)
            {
                return;
            }

            ClearDisplay();

            // result check
            if (mResultFrames == null || mResultFrames.Count == 0)
            {
                return;
            }

            // range check 0..maxPage
            if (page * displaySize >= mResultFrames.Count)
            {
                mPage = (mResultFrames.Count - 1) / displaySize;
            }
            else if (page < 0)
            {
                mPage = 0;
            }
            else
            {
                mPage = page;
            }

            // update page label
            // TODO
            PageNumberLabel = "Page: " + (mPage + 1).ToString();

            // extract frame subset
            int offset = mPage * displaySize;
            int count = (mResultFrames.Count - offset < displaySize) ? mResultFrames.Count - offset : displaySize;
            count = (count > 0) ? count : 0;
            List<RankedFrame> framesToDisplay = mResultFrames.GetRange(offset, count);

            // TODO: semantic/color sorting of displayed items on a page
            DisplayArrangement arrangementType;
            if (mSortDisplay)
            {
                arrangementType = DisplayArrangement.Semantic;
            }
            else
            {
                arrangementType = DisplayArrangement.Ranking;
            }

            RankedFrame[,] arrangedDisplay 
                = DisplayArranger.ArrangeDisplay(framesToDisplay, mDisplayRows, mDisplayCols, arrangementType);

            // display frames
            int iterator = 0;
            string message = "Result display displayed page " + mPage + ", "
                + framesToDisplay.Count + " items: ";
            for (int iRow = 0; iRow < mDisplayRows; iRow++)
            {
                for (int iCol = 0; iCol < mDisplayCols; iCol++)
                {
                    if (arrangedDisplay[iRow, iCol] == null) continue; // TODO rewrite
                    DisplayedFrames[iterator++].Frame = arrangedDisplay[iRow, iCol].Frame;

                    message += "(Frame ID:" + arrangedDisplay[iRow, iCol].Frame.ID
                            + ", Video:" + arrangedDisplay[iRow, iCol].Frame.FrameVideo.VideoID
                            + ", Number:" + arrangedDisplay[iRow, iCol].Frame.FrameNumber + "), ";

                    if (iterator > framesToDisplay.Count)
                    {
                        break;
                    }
                }
            }

            
            Logger.LogInfo(this, message);

            UpdateSelectionVisualization();
            RaiseDisplayChangedEvent();
        }

        public void IncrementDisplay(int nPages)
        {
            DisplayPage(mPage + nPages);
        }

        #endregion


        #region --[ Events ]--

        public delegate void DisplayEventHandler();
        public event DisplayEventHandler DisplayRandomItemsRequestedEvent;
        public event DisplayEventHandler DisplaySequentialItemsRequestedEvent;
        public event DisplayEventHandler DisplayChangedEvent;

        public event DisplayEventHandler ShowFilteredVideosEnabledEvent;
        public event DisplayEventHandler ShowFilteredVideosDisabledEvent;
        public event DisplayEventHandler FilterSelectedVideoEvent;

        public event DisplayEventHandler Max3FromVideoEnabledEvent;
        public event DisplayEventHandler Max3FromVideoDisabledEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void RaiseDisplayRandomItemsEvent()
        {
            DisplayRandomItemsRequestedEvent?.Invoke();
        }

        private void RaiseDisplaySequentialItemsEvent()
        {
            DisplaySequentialItemsRequestedEvent?.Invoke();
        }

        private void RaiseDisplayChangedEvent()
        {
            DisplayChangedEvent?.Invoke();
        }

        #endregion


        #region --[ GUI interaction ]--

        private void displayGrid_Loaded(object sender, RoutedEventArgs e)
        {
            FitDisplayToGridDimensions();
            UpdateSelectionVisualization();
        }

        private void sequentialDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseDisplaySequentialItemsEvent();
        }

        private void randomDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseDisplayRandomItemsEvent();
        }

        private void firstPageButton_Click(object sender, RoutedEventArgs e)
        {
            VBSLogger.AppendActionIncludeTimeParameter('P', true);
            DisplayPage(0);
        }

        private void previousPageButton_Click(object sender, RoutedEventArgs e)
        {
            VBSLogger.AppendActionIncludeTimeParameter('P', true);
            DisplayPage(mPage - 1);
        }

        private void nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            VBSLogger.AppendActionIncludeTimeParameter('P', true);
            DisplayPage(mPage + 1);
        }

        private void lastPageButton_Click(object sender, RoutedEventArgs e)
        {
            VBSLogger.AppendActionIncludeTimeParameter('P', true);
            DisplayPage(mResultFrames.Count / DisplayedFrames.Length);
        }

        
        private void sortDisplayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            mSortDisplay = true;
        }

        private void sortDisplayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            mSortDisplay = false;
        }

        private void largeDisplayCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            VBSLogger.AppendActionIncludeTimeParameter('B', true);
            nColumns = LARGE_DISPLAY_COLUMNS;
            FitDisplayToGridDimensions();
        }

        private void largeDisplayCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            VBSLogger.AppendActionIncludeTimeParameter('B', true);
            nColumns = SMALL_DISPLAY_COLUMNS;
            FitDisplayToGridDimensions();
        }

        private void showFilteredVideosCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ShowFilteredVideosDisabledEvent?.Invoke();
        }

        private void showFilteredVideosCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowFilteredVideosEnabledEvent?.Invoke();
        }

        private void filterVideoButton_Click(object sender, RoutedEventArgs e)
        {
            FilterSelectedVideoEvent?.Invoke();
        }

        private void Max3FromVideoCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Max3FromVideoEnabledEvent?.Invoke();
        }

        private void Max3FromVideoCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Max3FromVideoDisabledEvent?.Invoke();
        }

        //private void displayGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (e.Delta > 0)
        //    {
        //        DisplayPage(mPage - 1);
        //    }
        //    else
        //    {
        //        DisplayPage(mPage + 1);
        //    }
        //}

        #endregion

        //private void displayGrid_KeyDown(object sender, KeyEventArgs e)
        //{
        //    switch (e.Key)
        //    {
        //        case Key.Left:
        //            DisplayPage(mPage - 1);
        //            break;
        //        case Key.Right:
        //            DisplayPage(mPage + 1);
        //            break;
        //    }
        //}
    }
}
