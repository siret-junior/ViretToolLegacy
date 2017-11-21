using EvaluationServer.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using VitretTool.EvaluationServer.Controls;

namespace VitretTool.EvaluationServer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        VBSTasks tasks;
        Teams teams;
        UIDrawer drawer;

        public MainWindow() {
//#error Read this, then comment out.
//#warning 'tasks.txt', 'Teams' and 'Tasks' folder must exist in a location of the exe file.
//#warning In those folders, there are saved runs - when present, program will just show results.
//#warning Move to next task by pressing key N; to start task, press P (Works only if a task run is not saved.)
//#warning Make sure, the location of dataset files a few lines below is correct.
//#warning Be sure FrameIO.dll of the main VitretTool is reachable.
//#warning User submissions are cropped only when shown from saved runs, normally only last few are displayed.

            InitializeComponent();

            //var dataset = new ViretTool.DataModel.Dataset(
            //    "..\\..\\..\\TestData\\ITEC\\ITEC-KF3sec-100x75.thumb",
            //    "..\\..\\..\\TestData\\ITEC\\ITEC-4fps-100x75.thumb");
            var dataset = new ViretTool.DataModel.Dataset(
                "..\\..\\..\\TestData\\TRECVid\\TRECVid-KF-100x75.thumb",
                "..\\..\\..\\TestData\\TRECVid\\TRECVid-4fps-100x75.thumb");

            //ConvertFrameIdsToTimestamps.Convert(dataset, "E:\\TRECVidselected\\10s\\task_timing.for_generation_only", "E:\\TRECVidselected\\10s\\tasks.txt");

            int port = 9999;
            IPAddress ip = GetIPAddress();

            drawer = new UIDrawer(
                (Grid)FindName("InfoGrid"),
                (Grid)FindName("PresentationGrid"),
                (TextBlock)FindName("TimeInfo"),
                (TextBlock)FindName("TaskInfo"),
                (MediaElement)FindName("MediaElement"),
                (Grid)FindName("MediaElementFullscreen"),
                (Chart)FindName("TeamsChart"),
                (Grid)FindName("TeamsGrid"),
                (Grid)FindName("ContentBox"),
                dataset
                );
            drawer.ShowStartScreen(ip.ToString(), port);

            tasks = VBSTasks.LoadFromFile("tasks.txt", dataset);
            tasks.OnNewKeyframeSubmitted += drawer.DrawNewKeyframe;
            tasks.OnTaskLoaded += drawer.LoadTask;
            tasks.OnTaskStarted += drawer.StartTask;
            tasks.OnTaskFinished += drawer.FinishTask;
            tasks.OnTaskTimeUpdated += drawer.UpdateTime;

            teams = new Teams(tasks.EvaluateKeyframe, drawer.DrawNewTeam);
            
            KeyUp += OnKeyPressed;
            
            Server server = new Server(ip, port, teams, tasks);
            Task.Run(action: server.Listen);
        }

        public void OnKeyPressed(object sender, KeyEventArgs e) {
            if (e.Key == Key.N) {
                if (!tasks.NextTask()) {
                    drawer.ShowEndScreen();
                }
            } else if (e.Key == Key.P) {
                tasks.StartTask();
            }
        }

        private static IPAddress GetIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip;
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
