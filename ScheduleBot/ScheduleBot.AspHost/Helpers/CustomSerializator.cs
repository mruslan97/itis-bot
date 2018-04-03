using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
                answerMessage.AppendLine(
                    $"{lesson.Discipline} {lesson.Notation} \n{lesson.Teacher} \n{lesson.BeginTime.ToString("hh\\:mm")}-{(lesson.BeginTime + lesson.Duration).ToString("hh\\:mm")} \t ауд. {lesson.Place} \n---------------------------");

            return answerMessage.ToString();
        }
    }
}