using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

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
                            $"⚠️ В твоем расписании есть обновления! <b>{message}</b>.",
                            replyMarkup: keyboards.GetMainOptionsKeyboard(), parseMode:ParseMode.Html);
                        await Task.Delay(1000);
                    }
                });
            }
            
            
        }
    }
}