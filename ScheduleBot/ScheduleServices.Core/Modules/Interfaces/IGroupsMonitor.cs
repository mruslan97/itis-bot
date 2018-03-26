using System;
using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules.Interfaces
{
    public interface IGroupsMonitor
    {
        
        event EventHandler UpdatedEvent;
        IEnumerable<IScheduleGroup> AvailableGroups { get; }
        IEnumerable<IScheduleGroup> RemoveInvalidGroupsFrom(IEnumerable<IScheduleGroup> groups);
        bool IsGroupPresent(IScheduleGroup group);
        bool TryGetCorrectGroup(IScheduleGroup sample, out IScheduleGroup correct);
    }
}