using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public class GetEngGroupsCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IScheduleServise scheduler;
        private readonly IKeyboardsFactory keyboards;
        private readonly IBotDataStorage storage;

        public GetEngGroupsCommand(IBotDataStorage storage, IScheduleServise scheduler, IKeyboardsFactory keyboards) :
            base("getengs")
        {
            this.storage = storage;
            this.scheduler = scheduler;
            this.keyboards = new EngKeyboardDecorator(keyboards);
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().StartsWith("eng");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var userAcademicGroup =
                (await storage.GetGroupsForChatAsync(update.Message.Chat)).FirstOrDefault(g =>
                    g.GType == ScheduleGroupType.Academic);
            if (userAcademicGroup != null)
            {
                var engGroups = scheduler.GroupsMonitor.GetAllowedGroups(ScheduleGroupType.Eng, userAcademicGroup)?.ToList() ?? new List<IScheduleGroup>();
                if (engGroups.Any())
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Выбери свою группу по английскому.",
                        replyMarkup: keyboards.GetKeyboardForCollection(engGroups,
                            g => g.Name));
                }
                else
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "У тебя не нашлось групп по английскому, прости.",
                        replyMarkup: keyboards.GetSettingsKeyboard());
                }
                
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Сначала надо установить группу ☝️. Выбери курс.", replyMarkup: keyboards.GetCoursesKeyboad());
            }


            return UpdateHandlingResult.Handled;
        }
        private class EngKeyboardDecorator : IKeyboardsFactory
        {
            private readonly IKeyboardsFactory keyboards;

            public EngKeyboardDecorator(IKeyboardsFactory impl)
            {
                keyboards = impl;
            }

            public ReplyKeyboardMarkup GetCoursesKeyboad()
            {
                return keyboards.GetCoursesKeyboad();
            }

            public ReplyKeyboardMarkup GetGroupsOfCourseKeyboard(int course)
            {
                return keyboards.GetGroupsOfCourseKeyboard(course);
            }

            public ReplyKeyboardMarkup GetKeyboardForCollection<TItem>(IEnumerable<TItem> keyboardItems, Func<TItem, string> buttonTextSelector)
            {
                return keyboards.GetKeyboardForCollection(keyboardItems, (item) => buttonTextSelector(item).Substring(0, buttonTextSelector(item).IndexOf("_")));
            }

            public ReplyKeyboardMarkup GetPeriodOptionsKeyboard()
            {
                return keyboards.GetPeriodOptionsKeyboard();
            }

            public ReplyKeyboardMarkup GetSettingsKeyboard()
            {
                return keyboards.GetSettingsKeyboard();
            }
        }
    }
    
    public class SetUpEngGroupCommand : SetUpGroupCommand
    {

        public SetUpEngGroupCommand(IBotDataStorage storage, IScheduleServise scheduler, IKeyboardsFactory keyboards) :
            base(storage, scheduler, keyboards, "seteng")
        {
            
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return Scheduler.GroupsMonitor.AvailableGroups.Any(g => g.GType == ScheduleGroupType.Eng && g.Name.ToLowerInvariant().StartsWith(update.Message.Text.ToLowerInvariant().Trim()));
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var userAcademicGroup =
                (await Storage.GetGroupsForChatAsync(update.Message.Chat)).FirstOrDefault(g =>
                    g.GType == ScheduleGroupType.Academic);
            if (userAcademicGroup != null)
            {
                switch (userAcademicGroup.Name.ElementAt(3))
                {
                    
                    case '6':
                        args.RawInput = args.RawInput + "_2курс_1";
                        break;
                    case '7':
                        args.RawInput = args.RawInput + "_1курс";
                        if (Regex.IsMatch(userAcademicGroup.Name, $@"[1-5]$"))
                            args.RawInput = args.RawInput + "_1";
                        else
                            args.RawInput = args.RawInput + "_2";
                        break;
                }

                return await base.HandleCommand(update, args);


            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Сначала надо установить группу ☝️. Выбери курс.", replyMarkup: Keyboards.GetCoursesKeyboad());
                return UpdateHandlingResult.Handled;
            }
        }

        
    }

    
}