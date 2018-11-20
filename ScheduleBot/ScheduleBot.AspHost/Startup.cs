using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoogleSheetsSchedulesProvider.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads;
using ScheduleBot.AspHost.Commads.GetScheduleCommands;
using ScheduleBot.AspHost.Commads.SetUpCommands;
using ScheduleBot.AspHost.Commads.SetUpCommands.ElectivesSetUpCommands;
using ScheduleBot.AspHost.Commads.TeacherSearchCommands;
using ScheduleBot.AspHost.DAL;
using ScheduleBot.AspHost.DAL.Repositories.Impls;
using ScheduleBot.AspHost.DAL.Repositories.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using ScheduleBot.AspHost.Updating;
using ScheduleServices.Core.Config;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using TableRules.Core;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace ScheduleBot.AspHost
{
    public class Startup
    {
        private IConfigurationRoot configuration;
        private UpdatesScheduler updates;
        //link to store groups - see usages
        private readonly List<IScheduleGroup> presettedGroups = new List<IScheduleGroup>();

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
            var currentSetup = new SchedulesSetup();
            services.AddSingleton<SchedulesSetup>(currentSetup);
            //DAL
            services.AddEntityFrameworkNpgsql().AddDbContext<UsersContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("MainDatabase"))
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                );
            services.AddSingleton<UsersContextFactory>();
            services.AddTransient<IUsersGroupsRepository, UsersGroupsDbRepository>();
            //configure parser
            //todo: move to extension? ugly config flow
            services.AddSingleton<IEnumerable<ICellRule>>(currentSetup.GetCellHandlers());
            services.AddGoogleApiParser(configuration.GetSection("GoogleApi"));
            //configure schedule service core
            services.AddDefaultScheduleServiceCore(presettedGroups, currentSetup.GetGroupsRules());
            //update jobs
            services.AddTransient<UpdateJob>();
            services.AddSingleton<UpdateTeachersListJob>();
            services.AddSingleton<ITeachersSource>(prov => prov.GetRequiredService<UpdateTeachersListJob>());
            //services about bot infrastruct
            services.AddTransient<TeacherScheduleSelector>();
            services.AddTransient<IScheduleEventArgsFactory, DefaultEventArgsFactory>();
            services.AddSingleton<IBotDataStorage, InMemoryBotStorage>();
            services.AddSingleton<INotifiactionSender, Notificator>();
            services.AddSingleton<IKeyboardsFactory>(provider => new KeyboardsFactory(presettedGroups));
            services.AddTransient(prov =>
                configuration.GetSection("NotificationsSecret").Get<DistributionCommand.SecretKey>());
            services.AddTelegramBot<ItisScheduleBot>(configuration.GetSection("ScheduleBot"))
                //organize
                .AddUpdateHandler<DistributionCommand>()
                .AddUpdateHandler<EchoCommand>()
                .AddUpdateHandler<HelloCommand>()
                .AddUpdateHandler<SettingsOptionsCommand>()
                .AddUpdateHandler<SettingsBackCommand>()
                .AddUpdateHandler<NotFoundGroupCommand>()
                .AddUpdateHandler<WeekIsEvenCommand>()
                //get schedules
                .AddUpdateHandler<GetForTodayCommand>()
                .AddUpdateHandler<GetForTomorrowCommand>()
                .AddUpdateHandler<GetForWeekCommand>()
                .AddUpdateHandler<GetTeacherScheduleCommand>()
                //academic
                .AddUpdateHandler<ChangeAcademicGroupCommand>()
                .AddUpdateHandler<SetUpCourseCommand>()
                .AddUpdateHandler<SetUpAcademicGroupCommand>()
                //teachers get and upd inlines
                .AddUpdateHandler<UpdTeachersListCommand>()
                .AddUpdateHandler<GetTeachersListCommand>()
                //electives get
                .AddUpdateHandler<GetEngGroupsCommand>()
                .AddUpdateHandler<GetTechGroupsCommand>()
                .AddUpdateHandler<GetScienticGroupsCommand>()
                //electives set
                .AddUpdateHandler<SetUpEngGroupCommand>()
                .AddUpdateHandler<SetUpTechGroupCommand>()
                .AddUpdateHandler<SetUpScienticGroupCommand>()
                //
                .AddUpdateHandler<ForDevelopersCommand>()
                .AddUpdateHandler<TestCommand>()
                .Configure();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            
            ILogger logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation("Configuration started");
            logger.LogInformation($"Env: {env.EnvironmentName}");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            logger.LogInformation($"Bot up");

            //DAL preparations
            var dbcontext = app.ApplicationServices.GetRequiredService<UsersContext>();
            dbcontext.Database.Migrate();

            //update presettedGroups before any ctors have been called
            presettedGroups.AddRange(app.ApplicationServices.GetRequiredService<SchedulesSetup>().GetGroups());
            var repo = app.ApplicationServices.GetRequiredService<IUsersGroupsRepository>();
            repo.SyncGroupsFromSource(presettedGroups).Wait();


            if (configuration.GetSection("UseWebHook").Get<bool>())
            {
                app.UseTelegramBotWebhook<ItisScheduleBot>();
                
            }
            else
            {
                Task.Factory.StartNew(async () =>
                {
                    var botManager = app.ApplicationServices.GetRequiredService<IBotManager<ItisScheduleBot>>();
                    await botManager.SetWebhookStateAsync(false);
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
            }
            logger.LogInformation("Set up bot to notifier");
            //set up bot for notifier to fix DI-loop
            var notifier = (Notificator)app.ApplicationServices.GetRequiredService<INotifiactionSender>();
            notifier.Bot = app.ApplicationServices.GetRequiredService<ItisScheduleBot>();
            //run scheduled updates
            logger.LogInformation("Run schedules updating");
            updates = new UpdatesScheduler(app.ApplicationServices);
            updates.Start();

            app.Run(async (context) => { });
            logger.LogInformation("Configuration ended");
        }

        
    }
}