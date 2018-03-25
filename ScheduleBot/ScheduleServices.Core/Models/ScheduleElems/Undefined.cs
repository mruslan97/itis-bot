using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Undefined : IScheduleElem, IEquatable<Undefined>
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Undefined;
        public ICollection<IScheduleElem> Elems { get; set; }

        public bool Equals(Undefined other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Level == other.Level && Equals(Elems, other.Elems);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Undefined) obj);
        }
        public bool Equals(IScheduleElem obj)
        {
            
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Undefined)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Level * 397) ^ (Elems != null ? Elems.GetHashCode() : 0);
            }
        }
    }
}
