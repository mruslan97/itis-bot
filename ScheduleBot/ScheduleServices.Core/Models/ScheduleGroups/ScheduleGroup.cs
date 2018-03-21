using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleGroups
{
    public class ScheduleGroup : IScheduleGroup
    {
        public ScheduleGroupType GType { get; set; }
        public string Name { get; set; }
    }
}