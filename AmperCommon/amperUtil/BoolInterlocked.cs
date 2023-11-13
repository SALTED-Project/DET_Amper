
namespace amperUtil
{
    public class BoolInterlocked
    {
        private long m_value = 0;

        public BoolInterlocked(bool value)
        {
            if (value == true)
                SetTrue();
        }

        public void SetTrue()
        {
            System.Threading.Interlocked.CompareExchange(ref m_value, 1, 0);
        }
        public void SetFalse()
        {
            System.Threading.Interlocked.CompareExchange(ref m_value, 0, 1);
        }

        public bool Value()
        {
            long value = System.Threading.Interlocked.Read(ref m_value);
            if (value == 1)
                return true;
            return false;
        }
    }
}
