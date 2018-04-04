using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class DefaultEventArgsFactory : IScheduleEventArgsFactory
    {
        public EventArgs GetArgs(ISchedule schedule)
        {
            if (schedule.ScheduleRoot is Day day)
            {
                return new ParamEventArgs<DayOfWeek>() {Param = day.DayOfWeek};
            }
            return EventArgs.Empty;
            
        }
    }
}
