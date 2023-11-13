using System.Threading;
using amperUtil;

namespace amperUtil.Log
{
    public class Log
    {
        public static LogLevel m_logLevel = LogLevel.Log_Debug;

        static object m_lock = new object();
        static Log m_instance = null;

        public DoubleQueue<LogInformation> m_doubleQueue_logInformation;
        private Log()
        {
            m_doubleQueue_logInformation = new DoubleQueue<LogInformation>();
        }
        static private void CreateInstance()
        {
            Monitor.Enter(m_lock);
            try
            {
                if (m_instance == null)
                    m_instance = new Log();
            }
            finally
            {
                Monitor.Exit(m_lock);
            }
        }
        static public void Write(string message, LogLevel logLevel = LogLevel.Log_Debug,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (Log.m_logLevel < logLevel)
                return;
            CreateInstance();
            m_instance.m_doubleQueue_logInformation.Write(new LogInformation(message, logLevel, memberName, sourceFilePath, sourceLineNumber));
        }
        static public void WriteForcedThreadName(string message, string forcedThreadName, LogLevel logLevel = LogLevel.Log_Debug,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (Log.m_logLevel < logLevel)
                return;
            CreateInstance();
            m_instance.m_doubleQueue_logInformation.Write(new LogInformation(message, logLevel, forcedThreadName, memberName, sourceFilePath, sourceLineNumber));
        }
        static public void Write(AmperException ex,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateInstance();
            m_instance.m_doubleQueue_logInformation.Write(new LogInformation(ex, memberName, sourceFilePath, sourceLineNumber));
        }
        static public void WriteForcedThreadName(AmperException ex, string forcedThreadName,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateInstance();
            m_instance.m_doubleQueue_logInformation.Write(new LogInformation(ex, forcedThreadName, memberName, sourceFilePath, sourceLineNumber));
        }
        static public string Read()
        {
            CreateInstance();
            LogInformation li = m_instance.m_doubleQueue_logInformation.Read();
            if (li == null)
                return null;
            return li.Get();
        }

        static public bool _WaitForData(int timeOut)
        {
            CreateInstance();
            return m_instance.m_doubleQueue_logInformation.WaitForData(timeOut);
        }

    }
}
