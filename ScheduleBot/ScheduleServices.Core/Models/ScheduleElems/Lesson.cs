using System;
using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Lesson : IScheduleElem
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Lesson;
        public ICollection<IScheduleElem> Elems { get; set; } = null;

        public bool? IsOnEvenWeek { get; set; } = null;

        public string Discipline { get; set; }

        public string Teacher { get; set; }

        public string Place { get; set; }

        public TimeSpan BeginTime { get; set; }

        public TimeSpan Duration { get; set; }
    }
}