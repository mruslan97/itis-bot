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
        /// <summary>
        /// try to find group with specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resultGroup"></param>
        /// <returns>true if group with such name exists and this group, and false if not</returns>
        bool TryFindGroupByName(string name, out IScheduleGroup resultGroup);

        IEnumerable<IScheduleGroup> GetAllowedGroups(ScheduleGroupType ofType, IScheduleGroup target);
    }
}