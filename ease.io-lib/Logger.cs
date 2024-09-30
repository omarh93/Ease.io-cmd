using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ease.io_lib
{
    public class LogManager
    {
        // A Logger dispenser for the current assembly (Remember to call Flush on application exit)
        public static LogFactory Instance { get { return _instance.Value; } }
        private static Lazy<LogFactory> _instance = new Lazy<LogFactory>(BuildLogFactory);

        public static string pathFileName = string.Empty;
        public static string FileName = string.Empty;

        public static long maxSize = 100000000;
        public static int maxFiles = 10;

        public static void reset()
        {
            _instance = new Lazy<LogFactory>(BuildLogFactory);
        }


        public static void enableInternalLog()
        {
            NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
            NLog.Common.InternalLogger.LogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\IGS\nlog.txt";
        }

        private static LogFactory BuildLogFactory()
        {
            var config = new LoggingConfiguration();

            string _pathFileName;
            if (pathFileName.Length > 0)
            {
                _pathFileName = pathFileName;
            }
            else if (FileName.Length > 0)
            {
                _pathFileName = @"${basedir}/" + FileName;
            }
            else
            {
                _pathFileName = @"${basedir}/logfile.txt"; // default 
            }

            if (maxSize < 10000000)
            {
                maxSize = 10000000;  // 10 meg minimum
            }

            var fileTarget = new FileTarget("logfile")
            {
                FileName = _pathFileName,
                Layout = "${longdate} ${level} ${message}",
                CreateDirs = true,
                ArchiveAboveSize = maxSize,
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                MaxArchiveFiles = maxFiles
            };
            config.AddTarget(fileTarget);
            config.AddRuleForAllLevels(fileTarget);

            LogFactory logFactory = new LogFactory();

            logFactory.ThrowExceptions = false;
            logFactory.Configuration = config;
            return logFactory;
        }
    }
}
