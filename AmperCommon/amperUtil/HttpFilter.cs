using System;

namespace amperUtil.Http
{
    public enum HttpFilterOperator
    {
        EQUAL,NOT_EQUAL,START_BY,CONTAIN,GREATER,GREATER_EQUAL,LESS,LESS_EQUAL,AND,OR
    }
    public class HttpFilterOperatorString
    {
        public static string Get(HttpFilterOperator httpFilterOperator)
        {
            string ret;
            switch (httpFilterOperator)
            {
                case HttpFilterOperator.EQUAL:
                    ret = "eq";
                    break;
                case HttpFilterOperator.NOT_EQUAL:
                    ret = "ne";
                    break;
                case HttpFilterOperator.CONTAIN:
                    ret = "cn";
                    break;
                case HttpFilterOperator.START_BY:
                    ret = "st";
                    break;
                case HttpFilterOperator.GREATER:
                    ret = "gt";
                    break;
                case HttpFilterOperator.GREATER_EQUAL:
                    ret = "ge";
                    break;
                case HttpFilterOperator.LESS:
                    ret = "lt";
                    break;
                case HttpFilterOperator.LESS_EQUAL:
                    ret = "le";
                    break;
                case HttpFilterOperator.AND:
                    ret = "and";
                    break;
                case HttpFilterOperator.OR:
                    ret = "or";
                    break;
                default:
                    ret = "";
                    throw new Exception(httpFilterOperator.ToString());
                    //break;
            }
            return ret;
        }
    }
    public class HttpFilterParameter
    {
        string m_filter;
        public HttpFilterParameter(string name, HttpFilterOperator httpFilterOperator, string value,bool isnot=false,bool bSubFilter=false)
        {
            if (bSubFilter == false)
            {
                m_filter = string.Format("{0} {1} '{2}'",
                    name, HttpFilterOperatorString.Get(httpFilterOperator), value);
                if (isnot == true)
                {
                    m_filter = string.Format("not {0}", m_filter);
                }
            }
            else
            {
                if (isnot == false)
                { 
                    m_filter = string.Format("({0} {1} '{2}')",
                       name, HttpFilterOperatorString.Get(httpFilterOperator), value);
                }
                else
                {
                    m_filter = string.Format("(not {0} {1} '{2}')",
                       name, HttpFilterOperatorString.Get(httpFilterOperator), value);
                }
            }

        }
        public HttpFilterParameter(string name, HttpFilterOperator httpFilterOperator, Int32 value)
        {
            m_filter = string.Format("{0} {1} {2}",
                name, HttpFilterOperatorString.Get(httpFilterOperator), value);
        }
        public HttpFilterParameter(string name, HttpFilterOperator httpFilterOperator, Int64 value)
        {
            m_filter = string.Format("{0} {1} {2}",
                name, HttpFilterOperatorString.Get(httpFilterOperator), value);
        }
        public HttpFilterParameter(string name, HttpFilterOperator httpFilterOperator, bool value)
        {
            m_filter = string.Format("{0} {1} {2}",
                name, HttpFilterOperatorString.Get(httpFilterOperator), value==true?"true":"false");
        }

        public override string ToString()
        {
            return m_filter;
        }
    }
    public class HttpFilter
    {
        string m_filter;
        public HttpFilter(HttpFilterParameter httpFilterParameter)
        {
            m_filter = string.Format("/?filter={0}",httpFilterParameter.ToString());
        }
        public HttpFilter(HttpFilterParameter httpFilterParameter1,HttpFilterOperator httpFilterOperator, HttpFilterParameter httpFilterParameter2,bool bSubFilter=false)
        {
            if (bSubFilter == false)
            {
                m_filter = string.Format("/?filter={0} {1} {2}",
                    httpFilterParameter1.ToString(),
                    HttpFilterOperatorString.Get(httpFilterOperator),
                    httpFilterParameter2.ToString()
                    );
            }
            else
            {
                m_filter = string.Format("({0} {1} {2})",
                    httpFilterParameter1.ToString(),
                    HttpFilterOperatorString.Get(httpFilterOperator),
                    httpFilterParameter2.ToString()
                    );
            }
        }
        public HttpFilter(HttpFilter httpFilter, HttpFilterOperator httpFilterOperator, HttpFilterParameter httpFilterParameter2,bool bSubFilter=false)
        {
            if (bSubFilter == false)
            { 
                m_filter = string.Format("/?filter={0} {1} {2}",
                    httpFilter.ToString(),
                    HttpFilterOperatorString.Get(httpFilterOperator),
                    httpFilterParameter2.ToString()
                    );
            }
            else
            {
                m_filter = string.Format("/?filter=({0} {1} {2})",
                    httpFilter.ToString(),
                    HttpFilterOperatorString.Get(httpFilterOperator),
                    httpFilterParameter2.ToString()
                    );
            }
        }
        public override string ToString()
        {
            return m_filter;
        }

    }

    public class OrderBy
    {
        string m_field = "";
        bool m_bAsc = true;

        public OrderBy(string field, bool bAsc = true)
        {
            m_field = field;
            m_bAsc = bAsc;
        }

        public override string ToString()
        {
            string sAsc = "-asc";
            if (m_bAsc == false)
                sAsc = "-desc";
            string ret = string.Format("/?orderBy={0}{1}",
                m_field,
                sAsc
                );

            return ret;

        }
    }
}
