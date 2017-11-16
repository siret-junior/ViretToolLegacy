using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ViretTool.DataModel;

namespace VitretTool.EvaluationServer {
    class VBSTasks {
        private int mCurrentTask = -1;
        private List<VBSTask> mTasks;

        private VBSTasks() {
            mTasks = new List<VBSTask>();
        }

        public VBSTask CurrentTask {
            get {
                if (mCurrentTask < 0 || mCurrentTask >= mTasks.Count)
                    return null;
                return mTasks[mCurrentTask];
            }
        }

        public bool NextTask() {
            if (CurrentTask != null) CurrentTask.Finish();
            mCurrentTask++;

            if (CurrentTask != null) {
                CurrentTask.Load();
                return true;
            }
            return false;
        }

        public void StartTask() {
            if (CurrentTask != null) CurrentTask.Start();
        }

        public delegate void KeyframeEvaluator(long teamId, int videoId, int frameId);

        public void EvaluateKeyframe(long teamId, int videoId, int frameId) {
            if (CurrentTask != null) CurrentTask.EvaluateKeyframe(teamId, videoId, frameId);
        }

        public static VBSTasks LoadFromFile(string filename, Dataset dataset) {
            var tasks = new VBSTasks();

            string line;
            int count = 1;
            StreamReader file = new StreamReader(filename);

            while ((line = file.ReadLine()) != null) {
                if (line == string.Empty || line[0] == '#') continue;

                var t = VBSTask.LoadFromString(count++, line);
                t.RegisterEvents(tasks.mOnTaskLoaded, tasks.mOnTaskStarted,
                    tasks.mOnTaskFinished, tasks.mOnTaskTimeUpdated, tasks.mOnNewKeyframeSubmitted);
                tasks.mTasks.Add(t);
            }

            file.Close();
            return tasks;
        }

        private void mOnTaskTimeUpdated(TimeSpan time) {
            OnTaskTimeUpdated?.Invoke(time);
        }
        private void mOnTaskLoaded(int taskId, string videoSource) {
            OnTaskLoaded?.Invoke(taskId, videoSource);
        }
        private void mOnTaskStarted(int taskId) {
            OnTaskStarted?.Invoke(taskId);
        }
        private void mOnTaskFinished(int taskId) {
            OnTaskFinished?.Invoke(taskId);
        }
        private void mOnNewKeyframeSubmitted(long teamId, int videoId, int frameId, int value, int taskId) {
            OnNewKeyframeSubmitted?.Invoke(teamId, videoId, frameId, value, taskId);
        }

        public delegate void OnTaskTimeUpdatedHandler(TimeSpan time);
        public event OnTaskTimeUpdatedHandler OnTaskTimeUpdated;

        public delegate void OnTaskLoadedHandler(int taskId, string videoSource);
        public event OnTaskLoadedHandler OnTaskLoaded;

        public delegate void OnTaskStartedHandler(int taskId);
        public event OnTaskStartedHandler OnTaskStarted;

        public delegate void OnTaskFinishedHandler(int taskId);
        public event OnTaskFinishedHandler OnTaskFinished;

        public delegate void OnNewKeyframeSubmittedHandler(long teamId, int videoId, int frameId, int value, int taskId);
        public event OnNewKeyframeSubmittedHandler OnNewKeyframeSubmitted;

    }
}
