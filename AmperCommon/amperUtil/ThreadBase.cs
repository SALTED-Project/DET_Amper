using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace amperUtil
{
    public interface ExceptionSubscriber
    {
        void UnhandledExceptionHappened(Exception ex);
    }

    public abstract class ThreadBase
    {
        static Int32 ThreadBaseDefaultPeriod = 50;
        static Int32 ThreadMinimunTimeout = 500;

        private Thread m_Thread = null;
        private Int32 m_period = ThreadBaseDefaultPeriod;
        volatile protected BoolInterlocked m_bRun = new BoolInterlocked(true);

        public ManualResetEvent m_eventIamStopped = new ManualResetEvent(false);

        ExceptionSubscriber m_exceptionSubscriber = null;

        public ThreadBase(Int32 period, ExceptionSubscriber exceptionSubscriber)
        {
            if (period < 0)
                throw new Exception("Thread period has to be positive");

            m_period = period;
            m_exceptionSubscriber = exceptionSubscriber;
        }

        ~ThreadBase()
        {
            try
            {
                // Pues si no me cierran, acabo yo por las bravas
                if (m_Thread != null)
                {
                    m_Thread.Abort();
                    m_Thread = null;
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                //String s = "";
                //MyException me = new MyException(ref e, ref s);
            }
        }



        public bool Start(String ThreadName, bool bSleep)
        {
            try
            {
                if (bSleep == true)
                    m_Thread = new Thread(RunSleep);
                else
                    m_Thread = new Thread(RunWithoutSleep);
                if (m_Thread == null)
                    return false;
                m_Thread.Name = ThreadName;
                m_Thread.Start();
                m_bRun.SetTrue();
                return true;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                //String s = "";
                //MyException me = new MyException(ref e, ref s);
                return false;
            }
        }
        public void RequestStopThread()
        {
            try
            {
                if (m_Thread == null)
                    return;

                m_bRun.SetFalse();
                //Thread.Yield();
                Int32 timeOut = m_period * 2;
                if (timeOut < ThreadMinimunTimeout)
                    timeOut = ThreadMinimunTimeout;

                if (m_Thread.Join(timeOut) == true)
                {
                    m_Thread = null;
                }
            }
            finally
            {
            }
        }
        public void StopThread()
        {
            try
            {
                if (m_Thread == null)
                    return;

                m_bRun.SetFalse();
                Int32 timeOut = m_period * 2;
                if (timeOut < ThreadMinimunTimeout)
                    timeOut = ThreadMinimunTimeout;

                if (m_Thread.Join(timeOut) == false)
                {
                    // Le doy un poco más
                    Thread.Sleep(timeOut);
                    if (m_eventIamStopped.WaitOne(1000) == false)
                    {
                        m_Thread.Abort();
                        m_Thread = null;
                    }
                }

            }
            finally
            {
                m_Thread = null;
            }
        }

        private void RunSleep()
        {
            try

            {
                if (Init() == false)
                    return;


                while (m_bRun.Value() == true)
                {
                    Do();
                    if (m_bRun.Value() == true)
                    {
                        Thread.Sleep(m_period);
                    }
                }

                CleanUp();
            }
            catch (ThreadAbortException ex)
            {
                //Log.Log.Write(new AmperException(string.Format("{0} Aborted", m_Thread != null ? m_Thread.Name : "Unknow thread"), ex));
                UnhandledException(ex);
            }
            catch (Exception ex)
            {
                //Log.Log.Write(new AmperException(string.Format("{0} Unhandled exception ", m_Thread != null ? m_Thread.Name : "Unknow thread"), ex));
                UnhandledException(ex);
            }
            finally
            {
                m_eventIamStopped.Set();
            }
        }
        private void RunWithoutSleep()
        {
            try

            {
                if (Init() == false)
                    return;


                while (m_bRun.Value() == true)
                {
                    Do();
                }

                CleanUp();
            }
            catch (ThreadAbortException ex)
            {
                //Log.Log.Write(new AmperException(string.Format("{0} Aborted", m_Thread != null ? m_Thread.Name : "Unknow thread"), ex));
                UnhandledException(ex);
            }
            catch (Exception ex)
            {
                //Log.Log.Write(new AmperException(string.Format("{0} Unhandled exception ", m_Thread != null ? m_Thread.Name : "Unknow thread"), ex));
                UnhandledException(ex);
            }
            finally
            {
                m_eventIamStopped.Set();
            }
        }
        protected abstract bool Init();
        protected abstract void Do();
        protected abstract void CleanUp();

        protected void UnhandledException(Exception e)
        {
            if (m_exceptionSubscriber != null)
            {
                try
                {
                    CleanUp();
                }
                catch (Exception)
                {
                }
                try
                {
                    m_eventIamStopped.Set();
                }
                catch (Exception)
                {
                }

                m_exceptionSubscriber.UnhandledExceptionHappened(e);
            }
        }

        public bool AreYouRunning()
        {
            ThreadState myThreadState = m_Thread.ThreadState;
            if (myThreadState == ThreadState.Running || myThreadState == ThreadState.WaitSleepJoin)
                return true;
            else
                return false;

        }

        public bool SetThreadPriority(ThreadPriority threadPriority)
        {
            if (m_Thread == null)
                return false;
            m_Thread.Priority = threadPriority;

            return true;
        }
    }

    public class AmperException : Exception
    {
        public readonly string m_amperMessage;
        public readonly string m_message;
        public readonly string m_stackTrace;
        public AmperException(string amperMessage, Exception ex)
        {
            m_amperMessage = amperMessage;
            m_message = ex.Message;
            m_stackTrace = ex.StackTrace;
        }
    }
}
