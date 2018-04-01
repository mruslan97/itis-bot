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
        /// <summary>
        /// Refresh schedules in storage
        /// </summary>
        /// <param name="groups">list of groups, for which schedules will be updated</param>
        /// <param name="day">day of week, which updated schedules related on</param>
        /// <returns></returns>
        Task UpdateSchedulesAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day);
    }
}