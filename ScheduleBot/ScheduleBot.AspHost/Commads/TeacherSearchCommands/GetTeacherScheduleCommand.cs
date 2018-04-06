using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Helpers;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleBot.AspHost.Commads.TeacherSearchCommands
{
    public class GetTeacherScheduleCommand : InlineCommand
    {
        private readonly ITeachersSource teachers;
        private readonly IKeyboardsFactory keyboards;
        private readonly IScheduleService scheduleService;
        private readonly TeacherScheduleSelector teacherSelector;

        public GetTeacherScheduleCommand(ITeachersSource teachers, IKeyboardsFactory keyboards,
            IScheduleService scheduleService, TeacherScheduleSelector teacherSelector) : base("f_")
        {
            this.teachers = teachers;
            this.keyboards = keyboards;
            this.scheduleService = scheduleService;
            this.teacherSelector = teacherSelector;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (base.CanHandleCommand(update))
                return (teachers.GetTeachersNames().Contains(update.CallbackQuery.Data)) && update.CallbackQuery.Message.Text.Contains("препод");
            return false;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update)
        {
            var teacher = teachers.GetTeachersNames()
                .FirstOrDefault(x => x == update.CallbackQuery.Data);
            if (teacher != null)
            {
                teacherSelector.TeacherName = teacher;
                var teacherSchedule = await scheduleService.CompileScheduleWithSelector(teacherSelector);
                if (teacherSchedule.ScheduleRoot.Level == ScheduleElemLevel.Week)
                    foreach (var daySchedule in teacherSchedule.ScheduleRoot.Elems.Cast<Day>())
                    {
                        await SendDay(daySchedule);
                        await Task.Delay(200);
                    }
                else if (teacherSchedule.ScheduleRoot.Level == ScheduleElemLevel.Day)
                {
                    await SendDay((Day) teacherSchedule.ScheduleRoot);
                }
                else
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Пар нет", replyMarkup: keyboards.GetMainOptionsKeyboard());
                }
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Нет такого преподавателя.", replyMarkup: keyboards.GetMainOptionsKeyboard());
            }

            return UpdateHandlingResult.Handled;

            async Task SendDay(Day day)
            {
                var answer =
                    CustomSerializator.ProcessSchedule(day.Elems.OfType<Lesson>(),
                        day.DayOfWeek);
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    answer, ParseMode.Html);
            }
        }
    }
}