using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using amperUtil.Auth;
using amperUtil.Log;

namespace amperUtil
{
    class HttpClientREST
    {
        public HttpClient clt;
        public HttpClient rclient;

        public static string svUser { get; set; }
        public static string svPwd { get; set; }
        public static string svScope { get; set; }
        public static string svClientId { get; set; }
        public static string svClientSecret { get; set; }
        public static string svURL { get; set; }
        public HttpClientREST(string usr, string pwd, string scope, string cId, string cSecret, string url)
        {
            clt = new HttpClient();
            rclient = new HttpClient();

            svUser = usr;
            svPwd = pwd;
            svScope = scope;
            svClientId = cId;
            svClientSecret = cSecret;
            svURL = url;

        }

        public void ConnectToken(AuthToken token, AuthToken rToken)
        {
            clt.DefaultRequestHeaders.Accept.Clear();
            clt.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            clt.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

            Dictionary<string, string> reqProperties = new Dictionary<string, string>();
            reqProperties.Add("grant_type", "password");
            reqProperties.Add("username", svUser);
            reqProperties.Add("password", svPwd);
            reqProperties.Add("scope", svScope);
            reqProperties.Add("client_id", svClientId);
            reqProperties.Add("client_secret", svClientSecret);

            string authURL = svURL + "/OAuth/token";

            FormUrlEncodedContent content = new FormUrlEncodedContent(reqProperties);

            //Send authentication request
            SendPostRequest(svURL, content, token, rToken);
        }

        public void RefreshToken(AuthToken atoken, AuthToken rToken)
        {
            clt.DefaultRequestHeaders.Accept.Clear();
            clt.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            clt.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

            Dictionary<string, string> reqProperties = new Dictionary<string, string>();
            reqProperties.Add("grant_type", "refresh_token");
            reqProperties.Add("client_id", svClientId);
            reqProperties.Add("client_secret", svClientSecret);
            reqProperties.Add("refresh_token", rToken.token);

            string authURL = svURL + "/OAuth/token";

            FormUrlEncodedContent content = new FormUrlEncodedContent(reqProperties);

            //Send authentication request
            SendPostRequest(authURL, content, atoken, rToken);
        }

        public void DisconnectToken(AuthToken atoken)
        {
            string url = string.Empty;

            rclient.DefaultRequestHeaders.Accept.Clear();
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
            rclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", atoken.token);

            url = svURL + "/Oauth/Account/Logout";

            SendGetRequestLogout(url);
        }
        
        private async void SendPostRequest(string requestUrl, HttpContent content, AuthToken tkn, AuthToken rTkn)
        {
            string result = string.Empty;

            try
            {
                //Send request and wait for response
                HttpResponseMessage response = await clt.PostAsync(requestUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string payload = await response.Content.ReadAsStringAsync();
                    result = "OK: \n" + payload;

                    //get token from JSON message
                    JObject jo = JObject.Parse(payload);
                    tkn.token = jo["access_token"].ToString();
                    rTkn.token = jo["refresh_token"].ToString();
                }
                else
                {
                    result = "NOK: \n\"" + "StatusCode= " + response.StatusCode + "\n\"" +
                        "Reason: " + response.ReasonPhrase + "\n\"";
                }
            }
            catch (Exception ex)
            {
                result = "Exception getting authorization: \n\"" + ex.Message + "\"";
            }
        }

        private async void SendGetRequestLogout(string requestUrl)
        {
            HttpResponseMessage response = await rclient.GetAsync(requestUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string payload = await response.Content.ReadAsStringAsync();
            }
            else
            {
                string result = "NOK: \n\"" + "StatusCode= " + response.StatusCode + "\n\"" +
                    "Reason: " + response.ReasonPhrase + "\n\"";
            }
        }

        public void ReadRTVariable(string variable, AuthToken atoken)
        {
            string url = string.Empty;

            rclient.DefaultRequestHeaders.Accept.Clear();
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
            rclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", atoken.token);

            string varname = variable.Replace('.', '/');
            string valor = string.Empty;
            url = string.Format(svURL + "/RealtimeData/v2/Values/{0}/", varname);

            SendGetRequest(url, valor);
        }
        public void WriteRTVariable(string variable, AuthToken atoken)
        {
            string url = string.Empty;

            rclient.DefaultRequestHeaders.Accept.Clear();
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
            rclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", atoken.token);

            Dictionary<string, string> reqProperties = new Dictionary<string, string>();
            reqProperties.Add("value", variable);
            FormUrlEncodedContent content = new FormUrlEncodedContent(reqProperties);

            string varname = variable.Replace('.', '/');
            url = string.Format(svURL + "/RealtimeData/v2/Values/{0}/", varname);

            SendWritePostRequest(url, content);
        }

        private async void SendGetRequest(string requestUrl, string val)
        {
            HttpResponseMessage response = await rclient.GetAsync(requestUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string payload = await response.Content.ReadAsStringAsync();
                val = payload;

                //get token from JSON message
                JObject jo = JObject.Parse(payload);
                val = jo.First.First["value"].ToString();
            }
            else
            {
                string error = "NOK: \n\"" + "StatusCode= " + response.StatusCode + "\n\"" +
                    "Reason: " + response.ReasonPhrase + "\n\"";
            }
        }
        private async void SendWritePostRequest(string requestUrl, HttpContent content)
        {
            HttpResponseMessage response = await clt.PostAsync(requestUrl, content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string payload = await response.Content.ReadAsStringAsync();
                string result = payload;

            }
            else
            {
                string result = "NOK: \n\"" + "StatusCode= " + response.StatusCode + "\n\"" +
                    "Reason: " + response.ReasonPhrase + "\n\"";
            }

        }

        private void SubscribeToRTVariables(AuthToken atoken, List<string> miList)
        {
            rclient.DefaultRequestHeaders.Accept.Clear();
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
            rclient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", atoken.token);

            Dictionary<string, string> reqProperties = new Dictionary<string, string>();
            foreach (string v in miList)
            {
                reqProperties.Add("value", v);
            }
            FormUrlEncodedContent content = new FormUrlEncodedContent(reqProperties);

            string url = string.Empty;
            url = string.Format(svURL + "/RealtimeData/v2/Subscriptions/{0}/", miList);

            SendSubscribePostRequest(url, content);
        }

        private async void SendSubscribePostRequest(string url, HttpContent content)
        {
            HttpResponseMessage response = await clt.PostAsync(url, content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string payload = await response.Content.ReadAsStringAsync();
                string result = payload;

            }
            else
            {
                string result = "NOK: \n\"" + "StatusCode= " + response.StatusCode + "\n\"" +
                    "Reason: " + response.ReasonPhrase + "\n\"";
            }
        }

    }
}
