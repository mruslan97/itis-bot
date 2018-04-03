using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public class GetEngGropsCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IScheduleServise scheduler;
        private readonly KeyboardsFactory keyboards;
        private readonly IBotDataStorage storage;

        public GetEngGropsCommand(IBotDataStorage storage, IScheduleServise scheduler, KeyboardsFactory keyboards) :
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
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Выбери свою группу по английскому.",
                    replyMarkup: keyboards.GetKeyboardForCollection(
                        scheduler.GroupsMonitor.GetAllowedGroups(ScheduleGroupType.Eng, userAcademicGroup),
                        g => g.Name));
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
}