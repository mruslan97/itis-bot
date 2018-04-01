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
                var elements = doc.Element("users")?.Elements("user");
                foreach (var element in elements)
                    if (servise.GroupsMonitor.TryFindGroupByName(element.Element("value").Value, out var group))
                        usersGroups.AddOrUpdate(Convert.ToInt32(element.Element("chatId").Value),
                            new List<IScheduleGroup> {group},
                            (id, old) =>
                            {
                                old.Add(group);
                                return old;
                            });
                    else
                        Console.Out.WriteLine(
                            $"no group found of user {element.Element("chatId").Value} with name: {element.Element("value").Value}");
                //if (doc.Root != null)
                //    foreach (var element in doc.Root.Elements())
                //        if (long.TryParse(element.Attribute("ID")?.Value, out var chatId))
                //            foreach (var relationToGroup in element.Elements())
                //            {
                //                var groupname = relationToGroup.Attribute("GNAME")?.Value ?? "";
                //                if (servise.GroupsMonitor.TryFindGroupByName(groupname, out var group))
                //                    usersGroups.AddOrUpdate(chatId, new List<IScheduleGroup> {group},
                //                        (id, old) =>
                //                        {
                //                            old.Add(group);
                //                            return old;
                //                        });
                //                else
                //                    Console.Out.WriteLine($"no group found of user {chatId} with name: {groupname}");
                //            }
                //else
                //    Console.Out.WriteLine("doc load fail - no root");
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
                    else
                        oldList.Add(groupFromStorage);
                    return oldList;
                });
                try
                {
                    var xdoc = XDocument.Load(path);
                    var element = xdoc.Element("users")
                        ?.Elements("user").Where(u =>
                            u.Element("chatId")?.Value == chat.Id.ToString() &&
                            u.Element("groupType")?.Value == groupFromStorage.GType.ToString());
                    if (element == null)
                        xdoc.Element("users")
                            .Add(new XElement("user",
                                new XAttribute("name", chat.FirstName),
                                new XElement("groupType", groupFromStorage.GType.ToString()),
                                new XElement("chatId", chat.Id),
                                new XElement("value", groupFromStorage.Name)));
                    else
                        (xdoc.Element("users")
                                ?.Elements("user")).SingleOrDefault(u =>
                                u.Element("chatId")?.Value == chat.Id.ToString() &&
                                u.Element("groupType")?.Value ==
                                groupFromStorage.GType.ToString()).Element("value")
                            .Value = groupFromStorage.Name;
                    //else
                    //    (xdoc.Element("users")
                    //            ?.Elements("user")).SingleOrDefault(u =>
                    //            u.Element("chatId")?.Value == chat.Id.ToString() &&
                    //            u.Element("groupType")?.Value ==
                    //            groupFromStorage.GType.ToString()).Element("value")
                    //        .Value = groupFromStorage.Name;
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