using System.Collections.Generic;

namespace ScheduleServices.Core.Models.Interfaces
{
    public interface ISchedule
    {
        IEnumerable<IScheduleGroup> ScheduleGroups { get; set; }
        IScheduleElem ScheduleRoot { get; set; }

    }
}
