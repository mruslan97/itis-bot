using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleServices.Core.Factories
{
    public class DefaultSchElemsFactory : ISchElemsFactory
    {
        public Schedule GetSchedule()
        {
            throw new NotImplementedException();
        }

        public Week GetWeek(int days = 6)
        {
            throw new NotImplementedException();
        }

        public Day GetDay(DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            throw new NotImplementedException();
        }

        public Lesson GetLesson(string disciplineName, string teacher, string place, TimeSpan beginTime,
            TimeSpan duration = default(TimeSpan))
        {
            throw new NotImplementedException();
        }
    }
}
