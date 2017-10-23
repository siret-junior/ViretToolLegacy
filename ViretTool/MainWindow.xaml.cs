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

using ViretTool.RankingModels;

namespace ViretTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dataset = new DataModel.Dataset(
                "..\\..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb",
                "..\\..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb");

            var engine = new RankingEngine(dataset);

            engine.InitKeywordModel(
                (BasicClient.Controls.SuggestionTextBox)FindName("SuggestionTextBox"),
                new string[] {
                    "..\\..\\..\\..\\TestData\\ITEC\\GoogLeNet",
                    "..\\..\\..\\..\\TestData\\ITEC\\YFCC100M"
                });

            engine.BuildEngine((BasicClient.Controls.ModelSelector)FindName("ModelSelector"));
        }
    }
}
