using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Lesson : IScheduleElem
    {
        public ScheduleElemLevel Level { get; } = ScheduleElemLevel.Lesson;
        public IEnumerable<IScheduleElem> Elems { get;  } = null;

        public bool? IsOnEvenWeek { get; set; } = null;
    }
}