using System;

namespace WeChatAccessToken.AspNetCore.Models
{
    public class WeChatException : Exception
    {
        public int Code { get; }

        public WeChatException(string message) : base(message)
        {
        }

        public WeChatException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}