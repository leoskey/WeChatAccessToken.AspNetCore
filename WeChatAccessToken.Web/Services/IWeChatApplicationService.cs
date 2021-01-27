using System.Threading.Tasks;
using WeChatAccessToken.Web.Models;

namespace WeChatAccessToken.Web.Services
{
    public interface IWeChatApplicationService
    {
        Task<AccessTokenDto> GetByAppIdAsync(string appId);
        Task RemoveAsync(string appId);
        Task ForceUpdateAccessTokenAsync(string wechatAppId);
    }
}