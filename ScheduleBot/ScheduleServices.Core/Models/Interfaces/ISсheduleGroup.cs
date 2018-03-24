using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Models.Interfaces
{
    public enum ScheduleGroupType
    {
        Undefined = 0,
        Academic = 1,
        Eng = 2, 
        PickedScientic = 3,
        PickedTech = 4
    }
    public interface IScheduleGroup
    {
        ScheduleGroupType GType { get; set; }
        string Name { get; set; }
    }
}
