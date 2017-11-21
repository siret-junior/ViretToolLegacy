using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvaluationServer.Support {
    class ConvertFrameIdsToTimestamps {

        public static void Convert(ViretTool.DataModel.Dataset dataset, string timingFile, string taskFile) {
            using(var read = new StreamReader(timingFile)) {
                using (var write = new StreamWriter(taskFile)) {
                    write.WriteLine("#TYPE\t\tCORRECT VID ID\t\tSTART TIMESTAMP\t\tEND TIMESTAMP\t\tSOURCE\t\t\t\t\tTIME TO SOLVE (s)");

                    string line;
                    while((line = read.ReadLine()) != null) {
                        if (line[0] == '#') continue;

                        var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        int videoId = int.Parse(parts[0]);

                        int start = 0;
                        if (parts[1] != "t0") {
                            start = int.Parse(parts[1]);
                            var f = dataset.Frames[start];
                            start = dataset.Frames[start].FrameNumber;
                        }
                        int end = int.Parse(parts[2]);
                        end = dataset.Frames[end].FrameNumber;

                        write.WriteLine("VIDEO\t\t{0}\t\t\t\t\t{1}\t\t\t\t\t{2}\t\t\t\t\t10s/10s-{3}.mp4\t\t300", videoId, start, end, videoId.ToString("D5"));
                    }
                }
            }
        }

    }
}
