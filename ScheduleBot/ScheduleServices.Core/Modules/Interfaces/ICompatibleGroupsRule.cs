using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules.Interfaces
{
    public interface ICompatibleGroupsRule
    {
        bool AreCompatible(IScheduleGroup first, IScheduleGroup second);
    }
}
