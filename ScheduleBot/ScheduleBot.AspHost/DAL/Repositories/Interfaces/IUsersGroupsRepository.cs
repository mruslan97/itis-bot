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
        /// <summary>
        /// Returns all users and all their groups in navigation properties
        /// </summary>
        /// <returns>All users with their groups from storage</returns>
        Task<IList<Profile>> GetAllUsersWithGroupsAsync();

        /// <summary>
        /// Returns user with given chatId with all his groups
        /// </summary>
        /// <param name="chatId">User's chatId</param>
        /// <returns>Profile of user with all groups</returns>
        Task<Profile> FindUserByChatIdAsync(long chatId);

        /// <summary>
        /// Just adds group to list of user's groups.
        /// If there is no such group yet, it will be added.
        /// </summary>
        /// <param name="user">Profile of user</param>
        /// <param name="group">Group to add</param>
        /// <returns></returns>
        Task AddGroupToUserAsync(Profile user, IScheduleGroup group);

        /// <summary>
        /// Removes all groups from user and add single to his groups.
        /// If there is no such group yet, it will be added.
        /// </summary>
        /// <param name="user">Profile of user</param>
        /// <param name="group">Group to be added</param>
        /// <returns></returns>
        Task SetSingleGroupToUserAsync(Profile user, IScheduleGroup group);

        /// <summary>
        /// Replaces specified group by another for user.
        /// If there is no such new group yet, it will be added.
        /// </summary>
        /// <param name="user">Profile of user</param>
        /// <param name="oldGroup">Group which will be replaced</param>
        /// <param name="newGroup">Group which will replace</param>
        /// <returns></returns>
        Task ReplaceGroupAsync(Profile user, IScheduleGroup oldGroup, IScheduleGroup newGroup);

        /// <summary>
        /// Takes source and compare it with stored groups. Groups will be removed from storage,
        /// if they not exist in source. Groups will be added to storage, if they exist in source but not in storage.
        /// Groups will be updated if they match by predefined key: stored groups will take info, source groups will take storage id's
        /// </summary>
        /// <param name="groups">Source groups list</param>
        /// <param name="throwExceptionOnFall">Flag to throw exception on fail.</param>
        /// <returns></returns>
        Task SyncGroupsFromSource(IEnumerable<IScheduleGroup> groups, bool throwExceptionOnFall = true);
    }
}
