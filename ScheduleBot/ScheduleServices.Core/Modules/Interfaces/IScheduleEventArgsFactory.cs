using System;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules.Interfaces
{
    public interface IScheduleEventArgsFactory
    {
        EventArgs GetArgs(IScheduleElem schedule);
    }
}