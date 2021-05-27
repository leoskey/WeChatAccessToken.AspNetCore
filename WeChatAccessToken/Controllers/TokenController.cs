using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeChatAccessToken.Web.Models;
using WeChatAccessToken.Web.Services;

namespace WeChatAccessToken.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IWeChatApplicationService _weChatApplicationService;

        public TokenController(
            IWeChatApplicationService weChatApplicationService)
        {
            _weChatApplicationService = weChatApplicationService;
        }

        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet("{appId}")]
        public async Task<AccessTokenDto> GetByAppIdAsync(string appId)
        {
            return await _weChatApplicationService.GetAccessTokenByAppIdAsync(appId);
        }

        /// <summary>
        /// 重置 access_token
        /// </summary>
        /// <param name="appId"></param>
        [HttpPost("{appId}/reset")]
        public async Task<AccessTokenDto> Reset(string appId)
        {
            return await _weChatApplicationService.ResetAccessTokenAsync(appId);
        }
    }
}