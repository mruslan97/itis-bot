using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MagicParser.Configuration;
using MagicParser.Impls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using ScheduleBot.AspHost.Keyboards;
using ScheduleBot.AspHost.Updating;
using ScheduleServices.Core;
using ScheduleServices.Core.Config;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using ScheduleServices.Core.Providers.Storage;
using Telegram.Bot.Framework;
using NLog.Web;
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
                .AddJsonFile($"appsettings.DevLocal.json", optional: true)
                .AddEnvironmentVariables();
                
            configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //configure parser
            services.AddGoogleApiParser(configuration.GetSection("GoogleApi"));
            //configure schedule service core
            services.AddDefaultScheduleServiceCore(GetGroupsList(), GetRules());
            //update jobs
            services.AddTransient<UpdateJob>();
            services.AddSingleton<UpdateTeachersListJob>();
            services.AddSingleton<ITeachersSource>(prov => prov.GetRequiredService<UpdateTeachersListJob>());
            //services about bot infrastruct
            services.AddTransient<TeacherScheduleSelector>();
            services.AddTransient<IScheduleEventArgsFactory, DefaultEventArgsFactory>();
            services.AddSingleton<IBotDataStorage, InMemoryBotStorage>();
            services.AddSingleton<INotifiactionSender, Notificator>();
            services.AddSingleton<IKeyboardsFactory>(provider => new KeyboardsFactory(GetGroupsList()));

            services.AddTelegramBot<ItisScheduleBot>(configuration.GetSection("ScheduleBot"))
                //organize
                .AddUpdateHandler<EchoCommand>()
                .AddUpdateHandler<HelloCommand>()
                .AddUpdateHandler<SettingsOptionsCommand>()
                .AddUpdateHandler<SettingsBackCommand>()
                .AddUpdateHandler<NotFoundGroupCommand>()
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

            logger.LogInformation("Bot up");
            // TO RUN LONGPOOLING UNCOMMENT IT AND COMMNENT `app.UseTelegramBotWebhook<ItisScheduleBot>();` BELOW
            /*Task.Factory.StartNew(async () =>
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
            });*/
            // TO RUN WEBHOOK UNCOMMENT IT AND COMMNENT `Task.Factory.StartNew( ..` UPPER
            app.UseTelegramBotWebhook<ItisScheduleBot>();

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


                //ENG GROUPS
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мартынова Е.В._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мартынова Е.В._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Е.В._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Е.В._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Макаев Х.Ф._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Баранова А.Р._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мельникова О.К._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Саляхова Г.И._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Саляхова Г.И._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Исмагилова Г.К._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Шамсутдинова Э.Х._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Переточкина С.М._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Саляхова Г.И._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Исмагилова Г.К._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мартынова Е.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Е.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Махмутова А.Н._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Маршева Т.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сакаева Л.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Шамсутдинова Э.Х._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Яхин М.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сабирова Р.Н._2курс_1"},

                //Scientic groups
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Биоинформатика_Булыгина Е.А._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Вычислительная статистика_Новиков П.А._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Обработка текстов на естественном языке_Тутубаллина Е.В._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Введение в исскуственный интеллект_Таланов М.О._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Физика_Мутыгуллина А.А._3курс_1"},

                //tech groups
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Введение в теорию и практику анимации_Костюк Д.И._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Шахова) Проектирование веб- интерфейсов_Шахова И.С._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Технологии Net_Гумеров К.А._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Эффективная разработка_Якупов А.Ш._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Скриптинг_Хусаинов Р.Р._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Гиниятуллин) Проектирование веб- интерфейсов_Гиниятуллин Р.Г._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Методы оптимизации_Фазылов В.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Таланов) Введение в исскуственный интеллект_Таланов М.О._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Кугуракова) Введение в исскуственный интеллект_Кугуракова В.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Сидиков) Разработка корпоративных приложений_Сидиков М.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Аршинов) Разработка корпоративных приложений_Аршинов М.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Введение в робототехнику_Магид Е.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Интернет - программирование Django_Абрамский М.М._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Ruby_Бажанов В.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "PHP-_Кошарский И.Е._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Разработка мобильных приложений_Шахова И.С._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Визуализация данных_Костюк Д.И._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Программирование на С++_Сагитов А.Г._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Цифровая живопись_Евстафьев М.Е._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Кроссплатформенное прикладное программирование_Магид Е.А._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Мобильные информационные системы_Хайруллин А.Ф._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Интернет вещей_Даутов Р.И._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Механизмы защиты удаленного доступа_Зиятдинов М.Т._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Визуализация данных_Костюк Д.И._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Проектирование человеко- машинных интерфейсов_Зайдуллин С.С._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Аспектно-ориентированное проектирование и разработка_Костюк Д.И._4курс_1"},

            };
        }

        private IList<ICompatibleGroupsRule> GetRules()
        {
            return new List<ICompatibleGroupsRule>()
            {
                
                new SpecEngGroupsRule("1stCourse_1stStreamMartynova", (academicName, engName) =>
                {
                    if (engName.EndsWith("1") &&
                       engName.Contains("1курс"))
                    {
                        if (academicName.StartsWith("11-7") && Regex.IsMatch(academicName, $@"[1-4]$"))
                            return true;
                        else
                            return false;
                    }
                    return false;
                } ),
                new SpecEngGroupsRule("1stCourse_1stStreamNoMartynova", (academicName, engName) =>
                {
                    if (engName.EndsWith("1") &&
                        engName.Contains("1курс") && !engName.Contains("мартынова"))
                    {
                        if (academicName.StartsWith("11-7") && Regex.IsMatch(academicName, $@"[1-5]$"))
                            return true;
                        else
                            return false;
                    }
                    return false;
                } ),
                new CommonEngGroupsRule("1stCourse_2stStream", "2", "1курс", "11-7", $@"[6-9]$"),
                new CommonEngGroupsRule("2stCourse_1stStream", "1", "2курс", "11-6", $@"[1-8]$"),
                new CommonTypedRule("ScienticThirdCourse", ScheduleGroupType.PickedScientic, "11-5", "3курс"),
                new CommonTypedRule("TechSecondCourse", ScheduleGroupType.PickedTech, "11-6", "2курс"),
                new CommonTypedRule("TechThirdCourse", ScheduleGroupType.PickedTech, "11-5", "3курс"),
                new CommonTypedRule("Tech4Course", ScheduleGroupType.PickedTech, "11-4", "4курс"),
            };
        }

        private class CommonTypedRule : CompatibleGroupsFuncRule
        {
            public CommonTypedRule(string name, ScheduleGroupType secondType,
                string academicStarts, string targetCourse, string flow = "1",
                string academicRegexp = null) : base(name, (g1, g2) => true)
            {
                CheckFunc = (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup typed = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == secondType)
                        typed = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == secondType)
                        typed = second;
                    if (typed == null || academic == null)
                        return false;
                    if (typed.Name.ToLowerInvariant().EndsWith(flow) &&
                        typed.Name.ToLowerInvariant().Contains(targetCourse))
                    {
                        var trimmed = (academic.Name.Trim());
                        return trimmed.StartsWith(academicStarts) && (academicRegexp == null || Regex.IsMatch(trimmed, academicRegexp));
                    }

                    return false;
                };
            }
        }

        private class CommonEngGroupsRule : CompatibleGroupsFuncRule
        {
            public CommonEngGroupsRule(string name, string engEndsWith, string engContains, string academicStarts, string academicRegexp) : base(name, (g1, g2) => true)
            {
                CheckFunc = (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup eng = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == ScheduleGroupType.Eng)
                        eng = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == ScheduleGroupType.Eng)
                        eng = second;
                    if (eng == null || academic == null)
                        return false;
                    if (eng.Name.ToLowerInvariant().EndsWith(engEndsWith) &&
                        eng.Name.ToLowerInvariant().Contains(engContains))
                    {
                        var trimmed = (academic.Name.Trim());
                        if (trimmed.StartsWith(academicStarts) && Regex.IsMatch(trimmed, academicRegexp))
                            return true;
                        else
                            return false;
                    }

                    return false;
                };
            }
        }

        private class SpecEngGroupsRule : CompatibleGroupsFuncRule
        {
            public SpecEngGroupsRule(string name, Func<string, string, bool> academicAndEngNamesFunc) : base(name, (g1, g2) => true)
            {
                CheckFunc = (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup eng = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == ScheduleGroupType.Eng)
                        eng = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == ScheduleGroupType.Eng)
                        eng = second;
                    if (eng == null || academic == null)
                        return false;
                    return academicAndEngNamesFunc(academic.Name.Trim().ToLowerInvariant(),
                        eng.Name.Trim().ToLowerInvariant());
                };
            }
            /*new CompatibleGroupsFuncRule("1stCourse_1stStream", (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup eng = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == ScheduleGroupType.Eng)
                        eng = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == ScheduleGroupType.Eng)
                        eng = second;
                    if (eng == null || academic == null)
                        return false;
                    if (eng.Name.ToLowerInvariant().EndsWith("1") &&
                        eng.Name.ToLowerInvariant().Contains("1курс"))
                    {
                        var trimmed = (academic.Name.Trim());
                        if (trimmed.StartsWith("11-7") && Regex.IsMatch(trimmed, $@"[0-4]$"))
                            return true;
                        else
                            return false;
                    }
                    return false;
                })*/
        }
    }
}