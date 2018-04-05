using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework.Abstractions;

namespace ScheduleBot.AspHost.BotServices
{
    public class Notificator : INotifiactionSender
    {
        private readonly IKeyboardsFactory keyboards;
        public  IBot Bot { get; set; }

        public Notificator(IKeyboardsFactory keyboards)
        {
            this.keyboards = keyboards;
        }

        

        public async Task SendNotificationsForIdsAsync(IEnumerable<long> ids, string message)
        {
            if (Bot != null)
            {
                var list = ids.ToList();
                await Task.Run(async () =>
                {
                    foreach (var id in list)
                    {
                        await Bot.Client.SendTextMessageAsync(id,
                            $"Спешу сообщить, что в твоем расписании произошли изменения. {message}. Слава роботам!",
                            replyMarkup: keyboards.GetMainOptionsKeyboard());
                        await Task.Delay(1000);
                    }
                });
            }
            
            
        }
    }
}