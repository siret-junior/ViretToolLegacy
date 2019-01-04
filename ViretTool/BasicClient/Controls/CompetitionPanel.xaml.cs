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
using ViretTool.DataModel;

namespace ViretTool.BasicClient
{
    /// <summary>
    /// Interaction logic for CompetitionPanel.xaml
    /// </summary>
    public partial class CompetitionPanel : UserControl
    {
        public RandomScenePlayer Practice { get; set; }
        public CompetitionScenePlayer Competition { get; set; }

        public Dataset Dataset { get; set; }


        public CompetitionPanel()
        {
            InitializeComponent();
        }
        

        private void practiceButton_Click(object sender, RoutedEventArgs e)
        {
            Competition.Reset();
            Competition = null;
            Practice = new RandomScenePlayer(Dataset, playButton, 200);
        }

        private void competitionButton_Click(object sender, RoutedEventArgs e)
        {
            Practice.Reset();
            Practice = null;
            Competition = new CompetitionScenePlayer(Dataset, playButton, 200);
        }
    }


}
