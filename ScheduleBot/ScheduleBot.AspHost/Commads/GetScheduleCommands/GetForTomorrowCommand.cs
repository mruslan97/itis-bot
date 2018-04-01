﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleServices.Core;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.GetScheduleCommands
{
    public class GetForTomorrowCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IScheduleServise scheduler;
        private readonly IBotDataStorage storage;

        public GetForTomorrowCommand(IScheduleServise scheduler, IBotDataStorage storage) : base(name: "tomorrow")
        {
            this.scheduler = scheduler;
            this.storage = storage;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("завтра");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            string replyText = JsonConvert.SerializeObject(
                await scheduler.GetScheduleForAsync(
                    await storage.GetGroupsForChatAsync(update.Message.Chat.Id),
                    ScheduleRequiredFor.Tomorrow));

            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                replyText,
                replyToMessageId: update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }
    }
}