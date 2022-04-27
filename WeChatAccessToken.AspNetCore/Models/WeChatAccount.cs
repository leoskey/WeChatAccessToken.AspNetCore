namespace WeChatAccessToken.AspNetCore.Models
{
    public class WeChatAccount
    {
        public WeChatAccountType Type { get; set; } = WeChatAccountType.Official;
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}