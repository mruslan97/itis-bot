using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace SheduleBot.AspHost
{
    public class ItisSheduleBot : BotBase<ItisSheduleBot>
    {
        public ItisSheduleBot(IOptions<BotOptions<ItisSheduleBot>> botOptions) : base(botOptions)
        {
        }

        public  override Task HandleUnknownUpdate(Update update)
        {
             return Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Я тебе что, Эйнштейн?",
                replyToMessageId: update.Message.MessageId);
        }

        public override Task HandleFaultedUpdate(Update update, Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
