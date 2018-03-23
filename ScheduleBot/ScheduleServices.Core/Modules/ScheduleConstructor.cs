using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class ScheduleConstructor
    {
        public Task<ISchedule> ConstructFromMany(IEnumerable<ISchedule> schedules)
        {
            throw new NotImplementedException();
        }
    }
}
