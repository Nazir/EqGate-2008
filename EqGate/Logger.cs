using System;
using System.IO;

namespace Logger
{
    public partial class Log
    {
        //private bool AutoStart = false;
        public static string LogPath = @"e:\EXCHANGE\";
        public static string LogFileExt = ".log";
        public static string LogName = "log";
        public static string LogFile = String.Empty;
        public static string AppName = "";

        public static void SaveLog(string ALogName, string ALogText, string ALogStatus)
        {
            if (LogFile == String.Empty)
                LogFile = LogPath + LogName + LogFileExt;
            string LogTime = Convert.ToString(DateTime.Now);
            if (true)
            {
                Utils.Util.SaveTextToFile(LogTime + " " + ALogName + " " + ALogStatus + " " + AppName + " " + ALogText + "\r\n", LogFile, true);
            }
        }
    }
 }
