using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public InMemoryBotStorage(IScheduleServise servise)
        {
            this.servise = servise;
        }

        

        

        public Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(long chatId)
        {
            return Task.Run(() =>
            {
                usersGroups.TryGetValue(chatId, out ICollection<IScheduleGroup> groups);
                return (IEnumerable<IScheduleGroup>)groups;
            });
        }

        public Task AddGroupToChat(IScheduleGroup scheduleGroup, long chatId)
        {
            return Task.Run(() =>
            {
                if (servise.GroupsMonitor.TryGetCorrectGroup(scheduleGroup, out IScheduleGroup groupFromStorage))
                {
                    usersGroups.AddOrUpdate(chatId, new List<IScheduleGroup>() {groupFromStorage}, (id, oldList) =>
                    {
                        oldList.Add(groupFromStorage);
                        return oldList;
                    });
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            });
        }

        
    }
}