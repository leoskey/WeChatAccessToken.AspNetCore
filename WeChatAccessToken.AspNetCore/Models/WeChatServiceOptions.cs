using System.Collections.Generic;

namespace WeChatAccessToken.AspNetCore.Models
{
    public class WeChatServiceOptions
    {
        public List<WeChatAccount> WeChats { get; set; }
    }

    public class WeChatAccount
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}