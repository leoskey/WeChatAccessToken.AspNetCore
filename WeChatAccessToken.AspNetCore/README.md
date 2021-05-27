# 快速使用

Startup.cs

```c#
public void ConfigureServices(IServiceCollection services) {
    // 配置微信 AppId 以及 AppSecret
    services.Configure<WeChatServiceOptions>(Configuration);
    // 注册服务
    services.AddWeChatAccessTokenService();
    // 注册缓存
    services.AddDistributedMemoryCache();
}
```

appsettings.json

```json
{
  "WeChats": [
    {
      "AppId": "AppId",
      "AppSecret": "AppSecret"
    }
  ]
}
```

TokenController.cs

```c#
[ApiController]
[Route("[controller]/{appId}")]
public class TokenController : ControllerBase
{
    private readonly IWeChatApplicationService _weChatApplicationService;

    public TokenController(
        IWeChatApplicationService weChatApplicationService)
    {
        _weChatApplicationService = weChatApplicationService;
    }

    /// <summary>
    /// 获取 access_token
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    public async Task<AccessTokenResult> GetByAppIdAsync([FromRoute] string appId)
    {
        return await _weChatApplicationService.GetAccessTokenByAppIdAsync(appId);
    }

    /// <summary>
    /// 重置 access_token
    /// </summary>
    /// <param name="appId"></param>
    [HttpPost("reset")]
    public async Task<AccessTokenResult> Reset([FromRoute] string appId)
    {
        return await _weChatApplicationService.ResetAccessTokenAsync(appId);
    }
}
```

接下来，就可以通过 http 接口来获取或重置 access token

# 缓存方式

兼容 asp.net core 缓存

可参考文档：https://docs.microsoft.com/zh-cn/aspnet/core/performance/caching/response

## 示例

```c#
// 使用内存缓存
services.AddDistributedMemoryCache();

// 使用redis缓存
services.AddDistributedRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "WeChatAccessToken:";
});
```