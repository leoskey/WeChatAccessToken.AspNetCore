using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeChatAccessToken.AspNetCore.Models;

namespace WeChatAccessToken.AspNetCore.Services
{
    public class WeChatApplicationService : IWeChatApplicationService
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<WeChatServiceOptions> _optionsMonitor;

        public WeChatApplicationService(
            IDistributedCache cache,
            IOptionsMonitor<WeChatServiceOptions> optionsMonitor,
            IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _optionsMonitor = optionsMonitor;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// 获取 access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// <exception cref="WeChatException"></exception>
        public async Task<AccessTokenResult> GetAccessTokenByAppIdAsync(string appId)
        {
            var cacheKey = GetCacheKey(appId);

            var accessToken = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return JsonConvert.DeserializeObject<AccessTokenResult>(accessToken);
            }

            throw new WeChatException("access token不存在");
        }

        /// <summary>
        /// 重置 access_token 并缓存
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task<AccessTokenResult> ResetAccessTokenAsync(string appId)
        {
            var weChatOption = GetWeChatAccount(appId);
            var cacheKey = GetCacheKey(appId);
            var accessTokenResult = await GetAccessTokenAsync(weChatOption);
            accessTokenResult.expiration_time =
                DateTimeOffset.UtcNow.AddSeconds(accessTokenResult.expires_in).ToUnixTimeSeconds();
            await _cache.SetStringAsync(
                cacheKey,
                JsonConvert.SerializeObject(accessTokenResult),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(accessTokenResult.expires_in - 10)
                });
            return await GetAccessTokenByAppIdAsync(appId);
        }

        /// <summary>
        /// 申请 accessToken
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="WeChatException"></exception>
        private async Task<AccessTokenResult> GetAccessTokenAsync(WeChatAccount app)
        {
            var requestUri = app.Type == WeChatAccountType.Official
                ? $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={app.AppId}&secret={app.AppSecret}"
                : app.Type == WeChatAccountType.WeWork
                    ? $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={app.AppId}&corpsecret={app.AppSecret}"
                    : throw new WeChatException($"{app.AppId}未设置正确的类型:{app.Type}");

            var client = _httpClientFactory.CreateClient();
            var result = await client.GetStringAsync(
                requestUri);
            var accessTokenResult = JsonConvert.DeserializeObject<AccessTokenResult>(result);
            if (accessTokenResult.errcode != 0)
            {
                throw new WeChatException(accessTokenResult.errcode, accessTokenResult.errmsg);
            }

            return accessTokenResult;
        }

        /// <summary>
        /// 获取缓存 key
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private string GetCacheKey(string appId)
        {
            var app = GetWeChatAccount(appId);
            return $"wechat:access_token:{app.AppId}";
        }

        /// <summary>
        /// 获取 wechat 配置
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        /// <exception cref="WeChatException"></exception>
        private WeChatAccount GetWeChatAccount(string appId)
        {
            appId = appId.Trim();
            var wechat = _optionsMonitor.CurrentValue.WeChats
                .FirstOrDefault(t => t.AppId.Equals(appId));
            if (wechat != null)
            {
                return wechat;
            }

            throw new WeChatException("无效的 appid");
        }
    }
}