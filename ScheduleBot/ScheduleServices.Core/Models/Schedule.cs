using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ScheduleServices.Core.Models.Comparison;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Providers.Storage;

namespace ScheduleServices.Core.Models
{
    [BsonKnownTypes(typeof(SingleGroupSchedule))]
    public class Schedule : ISchedule, IEquatable<Schedule>
    {
        public ICollection<IScheduleGroup> ScheduleGroups { get; set; }
        public IScheduleElem ScheduleRoot { get; set; }

        public bool Equals(Schedule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ScheduleGroups.UnorderEquals(other.ScheduleGroups) && Equals(ScheduleRoot, other.ScheduleRoot);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Schedule) obj);
        }
        public  bool Equals(ISchedule obj)
        {
            
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Schedule)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ScheduleGroups != null ? ScheduleGroups.GetHashCode() : 0) * 397) ^ (ScheduleRoot != null ? ScheduleRoot.GetHashCode() : 0);
            }
        }
    }
}