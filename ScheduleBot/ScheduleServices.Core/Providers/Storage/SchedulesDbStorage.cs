using System;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;

namespace ScheduleServices.Core.Providers.Storage
{
    public class SchedulesDbStorage : ISchedulesStorage
    {
        public Task<ISchedule> GetScheduleAsync(IScheduleGroup group, DayOfWeek day)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateScheduleAsync(IScheduleGroup targetGroup, ISchedule schedule)
        {
            throw new NotImplementedException();
        }
    }
}