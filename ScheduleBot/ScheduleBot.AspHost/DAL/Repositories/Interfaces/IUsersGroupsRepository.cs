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
        Task<IList<Profile>> GetAllUsersWithGroupsAsync();
        Task<Profile> FindUserByChatIdAsync(long chatId);
        Task AddGroupToUserAsync(Profile user, IScheduleGroup group);
        Task SetSingleGroupToUserAsync(Profile user, IScheduleGroup group);
        Task ReplaceGroupAsync(Profile user, IScheduleGroup oldGroup, IScheduleGroup newGroup);
    }
}
