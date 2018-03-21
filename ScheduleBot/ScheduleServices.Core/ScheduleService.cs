using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core
{
    public class ScheduleService : IScheduleServise
    {
        public event EventHandler UpdatedEvent;
        public Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, ScheduleRequiredFor period)
        {
            throw new NotImplementedException();
        }

        public Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day)
        {
            throw new NotImplementedException();
        }

        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, ScheduleRequiredFor period)
        {
            throw new NotImplementedException();
        }

        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, DayOfWeek day)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IScheduleGroup>> GetAvailibleGroupsAsync()
        {
            throw new NotImplementedException();
        }
    }
}