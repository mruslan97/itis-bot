using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class CompatibleGroupsFuncRule : ICompatibleGroupsRule
    {
        private readonly Func<IScheduleGroup, IScheduleGroup, bool> checkFunc;
        public CompatibleGroupsFuncRule(Func<IScheduleGroup, IScheduleGroup, bool> checkFunc)
        {
            this.checkFunc = checkFunc;
        }

        public bool AreCompatible(IScheduleGroup first, IScheduleGroup second)
        {
            return checkFunc.Invoke(first, second);
        }
    }
}
