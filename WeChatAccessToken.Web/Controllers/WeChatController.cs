using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeChatAccessToken.Web.Models;

namespace WeChatAccessToken.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeChatController : ControllerBase
    {
        private readonly ILogger<WeChatController> _logger;
        private readonly IDistributedCache _cache;
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
        private readonly IHttpClientFactory _httpClientFactory;

        public WeChatController(
            ILogger<WeChatController> logger,
            IDistributedCache cache,
            IOptionsMonitor<AppSettings> optionsMonitor,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _cache = cache;
            _optionsMonitor = optionsMonitor;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        [HttpGet("{appId}/token")]
        public async Task<IActionResult> GetByAppId(string appId)
        {
            var key = GetKey(appId);

            var accessToken = await _cache.GetStringAsync(key);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return Ok(new AccessTokenDto(accessToken));
            }

            if (Monitor.TryEnter(key))
            {
                try
                {
                    var app = GetWeChat(appId);
                    var client = _httpClientFactory.CreateClient();
                    var result = await client.GetStringAsync(
                        $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={app.AppId}&secret={app.AppSecret}");
                    var json = JsonConvert.DeserializeObject<AccessTokenResult>(result);
                    if (json.errcode != 0)
                    {
                        return NotFound(new UserFriendlyException("wechat_error", json.errmsg));
                    }

                    accessToken = json.access_token;
                    await _cache.SetStringAsync(key, accessToken, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(json.expires_in - 10)
                    });

                    return Ok(new AccessTokenDto(accessToken));
                }
                finally
                {
                    Monitor.Exit(key);
                }
            }

            Monitor.Wait(key);
            accessToken = await _cache.GetStringAsync(key);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return Ok(new AccessTokenDto(accessToken));
            }

            return NotFound(new UserFriendlyException("internal_error", "获取失败"));
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="appId"></param>
        [HttpPost("{appId}/clear")]
        public async Task Clear(string appId)
        {
            var key = GetKey(appId);
            await _cache.RemoveAsync(key);
        }

        private string GetKey(string appId)
        {
            var app = GetWeChat(appId);
            return $"wechat:access_token:{app.AppId}:{app.AppSecret}";
        }

        private WeChat GetWeChat(string appId)
        {
            appId = appId.Trim();
            var app = _optionsMonitor.CurrentValue.WeChats
                .FirstOrDefault(t => t.AppId.Equals(appId));
            if (app == null)
            {
                throw new UserFriendlyException("invalid_appid", "无效的 appid");
            }

            return app;
        }
    }
}