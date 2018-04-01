using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private const string ResourceName = "usersGroupBackup";

        public InMemoryBotStorage(IScheduleServise servise)
        {
            this.servise = servise;
        }


        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(long chatId)
        {
            return Task.Run(() =>
            {
                usersGroups.TryGetValue(chatId, out ICollection<IScheduleGroup> groups);
                return (IEnumerable<IScheduleGroup>) groups;
            });
        }

        public bool TryAddGroupToChat(IScheduleGroup scheduleGroup, long chatId)
        {
            if (servise.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out IScheduleGroup groupFromStorage))
            {
                bool success = true;
                usersGroups.AddOrUpdate(chatId, new List<IScheduleGroup>() {groupFromStorage}, (id, oldList) =>
                {
                    if (oldList.Any(g => g.GType == groupFromStorage.GType && !g.Equals(groupFromStorage)))
                        success = false;
                    else
                        oldList.Add(groupFromStorage);
                    return oldList;
                });
                //todo: backup
                return success;
            }
            else
            {
                return false;
            }
        }
    }
}