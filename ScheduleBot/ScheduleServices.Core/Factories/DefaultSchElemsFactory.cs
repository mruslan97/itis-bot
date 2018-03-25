using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleServices.Core.Factories
{
    public class DefaultSchElemsFactory : ISchElemsFactory
    {
        public Schedule GetSchedule()
        {
            return new Schedule()
            {
                ScheduleGroups = new List<IScheduleGroup>(),
                ScheduleRoot = new Undefined()
            };
        }

        public Week GetWeek(int days = 6)
        {
            return new Week()
            {
                Elems = new List<IScheduleElem>()
            };
        }

        public Day GetDay(DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            return new Day()
            {
                DayOfWeek = dayOfWeek,
                Elems = new List<IScheduleElem>()
            };
        }

        public Lesson GetLesson(string disciplineName, string teacher, string place, TimeSpan beginTime,
            TimeSpan duration = default(TimeSpan), bool? isOnEvenWeek = null)
        {
            return new Lesson()
            {
                BeginTime = beginTime,
                Duration = duration,
                Discipline = disciplineName,
                Elems = null,
                IsOnEvenWeek = isOnEvenWeek,
                Teacher = teacher,
                Place = place
            };
        }
    }
}
