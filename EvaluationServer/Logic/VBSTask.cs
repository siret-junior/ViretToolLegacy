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
        public int StartFrame { get; private set; }
        public int EndFrame { get; private set; }
        public string Source { get; private set; }
        public TimeSpan Duration { get; private set; }
        public TimeSpan Remaining { get; private set; }

        public bool Started { get; private set; }
        public bool Finished { get; private set; }

        private Timer mTimer;
        private StreamWriter mStreamWriter;
        private Dictionary<long, Result> mSubmissions;

        private VBSTasks.OnTaskLoadedHandler OnTaskLoaded;
        private VBSTasks.OnTaskStartedHandler OnTaskStarted;
        private VBSTasks.OnTaskFinishedHandler OnTaskFinished;
        private VBSTasks.OnTaskTimeUpdatedHandler OnTaskTimeUpdated;
        private VBSTasks.OnNewKeyframeSubmittedHandler OnNewKeyframeSubmitted;

        private VBSTask() {
            mSubmissions = new Dictionary<long, Result>();
        }

        public static VBSTask LoadFromString(int id, string line) {
            var parts = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6 || parts[0] != "VIDEO") throw new Exception();

            var t = new VBSTask();
            t.VideoId = int.Parse(parts[1]);
            t.StartFrame = int.Parse(parts[2]);
            t.EndFrame = int.Parse(parts[3]);
            t.Source = parts[4];
            t.Duration = TimeSpan.FromSeconds(int.Parse(parts[5]));
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
            mStreamWriter.WriteLine("#TASK_STARTED: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            OnTaskStarted?.Invoke(TaskId);
        }

        public void Finish() {
            if (Finished) return;

            Finished = true;
            mTimer.Enabled = false;
            mStreamWriter.WriteLine("#TASK_ENDED: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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

        public void EvaluateKeyframe(long teamId, int videoId, int frameId) {
            if (!Started || Finished) return;

            Result res;

            lock (mSubmissions) {
                if (!mSubmissions.TryGetValue(teamId, out res)) {
                    res = new Result() { Value = 0, Tries = 0, Successful = false };
                    mSubmissions.Add(teamId, res);
                } else if (res.Successful) return;
            }
            
            if (videoId == VideoId && frameId <= EndFrame && frameId >= StartFrame) {
                res.Value = (int)Math.Max(0, (50 + 50 * (1 - Remaining.TotalSeconds / Duration.TotalSeconds) - res.Tries * 10));
                res.Successful = true;
            }

            res.Tries++;

            lock (mSubmissions) {
                mSubmissions[teamId] = res;
                mStreamWriter.WriteLine("{0}\t{1,20}\t{2,10}\t{3,10}\t{4,5}\t{5}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), teamId, videoId, frameId, res.Value, res.Successful ? "true" : "false");
            }

            OnNewKeyframeSubmitted?.Invoke(teamId, videoId, frameId, res.Value, TaskId, res.Successful);
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
                        int videoId = int.Parse(parts[2]);
                        int frameId = int.Parse(parts[3]);
                        int val = int.Parse(parts[4]);
                        bool success = parts[5] == "true";

                        if (mSubmissions.ContainsKey(teamId)) {
                            var res = mSubmissions[teamId];
                            res.Tries++;
                            res.Value = val;
                            res.Successful = success;
                            mSubmissions[teamId] = res;
                        } else {
                            mSubmissions.Add(teamId, new Result() { Value = val, Tries = 1, Successful = success });
                        }

                        OnNewKeyframeSubmitted?.Invoke(teamId, videoId, frameId, val, TaskId, success);
                    }
                }
            } catch (IOException) { }

            OnTaskFinished?.Invoke(TaskId);
        }

        struct Result {
            public int Value;
            public int Tries;
            public bool Successful;
        }
    }
}
