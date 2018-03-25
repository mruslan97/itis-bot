using System;
using System.Collections.Generic;
using ScheduleServices.Core.Models.Comparison;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Week : IScheduleElem, IEquatable<Week>
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Week;
        public ICollection<IScheduleElem> Elems { get; set; }

        public bool Equals(Week other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Level == other.Level && Elems.UnorderEquals(other.Elems);
        }

        public bool Equals(IScheduleElem other)
        {
            if (other.GetType() != this.GetType()) return false;
            return Equals((Week) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((Week) obj);
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