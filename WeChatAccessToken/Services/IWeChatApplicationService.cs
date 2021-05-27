using System.Threading.Tasks;
using WeChatAccessToken.Web.Models;

namespace WeChatAccessToken.Web.Services
{
    public interface IWeChatApplicationService
    {
        /// <summary>
        /// 获取缓存 key
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        string GetCacheKey(string appId);

        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<AccessTokenDto> GetAccessTokenByAppIdAsync(string appId);

        /// <summary>
        /// 重置 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        Task<AccessTokenDto> ResetAccessTokenAsync(string appId);
    }
}