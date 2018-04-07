using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.SetUpCommands.ElectivesSetUpCommands
{
    public abstract class SetUpElectiveGroupCommand : CommandBase<DefaultCommandArgs>
    {
        protected IScheduleService Scheduler;
        protected IKeyboardsFactory Keyboards;
        protected readonly ScheduleGroupType GroupType;
        protected IBotDataStorage Storage;
        protected Dictionary<long, ValueTuple<IScheduleGroup, IScheduleGroup>> Cache;

        protected SetUpElectiveGroupCommand(ScheduleGroupType type, IBotDataStorage storage, IScheduleService scheduler, IKeyboardsFactory keyboards,
            string command) : base(command)
        {
            this.GroupType = type;
            this.Storage = storage;
            this.Scheduler = scheduler;
            this.Keyboards = keyboards;
            Cache = new Dictionary<long, ValueTuple<IScheduleGroup, IScheduleGroup>>();
        }
        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                var userAcademicGroup = GetAcademic(update);

                if (userAcademicGroup != null)
                {
                    var allowed = GetFirstMatchAllowed(userAcademicGroup, update);
                    if (allowed != null)
                    {
                        Cache.TryAdd(update.Id, (userAcademicGroup, allowed));
                        return true;
                    }
                }
                return false;
            }
            return true;
        }
        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            IScheduleGroup userAcademicGroup;
            IScheduleGroup groupToSet;
            if (Cache.TryGetValue(update.Id, out ValueTuple<IScheduleGroup, IScheduleGroup> cached))
            {
                userAcademicGroup = cached.Item1 ?? await GetAcademicAsync(update);
                groupToSet = cached.Item2 ?? GetFirstMatchAllowed(userAcademicGroup, update);
                Cache.Remove(update.Id);
            }
            else
            {
                userAcademicGroup = await GetAcademicAsync(update);
                groupToSet = GetFirstMatchAllowed(userAcademicGroup, update);
            }

            if (userAcademicGroup != null && groupToSet != null)
            {
                if (await Storage.TryAddGroupToChatAsync(groupToSet, update.Message.Chat))
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Установлено!", replyMarkup: Keyboards.GetMainOptionsKeyboard());
                }
                else
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Не удалось установить группу :(");
                }
            }
            else
            {
                Console.Out.WriteLine($"Bad situation: allowed group not found but it can handle this command: {GroupType}");
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Сначала надо установить группу ☝️. Выбери курс.", replyMarkup: Keyboards.GetCoursesKeyboad());
            }
            return UpdateHandlingResult.Handled;
        }

        protected IScheduleGroup GetFirstMatchAllowed(IScheduleGroup academic, Update update)
        {
            var allowed = Scheduler.GroupsMonitor
                              .GetAllowedGroups(GroupType, academic)
                              ?.ToList() ?? new List<IScheduleGroup>();
            return allowed?.FirstOrDefault(g => g.GType == GroupType && g.Name.ToLowerInvariant()
                                                    .StartsWith(update.Message.Text.ToLowerInvariant()
                                                        .Trim()));
        }
        protected IScheduleGroup GetAcademic(Update update)
        {
            return GetAcademicAsync(update).Result;
        }
        protected async Task<IScheduleGroup> GetAcademicAsync(Update update)
        {
            return (await Storage.GetGroupsForChatAsync(update.Message.Chat)).FirstOrDefault(g =>
                g.GType == ScheduleGroupType.Academic);
        }
    }

    public abstract class GetElectiveGroupsCommand : CommandBase<DefaultCommandArgs>
    {
        protected IScheduleService Scheduler;
        protected IKeyboardsFactory Keyboards;
        protected readonly ScheduleGroupType GroupType;
        protected readonly string TriggerName;
        protected readonly string ResponseText;
        protected readonly string NotFoundResponseText;
        protected IBotDataStorage Storage;

        protected GetElectiveGroupsCommand(ScheduleGroupType type, string triggerName, string responseText, string notFoundResponseText, IBotDataStorage storage, IScheduleService scheduler, IKeyboardsFactory keyboards,
            string command) :
            base(command)
        {
            GroupType = type;
            this.TriggerName = triggerName;
            this.Storage = storage;
            this.Scheduler = scheduler;
            ResponseText = responseText;
            this.NotFoundResponseText = notFoundResponseText;
            this.Keyboards = new AdditionalCoursesKeyboardDecorator(keyboards);
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().StartsWith(TriggerName);
            }

            return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var userAcademicGroup =
                (await Storage.GetGroupsForChatAsync(update.Message.Chat)).FirstOrDefault(g =>
                    g.GType == ScheduleGroupType.Academic);
            if (userAcademicGroup != null)
            {
                var allowedGroups =
                    Scheduler.GroupsMonitor.GetAllowedGroups(GroupType, userAcademicGroup)?.ToList() ??
                    new List<IScheduleGroup>();
                if (allowedGroups.Any())
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        ResponseText,
                        replyMarkup: Keyboards.GetKeyboardForCollection(allowedGroups,
                            g => g.Name));
                }
                else
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        NotFoundResponseText,
                        replyMarkup: Keyboards.GetSettingsKeyboard());
                }
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Сначала надо установить группу ☝️. Выбери курс.", replyMarkup: Keyboards.GetCoursesKeyboad());
            }


            return UpdateHandlingResult.Handled;
        }

    }
}
