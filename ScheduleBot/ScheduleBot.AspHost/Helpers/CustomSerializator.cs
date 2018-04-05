using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ScheduleBot.AspHost.BotServices;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleBot.AspHost.Helpers
{
    public class CustomSerializator
    {
        public static string ProcessSchedule(IEnumerable<Lesson> lessons, DayOfWeek day)
        {
            var answerMessage = new StringBuilder();
            var culture = new CultureInfo("ru-Ru");
            var dayOfWeek = culture.DateTimeFormat.GetDayName(day);
            answerMessage.AppendLine($"<b>{dayOfWeek.ToUpper()}</b>");
            if (lessons.Count() == 0)
            {
                answerMessage.AppendLine("Пар нет 😄");
                return answerMessage.ToString();
            }

            foreach (var lesson in lessons)
            {
                var inLesson = lesson is TeacherScheduleSelector.LessonWithGroup wg
                    ? (wg.RelatedGroup?.Name ?? "")
                    : lesson.Teacher;
                answerMessage.AppendLine(
                    $"{lesson.Discipline} {ConvertBoolToString(lesson.IsOnEvenWeek)} {lesson.Notation} \n{inLesson} \n{lesson.BeginTime.ToString("hh\\:mm")}-{(lesson.BeginTime + lesson.Duration).ToString("hh\\:mm")} \t ауд. {lesson.Place} \n---------------------------");
            }


            return answerMessage.ToString();
        }

        private static string ConvertBoolToString(bool? isEvenWeek)
        {
            if (isEvenWeek == null)
                return "";
            if ((bool) isEvenWeek)
                return "(ч.н.)";
            return "(н.н.)";
        }
    }
}