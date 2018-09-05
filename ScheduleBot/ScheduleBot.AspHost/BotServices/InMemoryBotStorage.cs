using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.DAL.Entities;
using ScheduleBot.AspHost.DAL.Repositories.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.BotServices
{
    public class InMemoryBotStorage : IBotDataStorage
    {
        private readonly IScheduleService service;
        private readonly INotifiactionSender notifiactionSender;
        private readonly IUsersGroupsRepository usersGroupsRepository;
        private readonly ILogger<InMemoryBotStorage> logger;

        private readonly ConcurrentDictionary<long, ICollection<IScheduleGroup>> usersGroups =
            new ConcurrentDictionary<long, ICollection<IScheduleGroup>>();

        private readonly ConcurrentDictionary<IScheduleGroup, ICollection<long>> groupToUsers =
            new ConcurrentDictionary<IScheduleGroup, ICollection<long>>();

        private const string XmlFileName = "usersgroups.xml";
        private readonly string path;

        public InMemoryBotStorage(IScheduleService service, INotifiactionSender notifiactionSender, IUsersGroupsRepository usersGroupsRepository,
            ILogger<InMemoryBotStorage> logger = null)
        {
            this.service = service;
            this.notifiactionSender = notifiactionSender;
            this.usersGroupsRepository = usersGroupsRepository;
            this.logger = logger;
            var usersWithGroups = usersGroupsRepository.GetAllUsersWithGroups();
            foreach (var user in usersWithGroups)
            {
                try
                {
                    foreach (var dbGroup in user.ScheduleGroups)
                        if (service.GroupsMonitor.TryFindGroupByName(dbGroup.Name, out var group))
                        {
                            AddGroupToUserInMemory(user.ChatId, group);
                        }
                        else
                        {
                            logger?.LogError(
                                $"no group found of user (chatId: {user.ChatId}) with name: {dbGroup.Name}");
                        }
                }
                catch (Exception e)
                {
                    logger?.LogError(e, $"Failed to restore groups for user (chatId: {user.ChatId})");
                }
            }
        }

        private async void HandleGroupScheduleChanged(object sender, EventArgs args)
        {
            try
            {
                if (args is ParamEventArgs<DayOfWeek> paramEventArgs)
                {
                    var group = (IScheduleGroup) sender;
                    logger?.LogInformation("Get notification about changed sch in group {0}",
                        JsonConvert.SerializeObject(group));
                    if (groupToUsers.TryGetValue(group, out var list) && list != null && list.Any())
                    {
                        logger?.LogInformation("Prepare notification about changed sch in group {0}, users: {1}",
                            JsonConvert.SerializeObject(group), JsonConvert.SerializeObject(list));
                        var dayName = new CultureInfo("ru-Ru").DateTimeFormat.GetDayName(paramEventArgs.Param);
                        var verbEnd = dayName.EndsWith('а') ? "ась" : "ся";
                        await notifiactionSender.SendNotificationsForIdsAsync(list,
                            $"Изменил{verbEnd} {dayName}");
                    }
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Exc");
            }
        }

        public IEnumerable<long> GetAllUsersChatIds()
        {
            return usersGroups.Keys.ToList();
        }

        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(Chat chat)
        {
            return Task.Run(() =>
                usersGroups.TryGetValue(chat.Id, out var groups)
                    ? (IEnumerable<IScheduleGroup>) groups
                    : new List<IScheduleGroup>());
        }

        public Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, Chat chat)
        {
            return Task.Run(() =>
            {
                if (service.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out var groupFromStorage))
                {
                    AddGroupToUserInMemory(chat.Id, groupFromStorage);

                    Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                //var xdoc = XDocument.Load(path);
                                    var user = usersGroupsRepository.FindUserByChatId(chat.Id);//xdoc.Element("users")
                                        //?.Elements("user")
                                        //.FirstOrDefault(u => u.Element("chatId")?.Value == chat.Id.ToString());
                                    if (user == null)
                                        user = new Profile() {ChatId = chat.Id, ProfileAndGroups = new List<ProfileAndGroup>()};
                                    var group = user.ProfileAndGroups?.Select(pg => pg.Group).FirstOrDefault(g =>
                                            g.GType == groupFromStorage.GType);
                                    if (group == null)
                                    {
                                        usersGroupsRepository.AddGroupToUser(user, groupFromStorage);
                                    }
                                    else
                                    {
                                        //if course changed, reset all choosen courses
                                        if (groupFromStorage.GType == ScheduleGroupType.Academic &&
                                            group.Name.Substring(0, group.Name.Length - 2) !=
                                            groupFromStorage.Name.Substring(0, groupFromStorage.Name.Length - 2))
                                            usersGroupsRepository.SetSingleGroupToUser(user, groupFromStorage);
                                        else
                                            usersGroupsRepository.ReplaceGroup(user, group, groupFromStorage);
                                    }

                                
                            }
                            catch (Exception e)
                            {
                                logger?.LogError(e, "Exc");
                            }
                        }, TaskCreationOptions.RunContinuationsAsynchronously).ContinueWith(async t => await t)
                        .ConfigureAwait(false);

                    return true;
                }

                return false;
            });
        }

        private void AddGroupToUserInMemory(long chatId, IScheduleGroup group)
        {
            var clearOther = group.Name.StartsWith("11-");
            IScheduleGroup duplicate = null;
            IList<IScheduleGroup> otherGroups = null;
            usersGroups.AddOrUpdate(chatId, new List<IScheduleGroup> {group}, (id, oldList) =>
            {
                duplicate = oldList.FirstOrDefault(g =>
                    g.GType == group.GType);
                if (duplicate != null)
                {
                    if (!duplicate.Equals(group))
                    {
                        oldList.Remove(duplicate);
                        if (clearOther)
                        {
                            otherGroups = oldList.ToList();
                            oldList.Clear();
                        }

                        oldList.Add(group);
                    }
                }
                else
                {
                    oldList.Add(group);
                }

                return oldList;
            });
            try
            {
                if (!groupToUsers.ContainsKey(group))
                    group.ScheduleChanged += HandleGroupScheduleChanged;
                groupToUsers.AddOrUpdate(group, new List<long> {chatId}, (schGroup, oldList) =>
                {
                    if (!oldList.Contains(chatId))
                        oldList.Add(chatId);
                    return oldList;
                });


                if (duplicate != null && !duplicate.Equals(group))
                {
                    RemoveIdFromGroup(duplicate, chatId);
                    if (clearOther && otherGroups != null && otherGroups.Any())
                        foreach (var otherGroup in otherGroups)
                            RemoveIdFromGroup(otherGroup, chatId);
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Exc");
            }

            void RemoveIdFromGroup(IScheduleGroup rgroup, long id)
            {
                if (groupToUsers.TryGetValue(rgroup, out var list))
                {
                    list.Remove(id);
                    if (!list.Any())
                        groupToUsers.Remove(rgroup, out list);
                }

                if (!groupToUsers.ContainsKey(rgroup))
                    rgroup.ScheduleChanged -= HandleGroupScheduleChanged;
            }
        }
    }
}