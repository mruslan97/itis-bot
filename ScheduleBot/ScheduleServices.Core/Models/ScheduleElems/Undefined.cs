using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Undefined : IScheduleElem
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Undefined;
        public ICollection<IScheduleElem> Elems { get; set; }
    }
}
