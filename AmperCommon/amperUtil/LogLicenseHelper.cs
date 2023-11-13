using amperUtil.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace amperUtil
{
    public class JTIidentificador : IdentificadorString
    {


        public JTIidentificador(string id) : base(id)
        {
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

    public class JWThelper
    {
        public static TimeSpan GetExpiration(AuthToken token)
        {

            Test(token);
            JwtSecurityToken jwt = new JwtSecurityToken(token.token);
            JwtPayload jwp = jwt.Payload;
            if (jwp.Exp.HasValue && jwp.Iat.HasValue)
            {
                int exp = jwp.Exp.Value - jwp.Iat.Value;
                ++exp;
                return new TimeSpan(0, 0, exp);
            }
            else
            {
                return new TimeSpan(0, 10, 0);
            }
        }

        public static JTIidentificador GetJti(AuthToken token)
        {

            Test(token);
            JwtSecurityToken jwt = new JwtSecurityToken(token.token);
            JwtPayload jwp = jwt.Payload;
            if (jwp.Jti == null)
                return null;
            return new JTIidentificador(jwp.Jti);
        }
        static void Test(AuthToken tokenIn)
        {
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            var jwtInput = tokenIn.token;

            if (jwtHandler.CanReadToken(jwtInput) == false)
            {
                return;
            }

            var token = jwtHandler.ReadJwtToken(jwtInput);

            var headers = token.Header;
            foreach (var h in headers)
            {
                string s = string.Format("\tHeader key[{0}] value[{1}]", h.Key, h.Value);
            }

            var claims = token.Claims;
            foreach (var c in claims)
            {
                string s = string.Format("\tClaim Type[{0}] Value[{1}]", c.Type, c.Value);
            }



        }

    }

    public class UsernamePassword
    {
        public string username { set; get; }
        public string password { set; get; }
        public string profile { set; get; }
    }
    public class SesionLicencias
    {
        public string idSesion { get; set; }
        public List<string> Licencias { get; set; }
    }


    public enum LogLicenseInResultCode { SUCCESS, ERROR_AUTHENTICATION, ERROR_JTI, ERROR_NO_LICENSE, ERROR_AUTHORIZATION , MULTIPLE_PROFILES, NO_PROFILE};
    public enum LogLicenseOutResultCode { SUCCESS, ERROR };

    public class LogLicenseIn
    {
        public string tokenLogout { set; get; }
        public string token { set; get; }
        public string session { set; get; }
        public string username { set; get; }
        public LogLicenseInResultCode code { set; get; }
    }
    public class LogLicenseOut
    {
        public LogLicenseOutResultCode code { set; get; }
    }



    public class LogLicenseHelper
    {
        #region LogLicenseIn
        public async Task<Tuple<bool, string>> LogIn1(UsernamePassword usernamePassword, string AuthUrlSession)
        {
            string upString = JsonConvert.SerializeObject(usernamePassword);

            var jsonContent = new StringContent(upString, Encoding.UTF8, "application/json");

            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri(AuthUrlSession);

                var response = await cliente.PostAsync(AuthUrlSession, jsonContent);

                if (response.IsSuccessStatusCode == false)
                {
                    return new Tuple<bool, string>(false, "");
                }
                else
                {
                    return new Tuple<bool, string>(true, response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        public async Task<Tuple<bool, string>> LicenseIn(string jti,string tokenApplication, string AuthUrlLicenseUse, string license)
        {
            SesionLicencias sesionLicencias = new SesionLicencias
            {
                idSesion = jti,
                Licencias = new List<string> { license }
            };
            string upString = JsonConvert.SerializeObject(sesionLicencias);

            var jsonContent = new StringContent(upString, Encoding.UTF8, "application/json");

            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri(AuthUrlLicenseUse);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenApplication);

                var response = await cliente.PostAsync(AuthUrlLicenseUse, jsonContent);

                if (response.IsSuccessStatusCode == false)
                {
                    return new Tuple<bool, string>(false, "");
                }
                else
                {
                    return new Tuple<bool, string>(true, response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        public async Task<Tuple<bool, string>> LogIn2(string token, string BackUrlLogIn,string profile)
        {
            using (var cliente = new HttpClient())
            {
                if (!string.IsNullOrEmpty(profile))
                    BackUrlLogIn += "?profile=" + profile;
                cliente.BaseAddress = new Uri(BackUrlLogIn);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await cliente.GetAsync(BackUrlLogIn);

                if (response.IsSuccessStatusCode == false)
                {
                    return new Tuple<bool, string>(false, "");
                }
                else
                {
                    return new Tuple<bool, string>(true, response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        #endregion


        #region LogLicenseOut

        public async Task<Tuple<bool, string>> LogOut(string token, string AuthUrlSession)
        {
            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri(AuthUrlSession);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await cliente.DeleteAsync(AuthUrlSession);

                if (response.IsSuccessStatusCode == false)
                {
                    return new Tuple<bool, string>(false, response.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return new Tuple<bool, string>(true, response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        public async Task<bool> LicenseOut(string jti, string tokenApplication, string AuthUrlLicenseFree,string license)
        {
            
            SesionLicencias sesionLicencias = new SesionLicencias
            {
                idSesion = jti,
                Licencias = new List<string> { license }
            };
            string upString = JsonConvert.SerializeObject(sesionLicencias);

            var jsonContent = new StringContent(upString, Encoding.UTF8, "application/json");

            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri(AuthUrlLicenseFree);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenApplication);

                var response = await cliente.PostAsync(AuthUrlLicenseFree, jsonContent);

                if (response.IsSuccessStatusCode == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion
        public async Task<bool> LicenseRefresh(string jti, string tokenApplication, string AuthUrlLicenseRefresh, string license)
        {

            SesionLicencias sesionLicencias = new SesionLicencias
            {
                idSesion = jti,
                Licencias = new List<string> { license }
            };
            string upString = JsonConvert.SerializeObject(sesionLicencias);

            var jsonContent = new StringContent(upString, Encoding.UTF8, "application/json");

            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri(AuthUrlLicenseRefresh);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenApplication);
                cliente.Timeout = new TimeSpan(0, 0, 10);

                var response = await cliente.PostAsync(AuthUrlLicenseRefresh, jsonContent);

                if (response.IsSuccessStatusCode == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

    }
}
