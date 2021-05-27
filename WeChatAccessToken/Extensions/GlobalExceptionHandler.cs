using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WeChatAccessToken.Exceptions;

namespace WeChatAccessToken.Extensions
{
    public static class GlobalExceptionHandler
    {
        public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (UserFriendlyException e)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        e.Message
                    });
                }
            });
        }
    }
}