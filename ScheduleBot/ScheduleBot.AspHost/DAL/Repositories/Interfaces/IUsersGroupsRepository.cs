using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.DAL.Entities;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost.DAL.Repositories.Interfaces
{
    public interface IUsersGroupsRepository
    {
        IList<Profile> GetAllUsersWithGroups();
        Profile FindUserByChatId(long chatId);
        void AddGroupToUser(Profile user, IScheduleGroup group);
        void SetSingleGroupToUser(Profile user, IScheduleGroup group);
        void ReplaceGroup(Profile user, IScheduleGroup oldGroup, IScheduleGroup newGroup);
    }
}
