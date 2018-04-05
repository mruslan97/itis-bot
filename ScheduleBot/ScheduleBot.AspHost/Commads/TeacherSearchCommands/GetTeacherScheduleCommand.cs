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
    public class GetTeacherScheduleCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly ITeachersSource teachers;
        private readonly IKeyboardsFactory keyboards;
        private readonly IScheduleServise scheduleServise;
        private readonly TeacherScheduleSelector teacherSelector;

        public GetTeacherScheduleCommand(ITeachersSource teachers, IKeyboardsFactory keyboards,
            IScheduleServise scheduleServise, TeacherScheduleSelector teacherSelector) : base("lec")
        {
            this.teachers = teachers;
            this.keyboards = keyboards;
            this.scheduleServise = scheduleServise;
            this.teacherSelector = teacherSelector;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
                return update.Message.Text.StartsWith("f:") &&
                       teachers.GetTeachersNames().Contains(update.Message.Text.Substring(2));
            return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var teacher = teachers.GetTeachersNames()
                .FirstOrDefault(x => x == args.RawInput.Substring(2));
            if (teacher != null)
            {
                teacherSelector.TeacherName = teacher;
                var teacherSchedule = await scheduleServise.CompileScheduleWithSelector(teacherSelector);
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