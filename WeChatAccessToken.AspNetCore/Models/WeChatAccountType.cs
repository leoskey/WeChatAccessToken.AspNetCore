using System.ComponentModel;

namespace WeChatAccessToken.AspNetCore.Models
{
    public enum WeChatAccountType
    {
        [Description("公众号")]
        Official = 1,

        [Description("企业微信")]
        WeWork = 2
    }
}