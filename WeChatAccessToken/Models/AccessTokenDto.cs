namespace WeChatAccessToken.Web.Models
{
    public class AccessTokenDto
    {
        public AccessTokenDto(string accessToken)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; set; }
    }
}