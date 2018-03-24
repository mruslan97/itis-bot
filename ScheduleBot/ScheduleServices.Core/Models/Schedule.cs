using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models
{
    public class Schedule : ISchedule
    {
        public ICollection<IScheduleGroup> ScheduleGroups { get; set; }
        public IScheduleElem ScheduleRoot { get; set; }
    }
}