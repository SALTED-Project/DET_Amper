using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Timers;

//Buena descripción de log4net=>    https://logging.apache.org/log4net/release/manual/introduction.html

namespace amperUtil.Log
{
    public class Log4NetAux
    {
        #region Properties

        public static bool LogSql
        {
            get
            {
                if (_logSql != null)
                    return (bool)_logSql;

                string s = ConfigurationManager.AppSettings.Get("LogSQL");

                if (string.IsNullOrWhiteSpace(s))
                    return false;

                s = s.ToUpper().Trim();
                _logSql = (s == "TRUE" || s == "SI" || s == "YES" || s == "1");

                return (bool)_logSql;
            }
        }

        #endregion Properties

        #region Fields

        private static bool? _logSql = null;
        private static RollingFileAppender _roller = null;
        private static DateTime _lastClean = DateTime.MinValue;
        private static int _cleanInterval = 3600 * 1000;

        #endregion Fields

        #region Methods

        public static ILog GetLogger(Type type)
        {
            ConfigureIfNeeded();

            return LogManager.GetLogger(type);
        }

        public static ILog GetLogger(string name)
        {
            ConfigureIfNeeded();

            return LogManager.GetLogger(name);
        }

        public static Hierarchy ConfigureIfNeeded()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            if (hierarchy.Configured)
                return hierarchy;

            var logLevel = Level.Debug;
            var logFileName = string.Empty;

            try
            {
                var logLevelString = ConfigurationManager.AppSettings.Get("LogLevel");

                if (!string.IsNullOrEmpty(logLevelString))
                {
                    logLevelString = logLevelString.ToUpper();
                    if (logLevelString.StartsWith("INFO")) logLevel = Level.Info;
                    else if (logLevelString.StartsWith("WARN")) logLevel = Level.Warn;
                    else if (logLevelString == "ERROR") logLevel = Level.Error;
                    else if (logLevelString == "CRITICAL") logLevel = Level.Critical;
                    else logLevel = Level.Debug;
                }
            }
            catch (ConfigurationErrorsException) { }

            //logFileName

            PatternLayout patternLayout = new PatternLayout
            {
                //https://logging.apache.org/log4net/log4net-1.2.13/release/sdk/log4net.Layout.PatternLayout.html
                ConversionPattern = "%date [%5thread] %-5.5level - %30.30C.%-30.30M: %message%newline"
                //Old ConversionPattern = "%date %-5level [%thread] %logger -> %message%newline"
            };

            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender
            {
                Name = "MainFileAppender",
                AppendToFile = true,
                File = LogFileName,
                Layout = patternLayout,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                //DatePattern = "' ['yyyy' 'MM' 'dd']'", //"' 'yyyy.MM.dd HH.mm" Era más bonito, pero producía un cambio cada minuto...

                //StaticLogFileName = false, // default  true

                // MaxSizeRollBackups
                //          The maximum number of backup files that are kept before the oldest is erased.
                //
                //          0: (Default) there will be no backup files and the log file will be truncated when it reaches MaxFileSize.
                //   negative: no deletions will be made.Note that this could result in very slow performance as a large number of files are rolled over unless CountDirection is used.
                MaxSizeRollBackups = GetMaxFiles(),

                ////CountDirection =1,// Default -1. CountDirection >= 0 => log.1 is the first backup made, log.5 is the 5th backup made, etc. For infinite backups use CountDirection >= 0 to reduce rollover costs.(Queda raro...)
                //PreserveLogFileNameExtension = true,//Default false
                MaximumFileSize = GetMaxSize()
                //MaximumFileSize = "400KB", // Para pruebas
            };
            _roller = roller;

            roller.ActivateOptions();

            hierarchy.Root.AddAppender(roller);

            hierarchy.Root.Level = logLevel;
            hierarchy.Configured = true;

            StartCleanUpTimer();

            return hierarchy;
        }        

        #endregion Methods

        #region Properties

        private static string _logFileName = string.Empty;

