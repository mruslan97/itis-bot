using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Providers.Interfaces
{
    public interface ISchedulesStorage : IScheduleInfoProvider
    {
        //add or update schedule elem for some group, replace schedule, exclude with root of 'Lesson' type
        Task<bool> UpdateScheduleAsync(IScheduleGroup targetGroup, IScheduleElem scheduleRoot);
        IEnumerable<ISchedule> GetAll(IEnumerable<IScheduleGroup> availableGroups);
        Task RunVisitor(IDynamicElemVisitor visitor);
        Task RemoveScheduleAsync(IScheduleGroup targetGroup, DayOfWeek day);
    }
}