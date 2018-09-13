using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost
{
    public class ItisScheduleBot : BotBase<ItisScheduleBot>
    {
        private readonly ILogger<ItisScheduleBot> logger;

        public ItisScheduleBot(IOptions<BotOptions<ItisScheduleBot>> botOptions, ILogger<ItisScheduleBot> logger = null) : base(botOptions)
        {
            this.logger = logger;
        }

        public  override Task HandleUnknownUpdate(Update update)
        {
            logger?.LogWarning("Handler not found: {0}", JsonConvert.SerializeObject(update));
            //if (update.CallbackQuery != null){
                
            //    return Client.EditMessageTextAsync(update.CallbackQuery.From.Id, update.CallbackQuery.Message.MessageId,
            //        "Я тебе что, Эйнштейн?");
            //}
            //return Client.SendTextMessageAsync(
            //    update.Message.Chat.Id,
            //    "Нет такой команды :(",
            //    replyToMessageId: update.Message.MessageId);
            return Task.Run(() => Console.WriteLine($"unexpected message: {update?.Message?.Text} from {update?.Message?.Chat}"));
        }

        public override Task HandleFaultedUpdate(Update update, Exception e)
        {
            logger?.LogError(e, "Faulted update: {0}", JsonConvert.SerializeObject(update.Message));
            return Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "500 INTERNAL BOT ERROR (я сломался 😭)");
        }
    }
}
