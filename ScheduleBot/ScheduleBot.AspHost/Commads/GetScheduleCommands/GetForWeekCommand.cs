using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Helpers;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.ScheduleElems;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleBot.AspHost.Commads.GetScheduleCommands
{
    public class GetForWeekCommand : CommandBase<DefaultCommandArgs>
    {
        protected readonly IScheduleServise Scheduler;
        protected readonly IBotDataStorage Storage;

        public GetForWeekCommand(IScheduleServise scheduler, IBotDataStorage storage) : base("week")
        {
            Scheduler = scheduler;
            Storage = storage;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
                return update.Message.Text.ToLowerInvariant().Contains("недел");
            return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var userGroups = await Storage.GetGroupsForChatAsync(update.Message.Chat);
            if (userGroups != null)
            {
                var weekSchedule = await Scheduler.GetScheduleForAsync(userGroups, ScheduleRequiredFor.Week);
                foreach (var daySchedule in weekSchedule.ScheduleRoot.Elems.Cast<Day>())
                {
                    var answer =
                        CustomSerializator.ProcessSchedule(daySchedule.Elems.Cast<Lesson>(), daySchedule.DayOfWeek);
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        answer, ParseMode.Html);
                    System.Threading.Thread.Sleep(200);
                }
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "А группа?");
            }
            return UpdateHandlingResult.Handled;
        }
    }
}