using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ViretTool.DataModel;

namespace VitretTool.EvaluationServer {
    class VBSTask {
        public int TaskId { get; private set; }
        public int VideoId { get; private set; }
        public string Source { get; private set; }
        public TimeSpan Duration { get; private set; }
        public TimeSpan Remaining { get; private set; }

        public bool Started { get; private set; }
        public bool Finished { get; private set; }

        private Timer mTimer;
        private Dataset mDataset;
        private StreamWriter mStreamWriter;
        private Dictionary<long, int> mSubmissions;

        private VBSTasks.OnTaskLoadedHandler OnTaskLoaded;
        private VBSTasks.OnTaskStartedHandler OnTaskStarted;
        private VBSTasks.OnTaskFinishedHandler OnTaskFinished;
        private VBSTasks.OnTaskTimeUpdatedHandler OnTaskTimeUpdated;
        private VBSTasks.OnNewKeyframeSubmittedHandler OnNewKeyframeSubmitted;

        private VBSTask() {
            mSubmissions = new Dictionary<long, int>();
        }

        public static VBSTask LoadFromString(int id, string line, Dataset dataset) {
            var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 || parts[0] != "VIDEO") throw new Exception();

            var t = new VBSTask();
            t.VideoId = int.Parse(parts[1]);
            t.Source = parts[2];
            t.Duration = TimeSpan.FromSeconds(int.Parse(parts[3]));
            t.mDataset = dataset;
            t.TaskId = id;

            t.Started = false;
            t.Finished = false;

            if (File.Exists("Tasks/finished" + t.TaskId + ".txt")) {
                t.Started = true;
                t.Finished = true;
            } else {
                t.mStreamWriter = new StreamWriter("Tasks/finished" + t.TaskId + ".txt", false);
                t.mStreamWriter.AutoFlush = true;
            }

            return t;
        }

        public void RegisterEvents(
            VBSTasks.OnTaskLoadedHandler l,
            VBSTasks.OnTaskStartedHandler s,
            VBSTasks.OnTaskFinishedHandler f,
            VBSTasks.OnTaskTimeUpdatedHandler t,
            VBSTasks.OnNewKeyframeSubmittedHandler k) {

            OnTaskLoaded = l;
            OnTaskStarted = s;
            OnTaskFinished = f;
            OnTaskTimeUpdated = t;
            OnNewKeyframeSubmitted = k;
        }

        public void Load() {
            if (Started && Finished) {
                RestoreResults();
                return;
            }
            if (Started) return;

            Remaining = Duration;
            mTimer = new Timer(1000);
            mTimer.Elapsed += OnTimerElapsed;

            OnTaskLoaded?.Invoke(TaskId, Source);
            OnTaskTimeUpdated?.Invoke(Duration);
        }

        public void Start() {
            if (Started == true) return;
            Started = true;
            mTimer.Enabled = true;

            OnTaskStarted?.Invoke(TaskId);
        }

        public void Finish() {
            if (Finished) return;

            Finished = true;
            mTimer.Enabled = false;

            mStreamWriter.Close();
            OnTaskFinished?.Invoke(TaskId);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e) {
            Remaining -= TimeSpan.FromSeconds(1);
            if (Remaining == TimeSpan.Zero) {
                Finish();
            }
            OnTaskTimeUpdated?.Invoke(Remaining);
        }

        public void EvaluateKeyframe(long teamId, int frameId) {
            if (!Started || Finished) return;

            int val = 0;
            if (frameId < mDataset.Frames.Count && frameId >= 0) {

                lock (mSubmissions) {
                    if (mSubmissions.ContainsKey(teamId)) return;
                }

                if (mDataset.Frames[frameId].FrameVideo.VideoID == VideoId) {
                    val = (int)Remaining.TotalSeconds;

                    lock (mSubmissions) {
                        mSubmissions.Add(teamId, val);
                    }
                }
                OnNewKeyframeSubmitted?.Invoke(teamId, frameId, val, TaskId);

                lock (mStreamWriter) {
                    mStreamWriter.WriteLine("{0}\t{1,20}\t{2,10}\t{3,5}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), teamId, frameId, val);
                }
            }
        }

        private void RestoreResults() {
            OnTaskLoaded?.Invoke(TaskId, Source);
            OnTaskTimeUpdated?.Invoke(TimeSpan.Zero);

            try {
                using (var stream = new StreamReader("Tasks/finished" + TaskId + ".txt")) {
                    string line;

                    while ((line = stream.ReadLine()) != null) {
                        if (line == string.Empty || line[0] == '#') continue;

                        var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        long teamId = long.Parse(parts[1]);
                        int frameId = int.Parse(parts[2]);
                        int val = int.Parse(parts[3]);

                        if (val > 0) {
                            mSubmissions.Add(teamId, val);
                        }
                        OnNewKeyframeSubmitted?.Invoke(teamId, frameId, val, TaskId);
                    }
                }
            } catch (IOException) { }

            OnTaskFinished?.Invoke(TaskId);
        }
    }
}
