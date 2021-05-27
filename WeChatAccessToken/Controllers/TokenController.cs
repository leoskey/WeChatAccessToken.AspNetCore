using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeChatAccessToken.AspNetCore.Models;
using WeChatAccessToken.AspNetCore.Services;
using WeChatAccessToken.Exceptions;

namespace WeChatAccessToken.Controllers
{
    [ApiController]
    [Route("[controller]/{appId}")]
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
        [HttpGet]
        public async Task<AccessTokenResult> GetByAppIdAsync([FromRoute] string appId, [FromQuery] string token)
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
        [HttpPost("reset")]
        public async Task<AccessTokenResult> Reset([FromRoute] string appId, [FromQuery] string token)
        {
            if (!_optionsMonitor.CurrentValue.ApiToken.Equals(token))
            {
                throw new UserFriendlyException("token无效");
            }

            return await _weChatApplicationService.ResetAccessTokenAsync(appId);
        }
    }
}