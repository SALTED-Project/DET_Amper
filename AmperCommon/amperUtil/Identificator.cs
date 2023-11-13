using System;


namespace amperUtil
{
    public class Identificator<T> where T : IComparable
    {
        public T identificador { get; }
        public Identificator(T id)
        {
            System.Diagnostics.Debug.Assert(id != null, "Identificator can not be null");
            identificador = id;
        }


        public override int GetHashCode()
        {
            return identificador.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            Identificator<T> other = obj as Identificator<T>;
            return identificador.CompareTo(other.identificador) == 0 ? true : false;
        }

        public static bool IsNull(Identificator<T> a)
        {
            try
            {
                Identificator<T> b = new Identificator<T>(a.identificador);
                return false;
            }
            catch
            {
                return true;
            }
        }
        public static bool operator ==(Identificator<T> a, Identificator<T> b)
        {

            if (IsNull(a) == true && IsNull(b) == true)
                return true;
            if (IsNull(a) == true || IsNull(b) == true)
                return false;

            return a.identificador.Equals(b.identificador);
        }
        public static bool operator !=(Identificator<T> a, Identificator<T> b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return identificador.ToString();
        }
    }

    public class IdentificadorString : Identificator<string>
    {
        public IdentificadorString(string id) : base(id)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(id), "Identificator can not be null or empty");
            if (string.IsNullOrEmpty(id))
                throw new AmperException("IdentificadorString  can not be null or empty", new Exception("IdentificadorString can not be null or empty"));

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

}
