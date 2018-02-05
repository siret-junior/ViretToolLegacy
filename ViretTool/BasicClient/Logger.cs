using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BasicClient
{
    public enum Severity { Fatal, Error, Warn, Info, Debug, Trace }

    public class Logger : IDisposable
    {
        private const string LOG_FOLDER = "Log";

        private static readonly Logger Instance = new Logger();
        private static object logLock = new object();
        StreamWriter mLogWriter;


        public Logger(string filename)
        {
            mLogWriter = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read));
            mLogWriter.WriteLine("####  Log started at "
                + GetCurrentTime() + "  #################################################");
        }

        public Logger() : this(GenerateFilename())
        {
        }


        public static void Log(object sender, Severity severity, string message)
        {
            // build the log line
            string timestamp = GetCurrentTime();
            string severty = GetSeverityString(severity);
            string source = sender.GetType().Name;
            string line = severty + timestamp + " (" + source + "): " + message;

            // write and flush the message
            lock (logLock)
            {
                Instance.mLogWriter.WriteLine(line);
                Instance.mLogWriter.Flush();
            }
        }

        public static void LogInfo(object sender, string message)
        {
            Log(sender, Severity.Info, message);
        }

        private static string GenerateFilename()
        {
            string filename = "ViretTool_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            Directory.CreateDirectory(LOG_FOLDER);
            string relativePath = Path.Combine(LOG_FOLDER, filename);
            return relativePath;
        }

        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
        }

        private static string GetSeverityString(Severity severity)
        {
            switch (severity)
            {
                case Severity.Fatal:
                    return "FATAL: ";
                case Severity.Error:
                    return "ERROR: ";
                case Severity.Warn:
                    return "WARN:  ";
                case Severity.Info:
                    return "INFO:  ";
                case Severity.Debug:
                    return "DEBUG: ";
                case Severity.Trace:
                    return "TRACE: ";
                default:
                    throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            ((IDisposable)mLogWriter).Dispose();
        }
    }

    public class VBSLogger
    {
        private static readonly VBSLogger Instance = new VBSLogger("0");
        private static object logLock = new object();

        private char[] mDefinedActions;
        private StringBuilder mLog;
        private DateTime mLogCreationTime;
        private string mToolID;

        static VBSLogger()
        {
            Instance = new VBSLogger("0");
        }


        public VBSLogger(string toolID)
        {
            mDefinedActions = "KAOCEMSFPBTX".ToCharArray();
            mToolID = toolID;
            mLog = new StringBuilder();
            mLog.Append(mToolID);
            mLogCreationTime = DateTime.Now;
        }

        public static void ResetLog()
        {
            Instance.mLog = new StringBuilder();
            Instance.mLog.Append(Instance.mToolID);
            Instance.mLogCreationTime = DateTime.Now;
        }

        public static string AppendTimeAndGetLogString()
        {
            Instance.mLog.Append(";time " + DateTime.Now.ToString("H:mm:ss"));
            return Instance.mLog.ToString();
        }

        public static void AppendActionIncludeTimeParameter(char action, bool add, string parameters = "")
        {
            action = char.ToUpper(action);
            if (!Instance.mDefinedActions.Contains(action))
                throw new Exception("Unknown VBSLog action " + action);

            if (add) Instance.mLog.Append(";" + action);
            else Instance.mLog.Append(";-" + action);

            if (parameters != "") parameters = "," + parameters;
            parameters = "(" + DateTime.Now.Subtract(Instance.mLogCreationTime).TotalSeconds.ToString("0") + "s" + parameters + ")";

            Instance.mLog.Append(parameters);
        }
    }
}
