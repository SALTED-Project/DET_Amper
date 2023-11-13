using System;
using System.Linq;
using System.Net.Http;

namespace amperUtil
{
    public class HttpUtils
    {
        public static string GetHeaderFromRequest(HttpRequestMessage request, string headerName)
        {
            string header = string.Empty;
            try
            {
                header = request.Headers.GetValues(headerName).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return header;
        }
    }
}
