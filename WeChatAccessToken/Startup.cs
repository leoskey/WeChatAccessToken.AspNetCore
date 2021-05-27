using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WeChatAccessToken.Web.HostServices;
using WeChatAccessToken.Web.Models;
using WeChatAccessToken.Web.Services;

namespace WeChatAccessToken.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "WeChatAccessToken", Version = "v1"});
            });

            services.Configure<AppSettings>(Configuration);
            services.AddHttpClient();
            services.AddSingleton<IWeChatApplicationService, WeChatApplicationService>();
            services.AddHostedService<WeChatHostedService>();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration["Redis"];
                options.InstanceName = "WeChatAccessToken:";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeChatAccessToken v1"));
            }

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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}