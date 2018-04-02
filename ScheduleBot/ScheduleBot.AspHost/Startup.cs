using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MagicParser.Impls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads;
using ScheduleBot.AspHost.Commads.GetScheduleCommands;
using ScheduleBot.AspHost.Commads.SetUpCommands;
using ScheduleBot.AspHost.Keyboards;
using ScheduleBot.AspHost.Updating;
using ScheduleServices.Core;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using ScheduleServices.Core.Providers.Storage;
using Telegram.Bot.Framework.Abstractions;

namespace ScheduleBot.AspHost
{
    public class Startup
    {
        private IConfigurationRoot configuration;
        private UpdatesScheduler updates;

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
            //schedules upd
            services.AddTransient<UpdateJob>();
            //service
            services.AddTransient<ISchElemsFactory, DefaultSchElemsFactory>();
            services.AddTransient<IGroupsMonitor, GroupsMonitor>(provider => new GroupsMonitor(GetGroupsList()));
            services.AddTransient<IScheduleInfoProvider, ScheduleInfoProvider>();
            services.AddTransient<ISchedulesStorage, SchedulesInMemoryDbStorage>();
            services.AddSingleton<IScheduleServise, ScheduleService>();
            services.AddSingleton<IBotDataStorage, InMemoryBotStorage>();
            services.AddSingleton<KeyboardsFactory>(provider => new KeyboardsFactory(GetGroupsList()));
            services.AddTelegramBot<ItisScheduleBot>(configuration.GetSection("ScheduleBot"))
                .AddUpdateHandler<EchoCommand>()
                .AddUpdateHandler<SetUpGroupCommand>()
                .AddUpdateHandler<GetForTodayCommand>()
                .AddUpdateHandler<GetForTomorrowCommand>()
                .AddUpdateHandler<GetForWeekCommand>()
                .AddUpdateHandler<HelloCommand>()
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
                var botManager = app.ApplicationServices.GetRequiredService<IBotManager<ItisScheduleBot>>();
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

            updates = new UpdatesScheduler(app.ApplicationServices);
            updates.Start();

            app.Run(async (context) => { });
        }

        private IList<IScheduleGroup> GetGroupsList()
        {
            return new List<IScheduleGroup>()
            {
                //template
                /*
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-01"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-02"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-03"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-04"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-05"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-06"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-07"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-08"},
                */
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-401"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-402"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-403"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-404"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-405"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-406"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-407"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-408"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-501"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-502"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-503"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-504"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-505"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-506"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-507"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-508"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-601"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-602"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-603"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-604"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-605"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-606"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-607"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-608"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-701"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-702"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-703"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-704"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-705"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-706"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-707"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-708"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-709"},
            };
        }
    }
}