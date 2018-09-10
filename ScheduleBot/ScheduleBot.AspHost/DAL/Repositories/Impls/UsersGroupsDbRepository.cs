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
        private readonly ILogger<UsersGroupsDbRepository> logger;
        
        public UsersGroupsDbRepository(UsersContextFactory dbFactory, ILogger<UsersGroupsDbRepository> logger = null)
        {
            this.dbFactory = dbFactory;
            this.logger = logger;
        }
        public async Task<IList<Profile>> GetAllUsersWithGroupsAsync()
        {
            using (var context = dbFactory.CreateDbContext())
            {
                return await context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group).AsNoTracking()
                    .ToListAsync();
            }
        }
        
        /// <inheritdoc cref="IUsersGroupsRepository.FindUserByChatIdAsync"/>
        public async Task<Profile> FindUserByChatIdAsync(long chatId)
        {
            using (var context = dbFactory.CreateDbContext())
            {
                return await context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group).AsNoTracking().FirstOrDefaultAsync(p => p.ChatId == chatId);
            }
        }

        /// <inheritdoc cref="IUsersGroupsRepository.AddGroupToUserAsync"/>
        public async Task AddGroupToUserAsync(Profile user, IScheduleGroup group)
        {
            if (group is ScheduleGroup schGroup)
            {
                if (user.ProfileAndGroups == null)
                    user.ProfileAndGroups = new List<ProfileAndGroup>();
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups.Add(
                        new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id });
                    context.Profiles.Update(user);
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                logger?.LogError($"Unable to handle group of type {group.GetType()}");
                //todo: make cast to ScheduleGroup instead
                throw new ArgumentException("this type of IScheduleGroup is not supported", nameof(group));
            }
        }

        /// <inheritdoc cref="IUsersGroupsRepository.SetSingleGroupToUserAsync"/>
        public async Task SetSingleGroupToUserAsync(Profile user, IScheduleGroup group)
        {
            if (group is ScheduleGroup schGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    if (user.ProfileAndGroups != null && user.ProfileAndGroups.Count > 0)
                        context.ProfileAndGroups.RemoveRange(user.ProfileAndGroups);
                    user.ProfileAndGroups = new List<ProfileAndGroup>()
                        { new ProfileAndGroup() { ProfileId = user.Id, GroupId = schGroup.Id } };
                    context.Profiles.Update(user);
                    await context.SaveChangesAsync();
                }
            else
            {
                logger?.LogError($"Unable to handle group of type {group.GetType()}");
                throw new ArgumentException("this type of IScheduleGroup is not supported", nameof(group));
            }
        }

        /// <inheritdoc cref="IUsersGroupsRepository.ReplaceGroupAsync"/>
        public async Task ReplaceGroupAsync(Profile user, IScheduleGroup oldGroup, IScheduleGroup newGroup)
        {
            if (newGroup is ScheduleGroup schGroup && oldGroup is ScheduleGroup oldSchGroup)
                using (var context = dbFactory.CreateDbContext())
                {
                    user.ProfileAndGroups?.Remove(user.ProfileAndGroups?.First(pg => pg.GroupId == oldSchGroup.Id));
                    context.ProfileAndGroups.Remove(context.ProfileAndGroups.First(pg =>
                        pg.ProfileId == user.Id && pg.GroupId == oldSchGroup.Id));
                    user.ProfileAndGroups.Add(
                        new ProfileAndGroup() { Profile = user, ProfileId = user.Id, Group = schGroup, GroupId = schGroup.Id });
                    context.Profiles.Update(user);
                    await context.SaveChangesAsync();
                }
            else
            {
                logger?.LogError($"Unable to handle group of type {oldGroup.GetType()} or {newGroup.GetType()}");
                throw new ArgumentException("this type of IScheduleGroup is not supported");
            }
        }

        /// <inheritdoc cref="IUsersGroupsRepository.SyncGroupsFromSource"/>
        public async Task SyncGroupsFromSource(IEnumerable<IScheduleGroup> groups, bool throwExceptionOnFall = true)
        {
            try
            {
                var tasks = new List<Task>();
                List<ScheduleGroup> dbGroups;
                using (var mainContext = dbFactory.CreateDbContext())
                {
                    dbGroups = mainContext.Groups.AsNoTracking().ToList();
                }
                var matched = dbGroups.Join(groups, dbg => dbg.Name, group => group.Name,
                    (dbGroup, sourceGroup) => (DbGroup: dbGroup, SourceGroup: sourceGroup)).ToList();
                var toRemove = dbGroups.Except(matched.Select(m => m.DbGroup));
                var toAdd = groups.Except(matched.Select(m => m.SourceGroup));
                tasks.Add(UpdateExistingGroups(matched));
                tasks.Add(RemoveNotUsed(toRemove));
                tasks.Add(AddNewGroups(toAdd));
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception during groups sync with db");
                if (throwExceptionOnFall)
                    throw;
            }
            
        }

        private async Task AddNewGroups(IEnumerable<IScheduleGroup> toAdd)
        {
            using (var context = dbFactory.CreateDbContext())
            {
                context.Groups.AddRange(toAdd.Cast<ScheduleGroup>());
                await context.SaveChangesAsync();
            }
        }

        private async Task RemoveNotUsed(IEnumerable<ScheduleGroup> toRemove)
        {
            using (var context = dbFactory.CreateDbContext())
            {
                context.Groups.RemoveRange(toRemove);
                await context.SaveChangesAsync();
            }
        }

        private async Task UpdateExistingGroups(List<(ScheduleGroup DbGroup, IScheduleGroup SourceGroup)> matched)
        {
            using (var context = dbFactory.CreateDbContext())
            {
                context.Groups.UpdateRange(matched
                    .Select(elem =>
                    {
                        //assign id's from db to source's groups
                        if (elem.SourceGroup is ScheduleGroup castedSourceGroup)
                            castedSourceGroup.Id = elem.DbGroup.Id;
                        return elem;
                    })
                    .Where(elem => elem.DbGroup.GType != elem.SourceGroup.GType)
                    .Select(elem =>
                    {
                        elem.DbGroup.GType = elem.SourceGroup.GType;
                        return elem.DbGroup;
                    }));
                await context.SaveChangesAsync();
            }
        }
    }
}
