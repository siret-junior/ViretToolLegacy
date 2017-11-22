using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluationServer.Support {
    class ConvertFrameIdsToTimestamps {

        public static void Convert(ViretTool.DataModel.Dataset dataset, string fpsFile, string timingFile, string taskFile) {
            var dict = new Dictionary<int, double>();

            using (var fps = new StreamReader(fpsFile)) {
                string line;
                while ((line = fps.ReadLine()) != null) {
                    if (line[0] == '#') continue;

                    var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int videoId = int.Parse(parts[0]) - 35345;

                    var ints = parts[1].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    double cit = double.Parse(ints[0]);
                    double jmen = double.Parse(ints[1]);
                    double fps_ = cit / jmen;
                    dict.Add(videoId, fps_);
                }
            }

            using (var read = new StreamReader(timingFile)) {
                using (var write = new StreamWriter(taskFile)) {
                    write.WriteLine("#TYPE\t\tCORRECT VID ID\t\tSTART TIMESTAMP\t\tEND TIMESTAMP\t\tSOURCE\t\t\t\t\tTIME TO SOLVE (s)");

                    string line;
                    while((line = read.ReadLine()) != null) {
                        if (line[0] == '#') continue;

                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int videoId = int.Parse(parts[4].Substring(0, parts[4].Length - 4));

                        double startInSecs = 0;
                        var time = parts[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        double secs = double.Parse(time[2]);
                        int mins = int.Parse(time[1]);
                        int hours = int.Parse(time[0]);

                        startInSecs = hours * 3600 + mins * 60 + secs;

                        int start = (int)((startInSecs - 10) * dict[videoId]);
                        if (start < 0) start = 0;

                        int end = (int)((startInSecs + 20) * dict[videoId]);

                        write.WriteLine("VIDEO\t\t{0}\t\t\t\t\t{1}\t\t\t\t\t{2}\t\t\t\t\t10s/10s-{3}.wmv\t\t300", videoId, start, end, videoId.ToString("D5"));
                    }
                }
            }
        }

    }
}
