using System;
using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Day : IScheduleElem
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Day;
        public IEnumerable<IScheduleElem> Elems { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
    }
}