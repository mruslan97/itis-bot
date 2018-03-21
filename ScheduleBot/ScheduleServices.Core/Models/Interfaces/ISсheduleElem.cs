using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Models.Interfaces
{
    public enum ScheduleElemLevel
    {
        Week = 0,
        Day = 1,
        Lesson = 2
    }
    public interface IScheduleElem
    {
        ScheduleElemLevel Level { get; }
        IEnumerable<IScheduleElem> Elems { get; }
    }
}
