using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeChatAccessToken.AspNetCore.Models;

namespace WeChatAccessToken.AspNetCore.Services
{
    public class WeChatHostedService : BackgroundService
    {
        private readonly ILogger<WeChatHostedService> _logger;
        private readonly IOptionsMonitor<WeChatServiceOptions> _options;
        private readonly IWeChatApplicationService _weChatApplicationService;

        public WeChatHostedService(
            ILogger<WeChatHostedService> logger,
            IOptionsMonitor<WeChatServiceOptions> options,
            IWeChatApplicationService weChatApplicationService)
        {
            _logger = logger;
            _options = options;
            _weChatApplicationService = weChatApplicationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                var weChats = _options.CurrentValue.WeChats;
                foreach (var wechat in weChats)
                {
                    try
                    {
                        _logger.LogInformation($"更新中:{wechat.AppId}");
                        await _weChatApplicationService.ResetAccessTokenAsync(wechat.AppId);
                        _logger.LogInformation($"更新完毕:{wechat.AppId}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"更新异常:{wechat.AppId}");
                    }
                }

                await Task.Delay(7000 * 1000, stoppingToken);
            }
        }
    }
}