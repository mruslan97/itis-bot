using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost
{
    public class ItisScheduleBot : BotBase<ItisScheduleBot>
    {
        public ItisScheduleBot(IOptions<BotOptions<ItisScheduleBot>> botOptions) : base(botOptions)
        {
        }

        public  override Task HandleUnknownUpdate(Update update)
        {
            if (update.CallbackQuery != null)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Тудык")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Сюдык")
                    }
                });
                var answer = update.CallbackQuery.Message.Text;
                var number = Convert.ToInt32(answer.Substring(answer.Length - 1));
                number = update.CallbackQuery.Data == "Тудык" ? number += 1 : number -= 1;
                answer = answer.Substring(0, answer.Length - 1) + number;
                //Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, update.CallbackQuery.Data);
                return Client.EditMessageTextAsync(update.CallbackQuery.From.Id, update.CallbackQuery.Message.MessageId,
                    answer, replyMarkup: inlineKeyboard);
            }
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
