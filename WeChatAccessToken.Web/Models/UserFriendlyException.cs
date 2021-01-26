using System;

namespace WeChatAccessToken.Web.Models
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException(string code, string? message) : base(message)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}