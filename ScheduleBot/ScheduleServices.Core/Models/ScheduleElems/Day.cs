using System;
using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    public class Day : IScheduleElem, IEquatable<Day>
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Day;
        public ICollection<IScheduleElem> Elems { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public bool Equals(Day other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Level == other.Level && Equals(Elems, other.Elems) && DayOfWeek == other.DayOfWeek;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Day) obj);
        }
        public bool Equals(IScheduleElem obj)
        {
            return Equals((object)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Level;
                hashCode = (hashCode * 397) ^ (Elems != null ? Elems.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) DayOfWeek;
                return hashCode;
            }
        }
    }
}