using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
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
        private readonly IBotDataStorage storage;

        public SetUpGroupCommand(IBotDataStorage storage, IScheduleServise scheduler) : base("setupgroup")
        {
            this.storage = storage;
            this.scheduler = scheduler;
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
                && storage.TryAddGroupToChat(group, update.Message.Chat.Id))
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Установлено!");
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