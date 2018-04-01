using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules;

namespace ScheduleBot.AspHost.BotStorage
{
    public class InMemoryBotStorage : IBotDataStorage
    {
        private readonly IScheduleServise servise;

        private ConcurrentDictionary<long, ICollection<IScheduleGroup>> usersGroups =
            new ConcurrentDictionary<long, ICollection<IScheduleGroup>>();

        private const string XmlFileName = "usersgroups.xml";
        private string path;

        public InMemoryBotStorage(IScheduleServise servise)
        {
            this.servise = servise;
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BotStorage\\" + XmlFileName;
            try
            {
                XDocument doc = XDocument.Load(path);
                if (doc.Root != null)
                {
                    foreach (var element in doc.Root.Elements())
                    {
                        if (long.TryParse(element.Attribute("ID")?.Value, out long chatId))
                        {
                            foreach (var relationToGroup in element.Elements())
                            {
                                var groupname = (relationToGroup.Attribute("GNAME")?.Value ?? "");
                                if (servise.GroupsMonitor.TryFindGroupByName(groupname, out IScheduleGroup group))
                                {
                                    usersGroups.AddOrUpdate(chatId, new List<IScheduleGroup>() {group},
                                        (id, old) =>
                                        {
                                            old.Add(group);
                                            return old;
                                        });
                                }
                                else
                                {
                                    Console.Out.WriteLine($"no group found of user {chatId} with name: {groupname}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.Out.WriteLine("doc load fail - no root");
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.Out.WriteLine("but i'm alive");
            }
        }


        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(long chatId)
        {
            return Task.Run(() =>
            {
                usersGroups.TryGetValue(chatId, out ICollection<IScheduleGroup> groups);
                return (IEnumerable<IScheduleGroup>) groups;
            });
        }

        public async Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, long chatId)
        {
            if (servise.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out IScheduleGroup groupFromStorage))
            {
                IScheduleGroup duplicate = null;
                usersGroups.AddOrUpdate(chatId, new List<IScheduleGroup>() {groupFromStorage}, (id, oldList) =>
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
                    XDocument doc = XDocument.Load(path);
                    var idNode = doc.Root?.Elements()?.FirstOrDefault(e => e.Attribute("ID")?.Value == chatId.ToString());
                    if (duplicate != null)
                    {
                        if (idNode != null)
                        {
                            var duplNode = idNode.Elements().FirstOrDefault(n =>
                                (n.Attribute("GTYPE")?.Value ?? "") == duplicate.GType.ToString() &&
                                (n.Attribute("GNAME")?.Value ?? "") != duplicate.Name);
                            duplNode?.Attribute("GNAME")?.SetValue(groupFromStorage.Name);
                        }
                    }
                    else
                    {
                        if (idNode == null)
                        {
                            idNode = new XElement(XName.Get("user", doc.Root.Name.ToString()));
                            idNode.Attribute("ID")?.SetValue(chatId.ToString());
                            doc.Root.Add(idNode);
                        }

                        var newElem = new XElement("GROUP", idNode.ToString());
                        idNode.Add(newElem);
                    }

                    using (XmlTextWriter xtw = new XmlTextWriter(path, Encoding.UTF8))
                    {
                        await doc.SaveAsync(xtw, CancellationToken.None);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}