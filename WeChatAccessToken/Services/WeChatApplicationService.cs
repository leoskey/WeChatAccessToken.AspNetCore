using System;
using System.Linq;
using System.Net.Http;
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
        private readonly ILogger<TokenController> _logger;
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public WeChatApplicationService(
            ILogger<TokenController> logger,
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
        public async Task<AccessTokenDto> GetAccessTokenByAppIdAsync(string appId)
        {
            var cacheKey = GetCacheKey(appId);

            var accessToken = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return new AccessTokenDto(accessToken);
            }

            throw new UserFriendlyException("未获取access token");

            // if (Monitor.TryEnter(key))
            //     try
            //     {
            //         var app = GetWeChat(appId);
            //         var accessTokenResult = await GetAccessTokenAsync(app);
            //         await _cache.SetStringAsync(key, accessTokenResult.access_token, new DistributedCacheEntryOptions
            //         {
            //             AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(accessTokenResult.expires_in - 10)
            //         });
            //         return new AccessTokenDto(accessToken);
            //     }
            //     finally
            //     {
            //         Monitor.Exit(key);
            //     }
            //
            // Monitor.Wait(key);
            // accessToken = await _cache.GetStringAsync(key);
            // return !string.IsNullOrWhiteSpace(accessToken)
            //     ? new AccessTokenDto(accessToken)
            //     : throw new UserFriendlyException("internal_error", "获取失败");
        }

        /// <summary>
        /// 更新并缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<AccessTokenDto> ResetAccessTokenAsync(string appId)
        {
            var weChatOption = GetWeChatOption(appId);
            var cacheKey = GetCacheKey(appId);
            var accessTokenResult = await GetAccessTokenAsync(weChatOption);
            await _cache.SetStringAsync(cacheKey, accessTokenResult.access_token, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(accessTokenResult.expires_in - 10)
            });
            return await GetAccessTokenByAppIdAsync(appId);
        }

        /// <summary>
        /// 获取缓存 key
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public string GetCacheKey(string appId)
        {
            var app = GetWeChatOption(appId);
            return $"wechat:access_token:{app.AppId}";
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
            var accessTokenResult = JsonConvert.DeserializeObject<AccessTokenResult>(result);
            if (accessTokenResult.errcode != 0)
            {
                throw new UserFriendlyException(accessTokenResult.errmsg);
            }

            return accessTokenResult;
        }

        /// <summary>
        /// 获取 wechat 配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        private WeChat GetWeChatOption(string appId)
        {
            appId = appId.Trim();
            var app = _optionsMonitor.CurrentValue.WeChats
                .FirstOrDefault(t => t.AppId.Equals(appId));
            if (app == null) throw new UserFriendlyException("无效的 appid");

            return app;
        }
    }
}