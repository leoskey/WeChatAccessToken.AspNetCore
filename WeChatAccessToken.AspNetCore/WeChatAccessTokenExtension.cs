using Microsoft.Extensions.DependencyInjection;
using WeChatAccessToken.AspNetCore.Services;

namespace WeChatAccessToken.AspNetCore
{
    public static class WeChatAccessTokenExtension
    {
        public static IServiceCollection AddWeChatAccessTokenService(
            this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IWeChatApplicationService, WeChatApplicationService>();
            services.AddHostedService<WeChatHostedService>();

            return services;
        }
    }
}