using System;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Providers.Interfaces
{
    public interface IScheduleInfoProvider
    {
        Task<ISchedule> GetScheduleAsync(IScheduleGroup group, DayOfWeek day);
    }
}