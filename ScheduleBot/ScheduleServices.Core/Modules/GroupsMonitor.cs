using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class GroupsMonitor
    {
        public GroupsMonitor(List<IScheduleGroup> list)
        {

        }
        //todo impl
        public IEnumerable<IScheduleGroup> AvailableGroups { get; }
        public IEnumerable<IScheduleGroup> RemoveInvalidGroups(IEnumerable<IScheduleGroup> groups)
        {
            throw new NotImplementedException();
        }

        public bool IsGroupPresent(IScheduleGroup group)
        {
            throw new NotImplementedException();
        }

        public bool TryExtractOriginalGroup(IScheduleGroup group, out IScheduleGroup extracted)
        {
            throw new NotImplementedException();
        }
    }
}
