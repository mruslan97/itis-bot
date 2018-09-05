using System;
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
        public IList<Profile> GetAllUsersWithGroups()
        {
            using (var context = dbFactory.CreateDbContext())
            {
                return context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group).ToList();
            }
        }

        public Profile FindUserByChatId(long chatId)
        {
            using (var context = dbFactory.CreateDbContext())
            {
                return context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group).FirstOrDefault(p => p.ChatId == chatId);
            }
        }

        public void AddGroupToUser(Profile user, IScheduleGroup group)
        {
            if (group is ScheduleGroup schGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups.Add(
                        new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id });
                    context.Profiles.Update(user);
                    //todo: make async
                    context.SaveChangesAsync();
                }
            else
            {
                //todo: make informative
                throw new ArgumentException("group");
            }
        }

        public void SetSingleGroupToUser(Profile user, IScheduleGroup group)
        {
            if (group is ScheduleGroup schGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups = new List<ProfileAndGroup>()
                        { new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id } };
                    context.Profiles.Update(user);
                    context.SaveChangesAsync();
                }
            else
            {
                throw new ArgumentException("group");
            }
        }

        public void ReplaceGroup(Profile user, IScheduleGroup oldGroup, IScheduleGroup newGroup)
        {
            if (newGroup is ScheduleGroup schGroup && oldGroup is ScheduleGroup oldSchGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups.Remove(user.ProfileAndGroups.First(pg => pg.GroupId == oldSchGroup.Id));
                    user.ProfileAndGroups.Add(
                        new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id });
                    context.Profiles.Update(user);
                    context.SaveChangesAsync();
                }
            else
            {
                throw new ArgumentException("group");
            }
        }
    }
}
