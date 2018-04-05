using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.BotServices
{
    public class InMemoryBotStorage : IBotDataStorage
    {
        private readonly IScheduleServise servise;
        private readonly INotifiactionSender notifiactionSender;

        private readonly ConcurrentDictionary<long, ICollection<IScheduleGroup>> usersGroups =
            new ConcurrentDictionary<long, ICollection<IScheduleGroup>>();

        private readonly ConcurrentDictionary<IScheduleGroup, ICollection<long>> groupToUsers =
            new ConcurrentDictionary<IScheduleGroup, ICollection<long>>();

        private const string XmlFileName = "usersgroups.xml";
        private readonly string path;

        public InMemoryBotStorage(IScheduleServise servise, INotifiactionSender notifiactionSender)
        {
            this.servise = servise;
            this.notifiactionSender = notifiactionSender;
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BotServices\\" + XmlFileName;
            try
            {
                var doc = XDocument.Load(path);
                var users = doc.Element("users")?.Elements("user");
                foreach (var user in users)
                {
                    var groups = user.Element("groups").Elements("group");
                    foreach (var subGroup in groups)
                        if (servise.GroupsMonitor.TryFindGroupByName(subGroup.Attribute("name").Value, out var group))
                        {
                            if (long.TryParse(user.Element("chatId").Value, out var chatId))
                                AddGroupToUserInMemory(chatId, group);
                        }
                        else
                        {
                            Console.Out.WriteLine(
                                $"no group found of user {user.Element("chatId").Value} with name: {subGroup.Attribute("name").Value}");
                        }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.Out.WriteLine("but i'm alive");
            }
        }

        private async void HandleGroupScheduleChanged(object sender, EventArgs args)
        {
            try
            {
                if (args is ParamEventArgs<DayOfWeek> paramEventArgs)
                {
                    var group = (IScheduleGroup) sender;
                    if (groupToUsers.TryGetValue(group, out var list) && list != null && list.Any())
                        await notifiactionSender.SendNotificationsForIdsAsync(list,
                            $"Изменилась {new CultureInfo("ru-Ru").DateTimeFormat.GetDayName(paramEventArgs.Param)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(Chat chat)
        {
            return Task.Run(() => usersGroups.TryGetValue(chat.Id, out var groups) ? (IEnumerable<IScheduleGroup>) groups : new List<IScheduleGroup>());
        }

        public Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, Chat chat)
        {
            return Task.Run(() =>
            {
                if (servise.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out var groupFromStorage))
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
                                Console.WriteLine(e);
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
                    g.GType == group.GType && !g.Equals(group));
                if (duplicate != null && !duplicate.Equals(group))
                {
                    oldList.Remove(duplicate);
                    if (clearOther)
                    {
                        otherGroups = oldList.ToList();
                        oldList.Clear();
                    }
                }

                oldList.Add(group);
                return oldList;
            });
            try
            {
                groupToUsers.AddOrUpdate(group, new List<long> {chatId}, (schGroup, oldList) =>
                {
                    oldList.Add(chatId);
                    return oldList;
                });
                group.ScheduleChanged += HandleGroupScheduleChanged;
                if (duplicate != null && !duplicate.Equals(group))
                {
                    duplicate.ScheduleChanged -= HandleGroupScheduleChanged;
                    if (groupToUsers.TryGetValue(duplicate, out var list)) list.Remove(chatId);
                    if (clearOther && otherGroups != null && otherGroups.Any())
                        foreach (var otherGroup in otherGroups)
                        {
                            otherGroup.ScheduleChanged -= HandleGroupScheduleChanged;
                            if (groupToUsers.TryGetValue(duplicate, out var subs)) subs.Remove(chatId);
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}