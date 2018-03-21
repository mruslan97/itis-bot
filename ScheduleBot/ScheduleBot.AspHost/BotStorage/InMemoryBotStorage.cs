using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.BotStorage
{
    public class InMemoryBotStorage : IBotDataStorage
    {
        private readonly IScheduleServise scheduleServise;

        private ConcurrentDictionary<string, IScheduleGroup> allGroups =
            new ConcurrentDictionary<string, IScheduleGroup>();

        private ConcurrentDictionary<long, ICollection<IScheduleGroup>> usersGroups =
            new ConcurrentDictionary<long, ICollection<IScheduleGroup>>();

        public InMemoryBotStorage(IScheduleServise scheduleServise)
        {
            this.scheduleServise = scheduleServise;
            this.scheduleServise.UpdatedEvent += OnScheduleServiceUpdated;
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
                if (allGroups.TryGetValue(scheduleGroup.Name, out IScheduleGroup groupFromStorage))
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

        private async void OnScheduleServiceUpdated(object sender, EventArgs e)
        {
            // because this method is 'async void' using try-catch to not miss exception
            try
            {
                var groups = await scheduleServise.GetAvailibleGroupsAsync();
                foreach (var group in groups)
                {
                    allGroups.AddOrUpdate(group.Name, group, (name, oldGroup) =>
                    {
                        oldGroup.GType = group.GType;
                        oldGroup.Name = group.Name;
                        return oldGroup;
                    });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            
        }
    }
}