using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleServices.Core.Models.Comparison;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleElems
{
    
    public class Lesson : IScheduleElem, IEquatable<Lesson>
    {
        public ScheduleElemLevel Level { get; set; } = ScheduleElemLevel.Lesson;
        public ICollection<IScheduleElem> Elems { get; set; } = null;

        public bool? IsOnEvenWeek { get; set; } = null;

        public string Discipline { get; set; }

        public string Teacher { get; set; }

        public string Place { get; set; }
        public string Notation { get; set; }

        public TimeSpan BeginTime { get; set; }

        public TimeSpan Duration { get; set; }

        public bool Equals(Lesson other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Level == other.Level && IsOnEvenWeek == other.IsOnEvenWeek &&
                   string.Equals(Discipline, other.Discipline) && string.Equals(Teacher, other.Teacher) &&
                   string.Equals(Place, other.Place) && BeginTime.Equals(other.BeginTime) &&
                   string.Equals(Notation, other.Notation) &&
                   Duration.Equals(other.Duration) && Elems.UnorderEquals(other.Elems);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Lesson) obj);
        }
        public bool Equals(IScheduleElem obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Lesson)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Level;
                hashCode = (hashCode * 397) ^ (Elems != null ? Elems.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsOnEvenWeek.GetHashCode();
                hashCode = (hashCode * 397) ^ (Discipline != null ? Discipline.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Teacher != null ? Teacher.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Place != null ? Place.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ BeginTime.GetHashCode();
                hashCode = (hashCode * 397) ^ Duration.GetHashCode();
                hashCode = (hashCode * 397) ^ Notation.GetHashCode();
                return hashCode;
            }
        }
        public object Clone()
        {
            return new Lesson()
            {
                Teacher = this.Teacher,
                BeginTime = this.BeginTime,
                Discipline = this.Discipline,
                Duration = Duration,
                IsOnEvenWeek = IsOnEvenWeek,
                Place = Place,
                Notation = Notation,
                Level = this.Level,
                Elems = null
            };
        }
    }
}