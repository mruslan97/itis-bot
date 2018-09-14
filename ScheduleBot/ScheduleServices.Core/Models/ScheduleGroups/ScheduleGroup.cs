using System;
using System.ComponentModel;
using System.Xml;
using DAL.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PropertyChanged;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Models.ScheduleGroups
{
    [AddINotifyPropertyChangedInterface]
    public class ScheduleGroup : Entity, IScheduleGroup
    {
        //[JsonConverter(typeof(StringEnumConverter))] 
        //[BsonRepresentation(BsonType.String)]
        
        public ScheduleGroupType GType { get; set; }
        
        
        public string Name { get; set; }
        public event EventHandler ScheduleChanged;
        public void RaiseScheduleChanged(object sender, EventArgs args)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = ScheduleChanged;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, args);
            }
        }
        

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
                Id = this.Id,
                GType = this.GType,
                Name = (string)this.Name.Clone()
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        
    }
}