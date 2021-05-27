using System;

namespace WeChatAccessToken.Web.Models
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException(string message) : base(message)
        {
        }
    }
}