using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

namespace ScheduleBot.AspHost.Commads
{
    public class SettingsOptionsCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly KeyboardsFactory keyboards;

        public SettingsOptionsCommand(KeyboardsFactory keyboards) : base("getsettings")
        {
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("настройки");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Лол, удачи братан!", replyMarkup: keyboards.GetSettingsKeyboard());
            

            return UpdateHandlingResult.Handled;
        }
    }
    public class SettingsBackCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly KeyboardsFactory keyboards;

        public SettingsBackCommand(KeyboardsFactory keyboards) : base("backtomain")
        {
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("back");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {

            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id, "ня",  replyMarkup: keyboards.GetPeriodOptionsKeyboard());


            return UpdateHandlingResult.Handled;
        }
    }
}