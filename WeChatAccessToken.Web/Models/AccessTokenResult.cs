namespace WeChatAccessToken.Web.Models
{
    public class AccessTokenResult
    {
        public string access_token { get; set; }

        public int expires_in { get; set; }
        
        public int errcode { get; set; }

        public string errmsg { get; set; }
    }
}