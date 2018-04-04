using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
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
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BotStorage\\" + XmlFileName;
            try
            {
                var doc = XDocument.Load(path);
                var users = doc.Element("users")?.Elements("user");
                // todo check for null???
                foreach (var user in users)
                {
                    var groups = user.Element("groups").Elements("group");
                    foreach (var subGroup in groups)
                    {
                        if (servise.GroupsMonitor.TryFindGroupByName(subGroup.Attribute("name").Value, out var group))
                        {
                            if (long.TryParse(user.Element("chatId").Value, out long chatId))
                            {
                                usersGroups.AddOrUpdate(chatId,
                                    new List<IScheduleGroup> { group },
                                    (id, old) =>
                                    {
                                        old.Add(group);
                                        return old;
                                    });
                                groupToUsers.AddOrUpdate(group, new List<long>() {chatId}, (schGroup, oldList) =>
                                {
                                    oldList.Add(chatId);
                                    return oldList;
                                });
                                group.ScheduleChanged += HandleGroupScheduleChanged;
                            }
                            
                        }
                        else
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
                var group = (IScheduleGroup) sender;
                if (groupToUsers.TryGetValue(group, out var list) && list != null && list.Any())
                    await notifiactionSender.SendNotificationsForIdsAsync(list);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(Chat chat)
        {
            return Task.Run(() =>
            {
                usersGroups.TryGetValue(chat.Id, out var groups);
                return (IEnumerable<IScheduleGroup>) groups;
            });
        }

        public Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, Chat chat)
        {
            return Task.Run(() =>
            {
                if (servise.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out var groupFromStorage))
                {
                    IScheduleGroup duplicate = null;
                    usersGroups.AddOrUpdate(chat.Id, new List<IScheduleGroup> { groupFromStorage }, (id, oldList) =>
                    {
                        duplicate = oldList.FirstOrDefault(g =>
                            g.GType == groupFromStorage.GType && !g.Equals(groupFromStorage));
                        if (duplicate != null)
                            oldList.Remove(duplicate);
                        oldList.Add(groupFromStorage);
                        return oldList;
                    });
                    try
                    {
                        groupToUsers.AddOrUpdate(groupFromStorage, new List<long>() { chat.Id }, (schGroup, oldList) =>
                        {
                            oldList.Add(chat.Id);
                            return oldList;
                        });
                        groupFromStorage.ScheduleChanged += HandleGroupScheduleChanged;
                        if (duplicate != null)
                        {
                            duplicate.ScheduleChanged -= HandleGroupScheduleChanged;
                            if (groupToUsers.TryGetValue(duplicate, out var list))
                            {
                                list.Remove(chat.Id);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var xdoc = XDocument.Load(path);
                            var user = xdoc.Element("users")
                                ?.Elements("user").FirstOrDefault(u => u.Element("chatId")?.Value == chat.Id.ToString());
                            if (user == null)
                                xdoc.Element("users")?.Add(new XElement("user", new XAttribute("name", chat.FirstName),
                                    new XElement("chatId", chat.Id.ToString()), new XElement("groups")));
                            var group = user?.Element("groups")
                                ?.Elements("group").FirstOrDefault(g =>
                                    g.Attribute("type")?.Value == groupFromStorage.GType.ToString());
                            if (group == null)
                                xdoc.Element("users")
                                    ?.Elements("user")
                                    .FirstOrDefault(u => u.Element("chatId")?.Value == chat.Id.ToString())
                                    ?.Element("groups")
                                    ?.Add(new XElement("group", new XAttribute("type", groupFromStorage.GType.ToString()),
                                        new XAttribute("name", groupFromStorage.Name)));
                            else
                                xdoc.Element("users").Elements("user")
                                    .FirstOrDefault(u => u.Element("chatId").Value == chat.Id.ToString())
                                    .Element("groups").Elements("group").FirstOrDefault(g =>
                                        g.Attribute("type").Value == groupFromStorage.GType.ToString()).Attribute("name")
                                    .Value = groupFromStorage.Name;
                            xdoc.Save(path);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }, TaskCreationOptions.RunContinuationsAsynchronously).ContinueWith(async (t) => await t).ConfigureAwait(false);
                    
                    return true;
                }

                return false;
            });
            
        }
    }
}