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
        
        // TODO: result changed event and log
        // TODO: display changed method -> update visualization

        private List<RankedFrame> mResultFrames = null;
        public List<RankedFrame> ResultFrames
        {
            get
            { return mResultFrames; }
            set
            {
                if (value == null || value.Count == 0)
                {
                    return;
                }
                mResultFrames = value;
                DisplayPage(0);
                UpdateSelectionVisualization();
            }
        }

        public delegate void DisplayRandomItemsEventHandler();
        public event DisplayRandomItemsEventHandler DisplayRandomItemsEvent;
        public event DisplayRandomItemsEventHandler DisplaySequentialItemsEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


        private void RecomputeDisplaySize()
        {
            // TODO: custom nColumns from the GUI
            int nColumns = 8;
            double desiredFrameHeight = displayGrid.ActualWidth / nColumns * DESIRED_ASPECT_RATIO;
            int nRows = (int)((displayGrid.ActualHeight + desiredFrameHeight / 2) / desiredFrameHeight);

            ResizeDisplay(nRows, nColumns, displayGrid);

            // TODO: recompute correct page
            DisplayPage(0);
        }

        



        private void EmptyDisplay()
        {
            for (int i = 0; i < DisplayedFrames.Length; i++)
            {
                DisplayedFrames[i].Frame = null;
            }
        }

        private void DisplayPage(int page)
        {
            // display check
            int displaySize = DisplayedFrames.Length;
            if (displaySize == 0)
            {
                return;
            }

            EmptyDisplay();

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
            PageNumberLabel = "Page: " + (mPage + 1).ToString();

            // extract frame subset
            int offset = mPage * displaySize;
            int count = (mResultFrames.Count - offset < displaySize) ? mResultFrames.Count - offset : displaySize;
            count = (count > 0) ? count : 0;
            List <RankedFrame> framesToDisplay = mResultFrames.GetRange(offset, count);
            
            // TODO: semantic/color sorting of displayed items on a page

            // display frames
            for (int i = 0; i < count; i++)
            {
                DisplayedFrames[i].Frame = framesToDisplay[i].Frame;
            }
        }

        private void RaiseDisplayRandomItems()
        {
            DisplayRandomItemsEvent?.Invoke();
        }

        private void RaiseDisplaySequentialItems()
        {
            DisplaySequentialItemsEvent?.Invoke();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mPage++;
            DisplayPage(mPage);
        }

        private void displayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecomputeDisplaySize();
            UpdateSelectionVisualization();
        }

        private void sequentialDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseDisplaySequentialItems();
        }

        private void randomDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseDisplayRandomItems();
        }

        private void firstPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(0);
        }

        private void previousPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(mPage - 1);
        }

        private void nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(mPage + 1);
        }

        private void lastPageButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(mResultFrames.Count / DisplayedFrames.Length);
        }

        private void displayGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                DisplayPage(mPage - 1);
            }
            else
            {
                DisplayPage(mPage + 1);
            }
        }
    }
}
