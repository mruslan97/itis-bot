using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;

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
                        try
                        {
                            await Bot.Client.SendTextMessageAsync(id,
                                $"⚠️ В твоем расписании есть обновления! <b>{message}</b>.",
                                replyMarkup: keyboards.GetMainOptionsKeyboard(), parseMode: ParseMode.Html);
                            await Task.Delay(1000);
                        }
                        catch (Exception e)
                        {
                            logger?.LogWarning(e, "Failed sent to {0}, may be we're blocked?", id);
                        }
                       
                    }
                });
            }
            else
            {
                logger?.LogError("Bot not found");
            }
            
            
        }

        public async Task SendPureMessageForIdsAsync(IEnumerable<long> ids, string message)
        {
            if (Bot != null)
            {
                var list = ids.ToList();
                await Task.Run(async () =>
                {
                    foreach (var id in list)
                    {
                        await Bot.Client.SendTextMessageAsync(id,
                            $"{message}",
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