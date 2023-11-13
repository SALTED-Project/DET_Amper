
namespace amperUtil.Log
{
    public class ThreadLogConfig
    {
        static int minLogLevel = 0;
        static int maxLogLevel = 4;

        static int minNumberOfFiles = 10;
        static int maxNumberOfFiles = 1000;

        static int minNumberOfEntriesPerFile = 1000;
        static int maxNumberOfEntriesPerFile = 1000;

        static int minDaysLogPersistence = 30;
        static int maxDaysLogPersistence = 100;

        public string m_logDirectory { get; set; }
        public int m_logLevel { get; set; }
        public int m_numberOfFiles { get; set; }
        public int m_numberOfEntriesPerFile { get; set; }
        public int m_daysLogPersistence { get; set; }

        public string m_logFileName { get; set; }


        public void Check()
        {
            m_logLevel                  = InternalCheck(minLogLevel,                  maxLogLevel,                m_logLevel);
            m_numberOfFiles             = InternalCheck(minNumberOfFiles,             maxNumberOfFiles,           m_numberOfFiles);
            m_numberOfEntriesPerFile    = InternalCheck(minNumberOfEntriesPerFile,    maxNumberOfEntriesPerFile,  m_numberOfEntriesPerFile);
            m_daysLogPersistence        = InternalCheck(minDaysLogPersistence,        maxDaysLogPersistence,      m_daysLogPersistence);
        }

        private int InternalCheck(int min, int max, int value)
        {
            if (value < min)
                value = min;
            if (value > max)
                value = max;
            return value;
        }
    }
}
