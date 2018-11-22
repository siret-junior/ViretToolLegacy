using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.InteractionLogging.DataObjects;
using ViretTool.InteractionLogging.JsonSerialization;

namespace ViretTool.InteractionLogging
{
    public class InteractionLogger
    {
        public static InteractionLogger Instance { get { return _lazy.Value; } }

        private static readonly Lazy<InteractionLogger> _lazy =
            new Lazy<InteractionLogger>(() => new InteractionLogger());

        private const string LOG_FOLDER = "InteractionLog";
        private static object _lockObject = new object();
        private Log _log = new Log();
        private const int TIME_DELAY_MILISECONDS = 1 * 1000;

        private InteractionLogger()
        {
        }

        public void SetTeamName(string teamName)
        {
            lock (_lockObject)
            {
                _log.TeamName = teamName;
            }
        }
        public void SetMemberId(int memberId)
        {
            lock (_lockObject)
            {
                _log.MemberId = memberId;
            }
        }
        

        public void LogInteraction(string category, string type = null, string value = null, string attributes = null)
        {
            // TODO: considering events with only a single action for now
            Event interactionEvent = new Event();
            DataObjects.Action action = new DataObjects.Action(category, type, value, attributes);
            interactionEvent.Actions.Add(action);

            lock (_lockObject)
            {
                if (_log.Events.Count > 0)
                {
                    long lastEventTime = _log.Events[_log.Events.Count - 1].Timestamp;
                    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (Math.Abs(lastEventTime - currentTime) < TIME_DELAY_MILISECONDS)
                    {
                        return;
                    }
                }

                _log.Events.Add(interactionEvent);
            }
        }

        internal void SubmitLog()
        {
            LogInteraction("post", "submit");
            SaveLogFile();
        }

        internal void ResetLog()
        {
            SaveLogFile();
            lock (_lockObject)
            {
                _log.Events.Clear();
            }
        }


        private void SaveLogFile()
        {
            lock (_lockObject)
            {
                using (StreamWriter writer = new StreamWriter(GenerateFilename()))
                {
                    writer.Write(LowercaseJsonSerializer.SerializeObject(_log));
                }
                _log.Events.Clear();
            }
        }

        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
        }

        private static string GenerateFilename()
        {
            string filename = "ViretTool-InteractionLog_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            Directory.CreateDirectory(LOG_FOLDER);
            string relativePath = Path.Combine(LOG_FOLDER, filename);
            return relativePath;
        }

    }
}
