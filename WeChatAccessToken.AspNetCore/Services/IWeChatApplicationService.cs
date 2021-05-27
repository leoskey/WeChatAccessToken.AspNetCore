using System.Threading.Tasks;
using WeChatAccessToken.AspNetCore.Models;

namespace WeChatAccessToken.AspNetCore.Services
{
    public interface IWeChatApplicationService
    {
        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<AccessTokenResult> GetAccessTokenByAppIdAsync(string appId);

        /// <summary>
        /// 重置 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<AccessTokenResult> ResetAccessTokenAsync(string appId);
    }
}