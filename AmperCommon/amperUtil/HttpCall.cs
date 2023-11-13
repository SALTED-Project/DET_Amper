using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace amperUtil.Http
{
    internal class BLExceptionParser
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
    public class ErrorMessage
    {
        static string ErrorMessage_Default = "nemesis UNDEFINED";
        private string m_msg;
        public ErrorMessage()
        {
            m_msg = ErrorMessage.ErrorMessage_Default;
        }
        public ErrorMessage(string msg)
        {
            m_msg = msg;
        }
        public void Add(string sth)
        {
            m_msg += ". " + sth;
        }

        public override string ToString()
        {
            return m_msg;
        }

        public void Clear()
        {
            m_msg = null;
        }

    }

    public class HttpCallResult //: ActionResult
    {
        RawJSON m_json;
        ErrorMessage m_msg = new ErrorMessage();
        bool m_ok = false;
        HttpStatusCode m_StatusCode = HttpStatusCode.Unused;

        public HttpCallResult()
        {
        }
        public HttpCallResult(string error)
        {
            SetMsg(error,HttpStatusCode.MethodNotAllowed);
        }
        public HttpCallResult(bool ok)
        {
            m_ok = true;
        }
        public HttpCallResult(HttpCallResult res1, HttpCallResult res2)
        {
            HttpCallResult res = res2;
            if (res1.FAIL())
            {
                res = res1;
            }

            m_ok = res.m_ok;
            m_json = res.m_json;
            m_msg = res.m_msg;
            m_StatusCode = res.m_StatusCode;
        }
        public void Add(HttpCallResult res)
        {
            if (res.OK())
            {
                return;
            }

            m_ok = res.m_ok;
            m_json = res.m_json;
            m_msg = res.m_msg;
            m_StatusCode = res.m_StatusCode;
        }

        public void SetOK()
        {
            m_ok = true;
            m_msg.Clear();
            m_StatusCode = HttpStatusCode.OK;
        }

        public void SetJson(string json)
        {
            m_json = new RawJSON(json);
            SetOK();
        }

        public void SetMsg(string msg,HttpStatusCode httpStatusCode)
        {
            m_msg = new ErrorMessage(msg);
            m_StatusCode = httpStatusCode;

            m_ok = false;
            
        }

        public void SetMsgSmart(HttpResponseMessage response)
        {
            BLExceptionParser bLExceptionParser = null;
            string blExceptionRaw = response.Content.ReadAsStringAsync().Result;
            bLExceptionParser = JsonConvert.DeserializeObject<BLExceptionParser>(blExceptionRaw);


            m_msg = new ErrorMessage(bLExceptionParser.Message);
            m_StatusCode = response.StatusCode;

            m_ok = false;

        }

        public bool OK()
        {
            return m_ok;
        }

        public bool FAIL()
        {
            return !m_ok;
        }

        public RawJSON GetJSON()
        {
            return m_json;
        }

        public string GetMsgString()
        {
            if (m_msg != null)
                return m_msg.ToString();
            return "";
        }

        public HttpStatusCode GetHttpStatusCode()
        {
            return m_StatusCode;
        }
        public bool ToLogin()
        {
            if (m_StatusCode != HttpStatusCode.Unauthorized)
                return false;
            return true;
        }

        //public override void ExecuteResult(ControllerContext context)
        //{
        //    throw new NotImplementedException();
        //}
    }
    public class HttpCall
    {
        public static async Task<HttpCallResult> Get(string path,string token, HttpFilter httpFilter)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    if (httpFilter != null)
                        path += httpFilter.ToString();
                    var response = await cliente.GetAsync(path);


                    if (response.IsSuccessStatusCode)
                    {
                        result.SetJson(response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        result.SetMsgSmart(response);
                    }
                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message,HttpStatusCode.InternalServerError);
            }
            return result;
        }
        public static async Task<HttpCallResult> Get(string path,string id,string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    path += "?id=" + HttpUtility.UrlEncode(id);
                
                    var response = await cliente.GetAsync(path);


                    if (response.IsSuccessStatusCode)
                    {
                        result.SetJson(response.Content.ReadAsStringAsync().Result);
                    }
                    else
                        result.SetMsgSmart(response);

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }
        public static async Task<HttpCallResult> Get(string path, string id, string id2, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2);

                    var response = await cliente.GetAsync(path);


                    if (response.IsSuccessStatusCode)
                    {
                        result.SetJson(response.Content.ReadAsStringAsync().Result);
                    }
                    else
                        result.SetMsgSmart(response);

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }
        public static async Task<HttpCallResult> Get(string path, string id, string id2, string id3, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2)
                        + "&id3=" + HttpUtility.UrlEncode(id3);
                       

                    var response = await cliente.GetAsync(path);


                    if (response.IsSuccessStatusCode)
                    {
                        result.SetJson(response.Content.ReadAsStringAsync().Result);
                    }
                    else
                        result.SetMsgSmart(response);

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }
        public static async Task<HttpCallResult> Get(string path, string id, string id2, string id3, string id4, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2)
                        + "&id3=" + HttpUtility.UrlEncode(id3)
                        + "&id4=" + HttpUtility.UrlEncode(id4);

                    var response = await cliente.GetAsync(path);


                    if (response.IsSuccessStatusCode)
                    {
                        result.SetJson(response.Content.ReadAsStringAsync().Result);
                    }
                    else
                        result.SetMsgSmart(response);

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }
        public static async Task<HttpCallResult> Post<T>(string path, T objectToSerialize,string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.PostAsync(path, jsonContent);

                    if (response.IsSuccessStatusCode == false)
                    {
                        result.SetMsgSmart(response);
                    }
                    else
                    {
                        result.SetJson(response.Content.ReadAsStringAsync().Result);
                    }


                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public static async Task<HttpCallResult> Post<T>(string path, T objectToSerialize, string token, Tuple<string,string> additionalHeader)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    if (additionalHeader != null)
                    {
                        cliente.DefaultRequestHeaders.Add(additionalHeader.Item1,additionalHeader.Item2);
                    }

                    var response = await cliente.PostAsync(path, jsonContent);

                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetJson(response.Content.ReadAsStringAsync().Result);


                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public static async Task<HttpCallResult> Put<T>(string path, string id,T objectToSerialize,string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.PutAsync(path, jsonContent);
                    if (response.IsSuccessStatusCode == false)
                    {
                        result.SetMsgSmart(response);
                    }
                    else
                        result.SetOK();

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }
        public static async Task<HttpCallResult> Put<T>(string path, string id,string id2, T objectToSerialize, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.PutAsync(path, jsonContent);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetOK();

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;

        }
        public static async Task<HttpCallResult> Put<T>(string path, string id, T objectToSerialize, string token, Tuple<string, string> additionalHeader)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

                using (var cliente = new HttpClient())
                {
                    if (additionalHeader != null)
                    {
                        cliente.DefaultRequestHeaders.Add(additionalHeader.Item1, additionalHeader.Item2);
                    }

                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.PutAsync(path, jsonContent);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetOK();

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public static async Task<HttpCallResult> Put<T>(string path, string id, string id2, T objectToSerialize, string token, Tuple<string, string> additionalHeader)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {
                path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2);                

                var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

                using (var cliente = new HttpClient())
                {
                    if (additionalHeader != null)
                    {
                        cliente.DefaultRequestHeaders.Add(additionalHeader.Item1, additionalHeader.Item2);
                    }

                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.PutAsync(path, jsonContent);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsg(response.ReasonPhrase, response.StatusCode);
                    else
                        result.SetOK();

                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public static async Task<HttpCallResult> Delete(string path, string id, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id);

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.DeleteAsync(path);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetOK();


                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;
        }
        public static async Task<HttpCallResult> Delete(string path, string id, string id2, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2);

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.DeleteAsync(path);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetOK();


                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;

        }

        public static async Task<HttpCallResult> Delete(string path, string id, string id2, string id3, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2) + "&id3=" + HttpUtility.UrlEncode(id3);

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.DeleteAsync(path);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetOK();


                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;

        }
        public static async Task<HttpCallResult> Delete(string path, string id, string id2, string id3, string id4, string token)
        {
            HttpCallResult result = new HttpCallResult();
            try
            {

                path += "?id=" + HttpUtility.UrlEncode(id) + "&id2=" + HttpUtility.UrlEncode(id2) + "&id3=" + HttpUtility.UrlEncode(id3) + "&id4=" + HttpUtility.UrlEncode(id4);

                using (var cliente = new HttpClient())
                {
                    cliente.BaseAddress = new Uri(path);
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await cliente.DeleteAsync(path);
                    if (response.IsSuccessStatusCode == false)
                        result.SetMsgSmart(response);
                    else
                        result.SetOK();


                }
            }
            catch (System.Exception exception)
            {
                result.SetMsg(exception.Message, HttpStatusCode.InternalServerError);
            }

            return result;

        }
    }

}