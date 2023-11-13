using System.Collections.Generic;
using System.Threading;

namespace amperUtil
{
    public class DoubleQueue<T>
    {
        object m_lock = new object();
        Queue<T> m_queueWrite = new Queue<T>();
        Queue<T> m_queueRead = new Queue<T>();
        AutoResetEvent m_thereIsData = new AutoResetEvent(false);

        public void Write(T msg)
        {
            Monitor.Enter(m_lock);
            try
            {
                m_queueWrite.Enqueue(msg);
                m_thereIsData.Set();
            }
            finally
            {
                Monitor.Exit(m_lock);
            }
        }

        public bool WaitForData(int timeOut)
        {
            return m_thereIsData.WaitOne(timeOut);
        }

        public T Read()
        { 
            T res = InternalRead();
            if (EqualityComparer<T>.Default.Equals(res, default(T)))
            {
                Swap();
                return InternalRead();
            }
            return res;
        }
        private T InternalRead()
        {
            if (m_queueRead.Count > 0)
                return m_queueRead.Dequeue();
            return Default();
        }

        T Default()
        {
            return default(T);
        }
        private void Swap()
        {
            Monitor.Enter(m_lock);
            try
            {
                Queue<T> aux = m_queueWrite;
                m_queueWrite = m_queueRead;
                m_queueRead = aux;
                aux = null;
            }
            finally
            {
                Monitor.Exit(m_lock);
            }
        }
    }

    public class DoubleQueueSingleton<T>
    {
        static object m_lock = new object();
        static DoubleQueueSingleton<T> m_instance = null;

        DoubleQueue<T> m_doubleQueue;
        protected DoubleQueueSingleton()
        {
            m_doubleQueue = new DoubleQueue<T>();
        }

        static void CreateInstance()
        {
            Monitor.Enter(m_lock);
            try
            {
                if (m_instance == null)
                    m_instance = new DoubleQueueSingleton<T>();
            }
            finally
            {
                Monitor.Exit(m_lock);
            }
        }
        static public void Write(T message)
        {
            CreateInstance();
            m_instance.m_doubleQueue.Write(message);
        }
        static public T Read()
        {
            CreateInstance();
            return m_instance.m_doubleQueue.Read();
        }

        static public bool WaitForData(int timeOut)
        {
            CreateInstance();
            return m_instance.m_doubleQueue.WaitForData(timeOut);
        }
    }

}
