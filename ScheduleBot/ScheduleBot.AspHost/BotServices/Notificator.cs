using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework.Abstractions;

namespace ScheduleBot.AspHost.BotServices
{
    public class Notificator : INotifiactionSender
    {
        private readonly IKeyboardsFactory keyboards;
        private readonly ILogger<Notificator> logger;
        public  IBot Bot { get; set; }

        public Notificator(IKeyboardsFactory keyboards, ILogger<Notificator> logger)
        {
            this.keyboards = keyboards;
            this.logger = logger;
        }

        

        public async Task SendNotificationsForIdsAsync(IEnumerable<long> ids, string message)
        {
            logger?.LogInformation("Notification about changed sch send:" + message);
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
            else
            {
                logger?.LogError("Bot not found");
            }
            
            
        }
    }
}