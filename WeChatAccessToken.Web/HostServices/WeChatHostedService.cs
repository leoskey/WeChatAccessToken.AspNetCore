using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeChatAccessToken.Web.Models;
using WeChatAccessToken.Web.Services;

namespace WeChatAccessToken.Web.HostServices
{
    public class WeChatHostedService : IHostedService
    {
        private readonly ILogger<WeChatHostedService> _logger;
        private readonly IOptionsMonitor<AppSettings> _options;
        private readonly IDistributedCache _cache;
        private readonly IWeChatApplicationService _weChatApplicationService;

        public WeChatHostedService(
            ILogger<WeChatHostedService> logger,
            IOptionsMonitor<AppSettings> options,
            IDistributedCache cache,
            IWeChatApplicationService weChatApplicationService)
        {
            _logger = logger;
            _options = options;
            _cache = cache;
            _weChatApplicationService = weChatApplicationService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var wechats = _options.CurrentValue.WeChats;
                foreach (var wechat in wechats)
                {
                    var result = await _weChatApplicationService.GetByAppIdAsync(wechat.AppId);
                    if (result != null)
                    {
                        _logger.LogInformation($"{wechat.AppId}无需更新");
                        continue;
                    }

                    _logger.LogInformation($"{wechat.AppId}更新中");
                    await _weChatApplicationService.ForceUpdateAccessTokenAsync(wechat.AppId);
                    _logger.LogInformation($"{wechat.AppId}更新完毕");

                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}