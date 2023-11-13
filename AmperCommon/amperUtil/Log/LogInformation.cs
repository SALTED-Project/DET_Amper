
using System;


namespace amperUtil.Log
{
    public class LogInformation
    {
        static string[] LogLevelString =
            {
            "Value Info",
            "Error",
            "Warning",
            "Info",
            "Debug"
        };
        private string m_dateTime;
        private string m_msg;
        private int m_threadId;
        private string m_threadName;
        private LogLevel m_logLevel;
        private string m_memberName;
        private string m_sourceFilePath;
        private int m_sourceLineNumber;

        public LogInformation(string msg, LogLevel logLevel, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            m_dateTime = GetDateTimeFormated(DateTime.Now);

            m_msg = msg;

            m_logLevel = logLevel;

            m_threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            m_threadName = System.Threading.Thread.CurrentThread.Name;

            m_memberName = memberName;
            m_sourceFilePath = sourceFilePath;
            m_sourceLineNumber = sourceLineNumber;
        }

        public LogInformation(string msg, LogLevel logLevel, string forcedThreadName,string memberName, string sourceFilePath, int sourceLineNumber)
        {
            m_dateTime = GetDateTimeFormated(DateTime.Now);

            m_msg = msg;

            m_logLevel = logLevel;

            m_threadId = -1;
            m_threadName = forcedThreadName;

            m_memberName = memberName;
            m_sourceFilePath = sourceFilePath;
            m_sourceLineNumber = sourceLineNumber;
        }

        public LogInformation(AmperException ex, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            m_dateTime = GetDateTimeFormated(DateTime.Now);

            m_msg = "[[EXCEPTION]]";
            m_msg += ex.m_amperMessage;
            m_msg += "\r\n\t\t";
            m_msg += ex.m_message;
            m_msg += "\r\n\t\t";
            m_msg += ex.m_stackTrace;

            m_logLevel = LogLevel.Log_Critical;

            m_threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            m_threadName = System.Threading.Thread.CurrentThread.Name;

            m_memberName = memberName;
            m_sourceFilePath = sourceFilePath;
            m_sourceLineNumber = sourceLineNumber;
        }

        public LogInformation(AmperException ex, string forcedThreadName, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            m_dateTime = GetDateTimeFormated(DateTime.Now);

            m_msg = "[[EXCEPTION]]";
            m_msg += ex.m_amperMessage;
            m_msg += "\r\n\t\t";
            m_msg += ex.m_message;
            m_msg += "\r\n\t\t";
            m_msg += ex.m_stackTrace;

            m_logLevel = LogLevel.Log_Critical;

            m_threadId = -1;
            m_threadName = forcedThreadName;

            m_memberName = memberName;
            m_sourceFilePath = sourceFilePath;
            m_sourceLineNumber = sourceLineNumber;
        }

        private string GetDateTimeFormated(DateTime dt)
        {
            return string.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000}",
                dt.Year, dt.Month, dt.Day,
                dt.Hour, dt.Minute, dt.Second,
                dt.Millisecond);
        }
        public string Get()
        {
            int idx = m_sourceFilePath.LastIndexOf('\\');
            if (idx != -1)
                m_sourceFilePath = m_sourceFilePath.Substring(idx + 1);

            return string.Format("[{0}] [{1}] ThreadId:[{2:0000}] ThreadName:[{4}] File:[{7}] Member:[{5}] Line:[{6}] \r\n \t{3} \r\n",
                                m_dateTime, LogLevelString[(int)m_logLevel], m_threadId, m_msg,
                                m_threadName, m_memberName, m_sourceLineNumber, m_sourceFilePath);
        }
    }
}
