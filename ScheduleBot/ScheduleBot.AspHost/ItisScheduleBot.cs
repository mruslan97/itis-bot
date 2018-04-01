using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost
{
    public class ItisScheduleBot : BotBase<ItisScheduleBot>
    {
        public ItisScheduleBot(IOptions<BotOptions<ItisScheduleBot>> botOptions) : base(botOptions)
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
            Console.WriteLine(e);
            return Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Все пропало, шеф, йа сломалсо");
        }
    }
}
