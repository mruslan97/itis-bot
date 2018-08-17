using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Common;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.DAL.Entities
{

    public class Profile : Entity
    {
        public string Name { get; set; }
    }
    
}
