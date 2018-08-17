using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using DAL.Common;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost.DAL.Entities
{
    /// <summary>
    /// Technical class to store many to many relations between group and profiles
    /// </summary>
    public class ProfileAndGroup : Entity
    {
        public long ProfileId { get; set; }
        [ForeignKey("ProfileId")]
        public Profile Profile { get; set; }

        public long GroupId { get; set; }
        [ForeignKey("GroupId")]
        public ScheduleGroup Group { get; set; }
    }
}
