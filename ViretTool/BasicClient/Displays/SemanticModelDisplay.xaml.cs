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
using ViretTool.RankingModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for SemanticModelDisplay.xaml
    /// </summary>
    public partial class SemanticModelDisplay : DisplayControl
    {
        private int mColRatio = 3;
        private int mRowRatio = 2;

        
        public SemanticModelDisplay()
        {
            InitializeComponent();
            FitDisplay(1);
        }


        public void DisplayFrames(List<DataModel.Frame> selectedFrames)
        {
            // TODO move to separate method
            // clear display
            for (int i = 0; i < DisplayedFrames.Length; i++)
            {
                DisplayedFrames[i].Clear();
            }

            // skip if nothing to show
            if (selectedFrames == null)
            {
                return;
            }

            FitDisplay(selectedFrames.Count);

            // display frames
            for (int i = 0; i < selectedFrames.Count; i++)
            {
                DisplayedFrames[i].Set(selectedFrames[i]);
            }
        }

        
        private void FitDisplay(int frameCount)
        {
            int cols = mColRatio;
            int rows = mRowRatio;

            // scale down
            while (cols - 1 >= mColRatio && rows - 1 >= mRowRatio
                && (cols - 1) * (rows - 1) >= frameCount)
            {
                cols--;
                rows--;
            }

            // scale up
            while ((cols) * (rows) < frameCount)
            {
                cols++;
                rows++;
            }

            // resize if needed
            ResizeDisplay(rows, cols, displayGrid);
        }
        

        private void semanticClearButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseResettingSelectionEvent();
            RaiseSelectionSemanticSearchEvent();
        }


        //// TODO use bindings and a custom converter
        //private void colRatioTextbox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    int number;
        //    if (int.TryParse(colRatioTextbox.Text, out number) && number < 32)
        //    {
        //        mColRatio = number;
        //    }
        //    else
        //    {
        //        mColRatio = 1;
        //        colRatioTextbox.Text = "";
        //    }
            
        //}

        //private void rowRatioTextbox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    int number;
        //    if (int.TryParse(rowRatioTextbox.Text, out number) && number < 32)
        //    {
        //        mRowRatio = number;
        //    }
        //    else
        //    {
        //        mRowRatio = 1;
        //        rowRatioTextbox.Text = "";
        //    }
        //}
    }
}
