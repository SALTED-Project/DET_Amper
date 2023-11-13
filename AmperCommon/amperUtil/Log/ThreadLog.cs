
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace amperUtil.Log
{
    public class ThreadLog : ThreadBase
    {
        static Int32 ThreadLogPeriod = 50;

        ThreadLogConfig m_threadLogConfig;
        StreamWriter m_streamWriter;
        FileStream m_fileStream;

        TimeSpan m_tsDaysCleanLimit;

        string m_highLevelDirectory;
        string m_currentDirectory;
        string m_currentDirectoryPattern;

        int m_currentIdx = 0;
        int m_currentNumberOfEntries = 0;

        DateTime m_lastCheckDeleteDirectories;

        public ThreadLog(ThreadLogConfig threadLogConfig, ExceptionSubscriber exceptionSubscriber) : base(ThreadLogPeriod,exceptionSubscriber)
        {
            m_threadLogConfig = threadLogConfig;
            m_threadLogConfig.Check();
            m_tsDaysCleanLimit = new TimeSpan(m_threadLogConfig.m_daysLogPersistence, 0, 0, 0);


            Log.m_logLevel = (LogLevel)(m_threadLogConfig.m_logLevel);

            Log.Write("ThreadLog created");
        }

        protected override bool Init()
        {
            if (CreatePath() == false)
                return false;

            CreateStreams();

            DeleteDirectories();
            return true;
        }

        bool CreatePath()
        {
            if (false == CreateDirectory(m_threadLogConfig.m_logDirectory))
                return false;

            //m_threadLogConfig.m_logDirectory += "/nemesisGateway";
            m_highLevelDirectory = m_threadLogConfig.m_logDirectory;

            if (false == CreateDirectory(m_threadLogConfig.m_logDirectory))
                return false;

            DateTime dt = DateTime.Now;
            string dateTime = string.Format("{0:0000} {1:00} {2:00}   {3:00} {4:00} {5:00}   {6:000}",
                            dt.Year, dt.Month, dt.Day,
                            dt.Hour, dt.Minute, dt.Second,
                            dt.Millisecond);
            m_threadLogConfig.m_logDirectory += "/" + dateTime ;
            m_currentDirectory = m_threadLogConfig.m_logDirectory;
            m_currentDirectoryPattern = dateTime;

            return CreateDirectory(m_threadLogConfig.m_logDirectory);
        }
        bool CreateDirectory(string path)
        {
            if (Directory.Exists(path) == true)
                return true;
            return Directory.CreateDirectory(path).Exists;
        }

        string GetFileName()
        {
            //return string.Format("{0}/nemesisGateway_{1:0000}.log", m_threadLogConfig.m_logDirectory, m_currentIdx);
            return string.Format("{0}/{2}_{1:0000}.log", m_threadLogConfig.m_logDirectory, m_currentIdx, m_threadLogConfig.m_logFileName);
        }
        void CreateStreams()
        {
            if (m_fileStream != null || m_streamWriter != null)
                return;
            m_fileStream = new FileStream(GetFileName(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            m_streamWriter = new StreamWriter(m_fileStream, Encoding.Unicode);
        }

        void CloseStreams()
        {
            if (m_streamWriter != null)
            {
                m_streamWriter.Flush();
                m_streamWriter.Close();
            }
            if (m_fileStream != null)
                m_fileStream.Close();

            m_streamWriter = null;
            m_fileStream = null;
        }

        void RecreateStreams()
        {
            if (m_currentNumberOfEntries < m_threadLogConfig.m_numberOfEntriesPerFile)
                return;

            CloseStreams();
            ++m_currentIdx;
            if (m_currentIdx == 10000)
                m_currentIdx = 0;

            CreateStreams();
            m_currentNumberOfEntries = 0;

            DeleteFiles();

        }

        void DeleteDirectories()
        {
            DateTime now = DateTime.Now;
            m_lastCheckDeleteDirectories = now;

            var directories = Directory.GetDirectories(m_highLevelDirectory, "*", SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                if (directory.Contains(m_currentDirectoryPattern) == false)
                {
                    var dt = Directory.GetLastWriteTime(directory);
                    var ts = now - dt;
                    if (ts > m_tsDaysCleanLimit)
                        Directory.Delete(directory, true);
                }
            }

        }
        void DeleteFiles()
        {
            var files = Directory.GetFiles(m_currentDirectory, "*", SearchOption.AllDirectories);
            if (files.Length <= m_threadLogConfig.m_numberOfFiles)
                return;
            int nFilesToBeDeleted = files.Length - m_threadLogConfig.m_numberOfFiles;

            List<Tuple<DateTime, string>> listFiles = new List<Tuple<DateTime, string>>();
            foreach (var file in files)
            {
                var dt = File.GetLastWriteTime(file);
                listFiles.Add(new Tuple<DateTime, string>(dt,file));
            }

            listFiles.Sort();

            List<string> filesToDelete = new List<string>();
            int n = 0;
            foreach (var dtFile in listFiles)
            {
                if (n < nFilesToBeDeleted)
                {
                    File.Delete(dtFile.Item2);
                    ++n;
                }
                else
                {
                    break;
                }
            }
        }
        protected override void Do()
        {
            ReadMessages();
            if (DateTime.Now - m_lastCheckDeleteDirectories > new TimeSpan( 6, 0, 0))
                DeleteDirectories();
            Log._WaitForData(ThreadLog.ThreadLogPeriod);
        }
        private void ReadMessages()
        {
            string msg = null;
            do
            {
                msg = Log.Read();
                WriteMsg(msg);
            }
            while (msg != null);

        }
        protected override void CleanUp()
        {
            ReadMessages();
            CloseStreams();
        }

        void WriteMsg(string msg)
        {
            if (msg != null)
            {
                m_streamWriter.WriteLine(msg);
                m_streamWriter.Flush();

                ++m_currentNumberOfEntries;

                RecreateStreams();
            }
        }

    }
}
