using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Models.Interfaces
{
    public enum ScheduleElemLevel
    {
        Undefined = 0,
        Week = 1,
        Day = 2,
        Lesson = 3
        
    }
    public interface IScheduleElem
    {
        ScheduleElemLevel Level { get; set; } 
        ICollection<IScheduleElem> Elems { get; set; }
    }
}
