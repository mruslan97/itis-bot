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
        private readonly ILogger<InMemoryBotStorage> logger;

        private readonly ConcurrentDictionary<long, ICollection<IScheduleGroup>> usersGroups =
            new ConcurrentDictionary<long, ICollection<IScheduleGroup>>();

        private readonly ConcurrentDictionary<IScheduleGroup, ICollection<long>> groupToUsers =
            new ConcurrentDictionary<IScheduleGroup, ICollection<long>>();

        private const string XmlFileName = "usersgroups.xml";
        private readonly string path;

        public InMemoryBotStorage(IScheduleService service, INotifiactionSender notifiactionSender, ILogger<InMemoryBotStorage> logger = null)
        {
            this.service = service;
            this.notifiactionSender = notifiactionSender;
            this.logger = logger;
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BotServices\\" + XmlFileName;
            try
            {
                var doc = XDocument.Load(path);
                var users = doc.Element("users")?.Elements("user");
                foreach (var user in users)
                {
                    var groups = user.Element("groups").Elements("group");
                    foreach (var subGroup in groups)
                        if (service.GroupsMonitor.TryFindGroupByName(subGroup.Attribute("name").Value, out var group))
                        {
                            if (long.TryParse(user.Element("chatId").Value, out var chatId))
                                AddGroupToUserInMemory(chatId, group);
                        }
                        else
                        {
                            logger?.LogError(
                                $"no group found of user {user.Element("chatId").Value} with name: {subGroup.Attribute("name").Value}");
                        }
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Exc");
                logger?.LogWarning("but i'm alive");
            }
        }

        private async void HandleGroupScheduleChanged(object sender, EventArgs args)
        {
            try
            {
                
                if (args is ParamEventArgs<DayOfWeek> paramEventArgs)
                {
                    var group = (IScheduleGroup) sender;
                    logger?.LogInformation("Get notification about changed sch in group {0}", JsonConvert.SerializeObject(group));
                    if (groupToUsers.TryGetValue(group, out var list) && list != null && list.Any())
                    {
                        logger?.LogInformation("Prepare notification about changed sch in group {0}, users: {1}", JsonConvert.SerializeObject(group), JsonConvert.SerializeObject(list));
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
                                var xdoc = XDocument.Load(path);
                                var user = xdoc.Element("users")
                                    ?.Elements("user")
                                    .FirstOrDefault(u => u.Element("chatId")?.Value == chat.Id.ToString());
                                if (user == null)
                                    xdoc.Element("users")?.Add(new XElement("user",
                                        new XAttribute("name", chat.FirstName),
                                        new XElement("chatId", chat.Id.ToString()), new XElement("groups")));
                                var group = user?.Element("groups")
                                    ?.Elements("group").FirstOrDefault(g =>
                                        g.Attribute("type")?.Value == groupFromStorage.GType.ToString());
                                if (group == null)
                                {
                                    xdoc.Element("users")
                                        ?.Elements("user")
                                        .FirstOrDefault(u => u.Element("chatId")?.Value == chat.Id.ToString())
                                        ?.Element("groups")
                                        ?.Add(new XElement("group",
                                            new XAttribute("type", groupFromStorage.GType.ToString()),
                                            new XAttribute("name", groupFromStorage.Name)));
                                }
                                else
                                {
                                    if (groupFromStorage.GType == ScheduleGroupType.Academic &&
                                        group.Attribute("name").Value
                                            .Substring(0, group.Attribute("name").Value.Length - 1) !=
                                        groupFromStorage.Name.Substring(0, groupFromStorage.Name.Length - 1))
                                    {
                                        xdoc.Element("users").Elements("user")
                                            .FirstOrDefault(u => u.Element("chatId").Value == chat.Id.ToString())
                                            .Element("groups").Elements("group")
                                            .Where(g => g.Attribute("type").Value != "Academic").Remove();
                                    }

                                    xdoc.Element("users").Elements("user")
                                        .FirstOrDefault(u => u.Element("chatId").Value == chat.Id.ToString())
                                        .Element("groups").Elements("group").FirstOrDefault(g =>
                                            g.Attribute("type").Value == groupFromStorage.GType.ToString())
                                        .Attribute("name")
                                        .Value = groupFromStorage.Name;
                                }

                                xdoc.Save(path);
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
                        {
                            RemoveIdFromGroup(otherGroup, chatId);
                        }
                    
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