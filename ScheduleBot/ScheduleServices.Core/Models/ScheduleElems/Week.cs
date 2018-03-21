using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Week : IScheduleElem
    {
        public ScheduleElemLevel Level { get; } = ScheduleElemLevel.Week;
        public IEnumerable<IScheduleElem> Elems { get; set; }
    }
}