        public static string LogFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_logFileName))
                    _logFileName = GetFileFullName();
                return _logFileName;
            }
        }

        private static string GetPath()
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~") + "Logs";
            string logsDirectory = ConfigurationManager.AppSettings.Get("LogPath");
            if (!string.IsNullOrEmpty(logsDirectory))
            {
                path = logsDirectory;
                if (path.StartsWith("~"))
                {
                    string rPath = path.Replace("~", "");
                    path = System.Web.HttpContext.Current.Server.MapPath("~") + rPath;
                }
            }
            return path;
        }

        private static string GetFileFullName()
        {
            string filePath = GetPath();
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            string logFileName = ConfigurationManager.AppSettings.Get("LogFileName");
            if (string.IsNullOrEmpty(logFileName))
                logFileName = "nLogs.log";

            return filePath + "\\" + logFileName;
        }

        private static int GetMaxFiles()
        {
            int maxFiles = 5;
            string strMaxFiles = ConfigurationManager.AppSettings.Get("LogMaxFiles");
            if (!string.IsNullOrEmpty(strMaxFiles))
                int.TryParse(strMaxFiles, out maxFiles);
            return maxFiles;
        }

        private static string GetMaxSize()
        {
            string maxSize = ConfigurationManager.AppSettings.Get("LogFileMaxSize");
            if (string.IsNullOrEmpty(maxSize))
                return "100MB";
            return maxSize;
        }

        private static DateTime GetLogMinDate()
        {
            DateTime minDate = DateTime.Today;

            int maxDays = 10;
            string strMaxDays = ConfigurationManager.AppSettings.Get("LogFileMaxDays");
            if (!string.IsNullOrEmpty(strMaxDays))
                int.TryParse(strMaxDays, out maxDays);

            if (maxDays > 0)
                minDate = minDate.Subtract(new TimeSpan(maxDays, 0, 0, 0));

            return minDate;
        }

        #endregion

        #region - Clean -

        private static void StartCleanUpTimer()
        {
            DateTime dTime = GetLogMinDate();
            CleanUp(dTime);
            Timer cleanTimer = new Timer(3600 * 1000);
            cleanTimer.Elapsed += CleanTimer_Elapsed;
            cleanTimer.Start();
        }

        private static void CleanTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime dTime = GetLogMinDate();
            CleanUp(dTime);
        }

        /// <summary>
        /// Cleans up. Auto configures the cleanup based on the log4net configuration
        /// </summary>
        /// <param name="date">Anything prior will not be kept.</param>
        protected static void CleanUp(DateTime date)
        {
            if (_roller == null)
                return;
            CleanUp(_roller, date, Log4NetAux._lastClean, Log4NetAux._cleanInterval);
            Log4NetAux._lastClean = DateTime.Now;
        }

        /// <summary>
        /// Cleans up.
        /// </summary>
        /// <param name="logDirectory">The log directory.</param>
        /// <param name="logPrefix">The log prefix. Example: logfile dont include the file extension.</param>
        /// <param name="date">Anything prior will not be kept.</param>
        public static void CleanUp(RollingFileAppender roller, DateTime date, DateTime lastClean, int cleanInterval)
        {
            if (roller == null)
                return;

            string logDirectory = Path.GetDirectoryName(roller.File);
            string logPrefix = Path.GetFileName(roller.File);

            if (string.IsNullOrEmpty(logDirectory))
                throw new ArgumentException("logDirectory is missing");

            if (string.IsNullOrEmpty(logPrefix))
                throw new ArgumentException("logPrefix is missing");

            var dirInfo = new DirectoryInfo(logDirectory);
            if (!dirInfo.Exists)
                return;

            var fileInfos = dirInfo.GetFiles("{0}*.*".Sub(logPrefix));
            if (fileInfos.Length == 0)
                return;

            foreach (var info in fileInfos)
            {
                if (info.LastWriteTime < date)
                {
                    try
                    {
                        info.Delete();
                    }
                    catch { }
                }
            }

        }
        #endregion

    }

    /// <summary>
    /// Extension helper methods for strings
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough, System.Diagnostics.DebuggerNonUserCode]
    public static class StringExtensions
    {
        /// <summary>
        /// Formats a string using the <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        /// <returns>A string with the format placeholders replaced by the args.</returns>
        public static string Sub(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}