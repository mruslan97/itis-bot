using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Commads
{
    public class WeekIsEvenCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IKeyboardsFactory keyboards;
        private readonly DateTime firstEvenWeekStart = new DateTime(2018, 2, 11, 23, 59, 59, DateTimeKind.Utc);

        public WeekIsEvenCommand(IKeyboardsFactory keyboards) : base("isevenweek")
        {
            this.keyboards = keyboards;
        }
        

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("четность");
            }
            else
                return true;
        }

        

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var diffDaysCount = (DateTime.UtcNow.Date - firstEvenWeekStart).Days;
            diffDaysCount -= GetCurrentIndexOfDayOfWeekForEuropeanMan();
            var weeksSpent = diffDaysCount / 7;
            bool isEven = weeksSpent % 2 == 0;
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id, isEven ? $"Это {weeksSpent + 2}-я неделя - четная." : $"Это {weeksSpent + 2}-я неделя - нечетная.", replyMarkup: keyboards.GetMainOptionsKeyboard());

            return UpdateHandlingResult.Handled;

            int GetCurrentIndexOfDayOfWeekForEuropeanMan()
            {
                var utc = DateTime.UtcNow;
                if (utc.DayOfWeek == DayOfWeek.Sunday)
                    return 6;
                return (int)(utc.DayOfWeek - 1);
            }
        }
    }
}
