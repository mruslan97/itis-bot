using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Commads
{
    public class TestCommand : CommandBase<DefaultCommandArgs>
    {
        public TestCommand() : base(name: "test")
        {
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var replyText = "test клавиатуры";
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new [] // first row
                {
                    InlineKeyboardButton.WithCallbackData("test"),
                    InlineKeyboardButton.WithCallbackData("1.2"),
                },
                new [] // second row
                {
                    InlineKeyboardButton.WithCallbackData("2.1"),
                    InlineKeyboardButton.WithCallbackData("2.2"),
                }
            });

            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Message",replyMarkup:inlineKeyboard);

            return UpdateHandlingResult.Handled;
        }
    }
}
