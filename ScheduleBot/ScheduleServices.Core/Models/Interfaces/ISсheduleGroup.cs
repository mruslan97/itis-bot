using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Models.Interfaces
{
    public enum ScheduleGroupType
    {
        Academic = 0,
        Eng = 1, 
        PickedScientic = 2,
        PickedTech = 3
    }
    public interface IScheduleGroup
    {
        ScheduleGroupType GType { get; set; }
        string Name { get; set; }
    }
}
