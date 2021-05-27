using System.Collections.Generic;

namespace WeChatAccessToken.Web.Models
{
    public class AppSettings
    {
        public string ApiToken { get; set; }
        public List<WeChat> WeChats { get; set; }
    }

    public class WeChat
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}