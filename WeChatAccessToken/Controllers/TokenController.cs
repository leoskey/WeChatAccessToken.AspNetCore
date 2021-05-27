using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeChatAccessToken.Web.Models;
using WeChatAccessToken.Web.Services;

namespace WeChatAccessToken.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
        private readonly IWeChatApplicationService _weChatApplicationService;

        public TokenController(
            IOptionsMonitor<AppSettings> optionsMonitor,
            IWeChatApplicationService weChatApplicationService)
        {
            _optionsMonitor = optionsMonitor;
            _weChatApplicationService = weChatApplicationService;
        }

        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet("{appId}")]
        public async Task<AccessTokenDto> GetByAppIdAsync([FromRoute] string appId, [FromQuery] string token)
        {
            if (!_optionsMonitor.CurrentValue.ApiToken.Equals(token))
            {
                throw new UserFriendlyException("token无效");
            }

            return await _weChatApplicationService.GetAccessTokenByAppIdAsync(appId);
        }

        /// <summary>
        /// 重置 access_token
        /// </summary>
        /// <param name="appId"></param>
        [HttpPost("{appId}/reset")]
        public async Task<AccessTokenDto> Reset([FromRoute] string appId, [FromQuery] string token)
        {
            if (!_optionsMonitor.CurrentValue.ApiToken.Equals(token))
            {
                throw new UserFriendlyException("token无效");
            }

            return await _weChatApplicationService.ResetAccessTokenAsync(appId);
        }
    }
}