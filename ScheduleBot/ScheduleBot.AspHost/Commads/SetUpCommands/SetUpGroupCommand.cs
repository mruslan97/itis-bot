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
    public class SetUpGroupCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IScheduleServise scheduler;
        private readonly KeyboardsFactory keyboards;
        private readonly IBotDataStorage storage;

        public SetUpGroupCommand(IBotDataStorage storage, IScheduleServise scheduler, KeyboardsFactory keyboards) : base("setupgroup")
        {
            this.storage = storage;
            this.scheduler = scheduler;
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().StartsWith("11-");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            string groupName;
            try
            {
                groupName = args.RawInput.Substring(0, 6);
            }
            catch (Exception e)
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Нет такой группы :(");
                return UpdateHandlingResult.Handled;
            }

            if (scheduler.GroupsMonitor.TryFindGroupByName(groupName, out IScheduleGroup group)
                && await storage.TryAddGroupToChatAsync(group, update.Message.Chat))
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Установлено!", replyMarkup: keyboards.GetPeriodOptionsKeyboard());
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Нет такой группы :(");
            }


            return UpdateHandlingResult.Handled;
        }
    }
}