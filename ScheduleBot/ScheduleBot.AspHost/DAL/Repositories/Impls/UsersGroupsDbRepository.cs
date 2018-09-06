using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.DAL.Entities;
using ScheduleBot.AspHost.DAL.Repositories.Interfaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost.DAL.Repositories.Impls
{
    public class UsersGroupsDbRepository : IUsersGroupsRepository
    {
        private readonly UsersContextFactory dbFactory;

        public UsersGroupsDbRepository(UsersContextFactory dbFactory, ILogger<UsersGroupsDbRepository> logger = null)
        {
            this.dbFactory = dbFactory;
        }
        public Task<IList<Profile>> GetAllUsersWithGroupsAsync()
        {
            using (var context = dbFactory.CreateDbContext())
            {
                return context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group)
                    .ToListAsync().ContinueWith(t => (IList<Profile>) t.Result);
            }
        }

        public Task<Profile> FindUserByChatIdAsync(long chatId)
        {
            using (var context = dbFactory.CreateDbContext())
            {
                return context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group).FirstOrDefaultAsync(p => p.ChatId == chatId);
            }
        }

        public async Task AddGroupToUserAsync(Profile user, IScheduleGroup group)
        {
            if (group is ScheduleGroup schGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups.Add(
                        new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id });
                    context.Profiles.Update(user);
                    await context.SaveChangesAsync();
                }
            else
            {
                //todo: make informative
                throw new ArgumentException("group");
            }
        }

        public async Task SetSingleGroupToUserAsync(Profile user, IScheduleGroup group)
        {
            if (group is ScheduleGroup schGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups = new List<ProfileAndGroup>()
                        { new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id } };
                    context.Profiles.Update(user);
                    await context.SaveChangesAsync();
                }
            else
            {
                throw new ArgumentException("group");
            }
        }

        public async Task ReplaceGroupAsync(Profile user, IScheduleGroup oldGroup, IScheduleGroup newGroup)
        {
            if (newGroup is ScheduleGroup schGroup && oldGroup is ScheduleGroup oldSchGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups.Remove(user.ProfileAndGroups.First(pg => pg.GroupId == oldSchGroup.Id));
                    user.ProfileAndGroups.Add(
                        new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id });
                    context.Profiles.Update(user);
                    await context.SaveChangesAsync();
                }
            else
            {
                throw new ArgumentException("group");
            }
        }
    }
}
