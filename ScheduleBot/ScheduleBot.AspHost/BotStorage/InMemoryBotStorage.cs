using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.BotStorage
{
    public class InMemoryBotStorage : IBotDataStorage
    {
        private readonly IScheduleServise servise;

        private readonly ConcurrentDictionary<long, ICollection<IScheduleGroup>> usersGroups =
            new ConcurrentDictionary<long, ICollection<IScheduleGroup>>();

        private const string XmlFileName = "usersgroups.xml";
        private readonly string path;

        public InMemoryBotStorage(IScheduleServise servise)
        {
            this.servise = servise;
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
                            usersGroups.AddOrUpdate(Convert.ToInt32(user.Element("chatId").Value),
                                new List<IScheduleGroup> { group },
                                (id, old) =>
                                {
                                    old.Add(group);
                                    return old;
                                });
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


        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(Chat chat)
        {
            return Task.Run(() =>
            {
                usersGroups.TryGetValue(chat.Id, out var groups);
                return (IEnumerable<IScheduleGroup>) groups;
            });
        }

        public async Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, Chat chat)
        {
            if (servise.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out var groupFromStorage))
            {
                IScheduleGroup duplicate = null;
                usersGroups.AddOrUpdate(chat.Id, new List<IScheduleGroup> {groupFromStorage}, (id, oldList) =>
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

                return true;
            }

            return false;
        }
    }
}