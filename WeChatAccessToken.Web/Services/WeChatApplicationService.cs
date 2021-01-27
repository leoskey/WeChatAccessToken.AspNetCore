using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeChatAccessToken.Web.Controllers;
using WeChatAccessToken.Web.Models;

namespace WeChatAccessToken.Web.Services
{
    public class WeChatApplicationService : IWeChatApplicationService
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WeChatController> _logger;
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public WeChatApplicationService(
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
        public async Task<AccessTokenDto> GetByAppIdAsync(string appId)
        {
            var key = GetKey(appId);

            var accessToken = await _cache.GetStringAsync(key);
            if (!string.IsNullOrWhiteSpace(accessToken)) return new AccessTokenDto(accessToken);

            if (Monitor.TryEnter(key))
                try
                {
                    var app = GetWeChat(appId);
                    var accessTokenResult = await GetAccessTokenAsync(app);
                    await _cache.SetStringAsync(key, accessTokenResult.access_token, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(accessTokenResult.expires_in - 10)
                    });
                    return new AccessTokenDto(accessToken);
                }
                finally
                {
                    Monitor.Exit(key);
                }

            Monitor.Wait(key);
            accessToken = await _cache.GetStringAsync(key);
            return !string.IsNullOrWhiteSpace(accessToken)
                ? new AccessTokenDto(accessToken)
                : throw new UserFriendlyException("internal_error", "获取失败");
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="appId"></param>
        public async Task RemoveAsync(string appId)
        {
            var key = GetKey(appId);
            await _cache.RemoveAsync(key);
        }

        /// <summary>
        /// 更新并缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task ForceUpdateAccessTokenAsync(string appId)
        {
            var app = GetWeChat(appId);
            var key = GetKey(appId);
            var accessTokenResult = await GetAccessTokenAsync(app);
            await _cache.SetStringAsync(key, accessTokenResult.access_token, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(accessTokenResult.expires_in - 10)
            });
        }

        /// <summary>
        /// 申请 accessToken
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        private async Task<AccessTokenResult> GetAccessTokenAsync(WeChat app)
        {
            var client = _httpClientFactory.CreateClient();
            var result = await client.GetStringAsync(
                $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={app.AppId}&secret={app.AppSecret}");
            var json = JsonConvert.DeserializeObject<AccessTokenResult>(result);
            if (json.errcode != 0) throw new UserFriendlyException("wechat_error", json.errmsg);

            return json;
        }

        /// <summary>
        /// 获取缓存 key
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public string GetKey(string appId)
        {
            var app = GetWeChat(appId);
            return $"wechat:access_token:{app.AppId}:{app.AppSecret}";
        }

        /// <summary>
        ///     获取 wechat 配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        private WeChat GetWeChat(string appId)
        {
            appId = appId.Trim();
            var app = _optionsMonitor.CurrentValue.WeChats
                .FirstOrDefault(t => t.AppId.Equals(appId));
            if (app == null) throw new UserFriendlyException("invalid_appid", "无效的 appid");

            return app;
        }
    }
}