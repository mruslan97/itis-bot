using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleBot.AspHost.Helpers
{
    public class CustomSerializator
    {
        public static string ProcessSchedule(ISchedule schedule)
        {
            var lessons = schedule.ScheduleRoot.Elems.Cast<Lesson>();
            if (lessons.Count() == 0)
                return "Пар нет 😄";
            var answerMessage = new StringBuilder();
            var culture = new System.Globalization.CultureInfo("ru-Ru");
            var dayOfWeek = culture.DateTimeFormat.GetDayName(((Day)schedule.ScheduleRoot).DayOfWeek);
            answerMessage.AppendLine($"<b>{dayOfWeek.ToUpper()}</b>");
            foreach (var lesson in lessons)
            {
                answerMessage.AppendLine($"{lesson.Discipline} \n{lesson.Teacher} \n{lesson.BeginTime.ToString("hh\\:mm")} \t ауд. {lesson.Place} \n---------------------------");
            }

            return answerMessage.ToString();
        }
    }
}
