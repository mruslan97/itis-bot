using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SheduleBot.AspHost.Commads;
using Telegram.Bot.Framework.Abstractions;

namespace SheduleBot.AspHost
{
    public class Startup
    {
        private IConfigurationRoot configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTelegramBot<ItisSheduleBot>(configuration.GetSection("SheduleBot"))
                .AddUpdateHandler<EchoCommand>()
                .Configure();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            ILogger logger = loggerFactory.CreateLogger<Startup>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Task.Factory.StartNew(async () =>
            {
                
               
                    var botManager = app.ApplicationServices.GetRequiredService<IBotManager<ItisSheduleBot>>();
                    await botManager.SetWebhookStateAsync(false);
                
                // make sure webhook is disabled so we can use long-polling
                

                while (true)
                {
                    try
                    {
                        await botManager.GetAndHandleNewUpdatesAsync();
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception: {e}");
                    }
                    
                }
               
            }).ContinueWith(t =>
            {
                if (t.IsFaulted) throw t.Exception;
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
