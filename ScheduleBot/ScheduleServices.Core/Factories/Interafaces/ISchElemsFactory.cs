using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleServices.Core.Factories.Interafaces
{
    public interface ISchElemsFactory
    {
        Schedule GetSchedule();
        Week GetWeek(int days = 6);
        Day GetDay(DayOfWeek dayOfWeek = DayOfWeek.Monday);
        Lesson GetLesson(string disciplineName, string teacher, string place, TimeSpan beginTime, TimeSpan duration = default(TimeSpan), bool? isOnEvenWeek = null);
        Undefined GetUndefined();
    }
}
