using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;

using amperUtil.Http;
using amperUtil.Log;
using amperUtil.Auth;


using Newtonsoft.Json;

namespace AmperCore
{
    public class HttpSender
    {
        object m_lockToken = new object();
        AuthToken m_token;
        string m_url;
        Tuple<string, string> m_additionalHeader;
        private String profile = null;

        public string Profile { get => profile; set => profile = value; }

        public HttpSender(string url)
        {
            m_url = url;
        }

        public void SetToken(AuthToken token)
        {
           Monitor.Enter(m_lockToken);
            try
            {
                Log.Write(string.Format("Updating token {0}", token.token));
                m_token = token;
            }
            finally
            {
                Monitor.Exit(m_lockToken);
            }

        }

        public void SetUrl(string url)
        {
            try
            {
                Log.Write(string.Format("Updating url {0}", url));
                m_url = url;
            }
            catch(Exception ex)
            { Log.Write(string.Format("Exception Updating url {0}", ex.Message)); }
        }

        public void SetAdditionalHeader(Tuple<string,string> additionalHeader)
        {
            Monitor.Enter(m_lockToken);
            try
            {
                Log.Write(string.Format("Updating header ID {0} {1}", additionalHeader.Item1, additionalHeader.Item2));
                m_additionalHeader = null;
                m_additionalHeader = additionalHeader;
            }
            finally
            {
                Monitor.Exit(m_lockToken);
            }

        }

        public HttpCallResult Post(object message)
        {
            HttpCallResult res = HttpCall.Post<object>
                (m_url, 
                message, 
                m_token.token,
                m_additionalHeader).Result;
            if (res.FAIL() == true)
                Log.Write(res.GetMsgString(), LogLevel.Log_Error);
            return res;
        }

        public HttpCallResult Get(object message)
        {
            HttpFilter httpFilter = null;
            HttpCallResult res = HttpCall.Get(m_url, m_token.token, httpFilter).Result;
            if (res.FAIL() == true)
                Log.Write(res.GetMsgString(), LogLevel.Log_Error);
            return res;
        }

        public HttpCallResult Get(object message, string id)
        {
            HttpFilter httpFilter = null;
            HttpCallResult res = HttpCall.Get(m_url, id, m_token.token).Result;
            if (res.FAIL() == true)
                Log.Write(res.GetMsgString(), LogLevel.Log_Error);
            return res;
        }

        public HttpCallResult Get(object message, string filtro, string valor)
        {
            HttpFilter httpFilter = new HttpFilter(new HttpFilterParameter(filtro, HttpFilterOperator.EQUAL, valor));
            HttpCallResult res = HttpCall.Get(m_url, m_token.token, httpFilter).Result;
            if (res.FAIL() == true)
                Log.Write(res.GetMsgString(), LogLevel.Log_Error);
            return res;
        }

        public HttpCallResult Get(object message, string filtro, int valor)
        {
            HttpFilter httpFilter = new HttpFilter(new HttpFilterParameter(filtro, HttpFilterOperator.EQUAL, valor));
            HttpCallResult res = HttpCall.Get(m_url, m_token.token, httpFilter).Result;
            if (res.FAIL() == true)
                Log.Write(res.GetMsgString(), LogLevel.Log_Error);
            return res;
        }

        public HttpCallResult Put(string id, object message)
        {
            if (m_additionalHeader != null)
            {
                HttpCallResult res = HttpCall.Put<object>
                    (m_url,
                    id,
                    message,
                    m_token.token, m_additionalHeader).Result;
                if (res.FAIL() == true)
                    Log.Write(res.GetMsgString(), LogLevel.Log_Error);
                return res;
            }
            else
            {
                HttpCallResult res = HttpCall.Put<object>
                    (m_url,
                    id,
                    message,
                    m_token.token).Result;
                if (res.FAIL() == true)
                    Log.Write(res.GetMsgString(), LogLevel.Log_Error);
                return res;
            }
            
        }

        //public HttpResponseMessage Patch(string id, object message)
        //{
        //    HttpResponseMessage res = HttpCall.Patch<object>
        //        (m_url,
        //        id,
        //        message,
        //        m_token.token).Result;

        //    if (!res.IsSuccessStatusCode == true)
        //        Log.Write(res.ReasonPhrase.ToString(), LogLevel.Log_Error);
        //    return res;

        //}

        public AuthToken GetTokenAs(/*string profile,*/HttpFilter httpFilter = null)
        {
            AuthToken ret = null;
            string url = m_url;
            //String  url = m_url  + "?profile=ProfileDev";

            if (profile != null)
            {
                url = m_url + "?profile="+profile;
            }
           
          HttpCallResult res = HttpCall.Get
                (url,
                m_token.token,
                httpFilter).Result;
            if (res.FAIL() == true)
                Log.Write(res.GetMsgString(), LogLevel.Log_Error);
            try
            {
                //Dictionary<string, string> list = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.GetJSON().ToString());
                String resString = res.GetJSON().ToString();
                Dictionary<string, string> list = JsonConvert.DeserializeObject<Dictionary<string, string>>(resString);
                if (list.ContainsKey("token") == false)
                {

                    return null;
                }

                ret = new AuthToken();
                ret.token = (list["token"]);

            }
            catch (Exception e)
            {
                Log.Write("GetTokenAs " + e.ToString());
            }
            return ret;
        }

    }
}
