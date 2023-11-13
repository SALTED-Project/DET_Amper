
namespace amperUtil.Auth
{
    public class ProfileCodeWrqpper
    {
        public string ProfileCode { get; set; }
        public string ProfileName { get; set; }

    }
    public class AuthToken
    {
        public string token { get; set; }

        public AuthToken()
        {
        }
        public AuthToken(string tokenIn)
        {
            token = tokenIn;
        }
    }
}
