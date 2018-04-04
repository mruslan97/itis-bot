using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public class GetEngGroupsCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IScheduleServise scheduler;
        private readonly KeyboardsFactory keyboards;
        private readonly IBotDataStorage storage;

        public GetEngGroupsCommand(IBotDataStorage storage, IScheduleServise scheduler, KeyboardsFactory keyboards) :
            base("getengs")
        {
            this.storage = storage;
            this.scheduler = scheduler;
            this.keyboards = keyboards;
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
    }
    
    public class SetUpEngGroupCommand : SetUpGroupCommand
    {

        public SetUpEngGroupCommand(IBotDataStorage storage, IScheduleServise scheduler, KeyboardsFactory keyboards) :
            base(storage, scheduler, keyboards, "seteng")
        {
            
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return Scheduler.GroupsMonitor.AvailableGroups.Any(g => g.GType == ScheduleGroupType.Eng && g.Name == update.Message.Text.Trim());
            }
            else
                return true;
        }
        
    }
}