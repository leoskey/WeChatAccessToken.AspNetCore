using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeChatAccessToken.Web.Models;
using WeChatAccessToken.Web.Services;

namespace WeChatAccessToken.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeChatController : ControllerBase
    {
        private readonly IWeChatApplicationService _weChatApplicationService;

        public WeChatController(
            IWeChatApplicationService weChatApplicationService)
        {
            _weChatApplicationService = weChatApplicationService;
        }

        /// <summary>
        ///     获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet("{appId}/token")]
        public async Task<AccessTokenDto> GetByAppIdAsync(string appId)
        {
            return await _weChatApplicationService.GetByAppIdAsync(appId);
        }

        /// <summary>
        ///     删除缓存
        /// </summary>
        /// <param name="appId"></param>
        [HttpPost("{appId}/clear")]
        public async Task Clear(string appId)
        {
            await _weChatApplicationService.RemoveAsync(appId);
        }
    }
}