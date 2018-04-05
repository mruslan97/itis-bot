using System;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Helpers;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleBot.AspHost.Commads.GetScheduleCommands
{
    public abstract class AbstractGetForCommand : CommandBase<DefaultCommandArgs>
    {
        protected readonly IScheduleService Scheduler;
        protected readonly IBotDataStorage Storage;

        public AbstractGetForCommand(string name, IScheduleService scheduler, IBotDataStorage storage) : base(name)
        {
            Scheduler = scheduler;
            Storage = storage;
        }


        public abstract override Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args);

        protected async Task<UpdateHandlingResult> HandleCommandForPeriod(Update update, DefaultCommandArgs args,
            ScheduleRequiredFor period)
        {
            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday && period == ScheduleRequiredFor.Today
                || DateTime.Today.DayOfWeek == DayOfWeek.Saturday && period == ScheduleRequiredFor.Tomorrow)
            {
                await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, "Выходной день 😄");
                return UpdateHandlingResult.Handled;
            }

            var userGroups = await Storage.GetGroupsForChatAsync(update.Message.Chat);
            if (userGroups != null)
            {
                var schedule = await Scheduler.GetScheduleForAsync(userGroups, period);
                if (schedule.ScheduleRoot.Level == ScheduleElemLevel.Undefined)
                {
                    await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, "Пар нет 😄");
                    return UpdateHandlingResult.Handled;
                }

                var answer =
                    CustomSerializator.ProcessSchedule(schedule.ScheduleRoot.Elems.Cast<Lesson>(),
                        ((Day) schedule.ScheduleRoot).DayOfWeek);

                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    answer, ParseMode.Html);
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