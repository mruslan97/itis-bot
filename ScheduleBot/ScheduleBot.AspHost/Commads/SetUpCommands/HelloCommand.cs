using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public class HelloCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly KeyboardsFactory keyboards;

        public HelloCommand(KeyboardsFactory keyboards) : base(name: "start")
        {
            this.keyboards = keyboards;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                $"Привет, {update.Message.Chat.FirstName}! Выбери курс.", replyMarkup: keyboards.GetCoursesKeyboad());

            return UpdateHandlingResult.Handled;
        }
    }
}
