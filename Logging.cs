using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenGrabberTI
{
    public enum LogTarget
    {
        File
    }

    public abstract class LogBase
    {
        protected readonly object lockObj = new object();
        public abstract void Log(string message);
    }

    public class FileLogger : LogBase
    {
        public static string appdata = Environment.ExpandEnvironmentVariables("%APPDATA%");        
        public string filePath = appdata + @"\ScreenGrabber\log.txt";
        public override void Log(string message)
        {
            lock (lockObj)
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, true))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString() + " | " + message);
                    streamWriter.Close();
                }
            }
        }
    }

    public static class LogHelper
    {
        private static LogBase logger = null;
        public static void Log(LogTarget target, string message)
        {
            switch (target)
            {
                case LogTarget.File:
                    logger = new FileLogger();
                    logger.Log(message);
                    break;
                default:
                    return;
            }
        }
    }
}
