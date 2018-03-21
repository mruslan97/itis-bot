using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads
{
    public class EchoCommand : CommandBase<DefaultCommandArgs>
    {
        public EchoCommand() : base(name: "echo")
        {
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            string replyText = string.IsNullOrWhiteSpace(args.ArgsInput) ? "Echo What?" : args.ArgsInput;

            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                replyText,
                replyToMessageId: update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }
    }
}