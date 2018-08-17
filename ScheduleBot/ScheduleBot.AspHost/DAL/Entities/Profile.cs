using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using DAL.Common;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost.DAL.Entities
{

    public class Profile : Entity
    {
        public string Name { get; set; }

        public ICollection<ProfileAndGroup> ProfileAndGroups { get; set; }
        [NotMapped]
        public IEnumerable<ScheduleGroup> ScheduleGroups
        {
            get { return ProfileAndGroups.Select(pg => pg.Group); }
        }
    }
    
}
