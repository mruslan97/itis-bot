using System;
using System.Collections.Generic;

namespace ScheduleServices.Core.Models.Interfaces
{
    public interface ISchedule : IEquatable<ISchedule>
    {
        ICollection<IScheduleGroup> ScheduleGroups { get; set; }
        IScheduleElem ScheduleRoot { get; set; }

    }
}
