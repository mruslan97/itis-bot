using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PropertyChanged;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleGroups
{
    [AddINotifyPropertyChangedInterface]
    public class ScheduleGroup : IScheduleGroup, IEquatable<ScheduleGroup>
    {
        //[JsonConverter(typeof(StringEnumConverter))] 
        //[BsonRepresentation(BsonType.String)]
        
        public ScheduleGroupType GType { get; set; }
        
        
        public string Name { get; set; }

        public bool Equals(ScheduleGroup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GType == other.GType && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ScheduleGroup) obj);
        }

        public bool Equals(IScheduleGroup obj)
        {
            
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ScheduleGroup)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) GType * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public object Clone()
        {
            return new ScheduleGroup()
            {
                GType = this.GType,
                Name = (string)this.Name.Clone()
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
    }
}