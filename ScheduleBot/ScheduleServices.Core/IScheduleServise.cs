using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core
{
    public enum ScheduleRequiredFor 
    {
        Week = 0,
        Today = 1,
        Tomorrow = 2
    }
    public interface IScheduleServise
    {
        IGroupsMonitor GroupsMonitor { get; }
        Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, ScheduleRequiredFor period);
        Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day);
        Task<ISchedule> GetScheduleForAsync(IScheduleGroup group, ScheduleRequiredFor period);
        Task<ISchedule> GetScheduleForAsync(IScheduleGroup group, DayOfWeek day);
    }
}