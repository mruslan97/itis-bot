﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using System.Collections.Concurrent;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core
{
    public class ScheduleService : IScheduleServise
    {
        private readonly ISchedulesStorage storage;
        private readonly ScheduleConstructor scheduleConstructor;
        public IGroupsMonitor GroupsMonitor { get; }
        private IScheduleInfoProvider freshInfoProvider;

        public ScheduleService(ISchedulesStorage storage, IGroupsMonitor groupsMonitor, IScheduleInfoProvider freshInfoProvider)
        {
            this.storage = storage;
            this.freshInfoProvider = freshInfoProvider;
            this.scheduleConstructor = new ScheduleConstructor(new DefaultSchElemsFactory());
            this.GroupsMonitor = groupsMonitor;
        }

        
        public event EventHandler UpdatedEvent;

        public Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, ScheduleRequiredFor period)
        {
            if (period != ScheduleRequiredFor.Week)
                return GetScheduleForAsync(groups, DayOfWeekFromPeriod(period));
            else
                return GetWeekScheduleForAsync(groups);
        }

        public async Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day)
        {
            var preparedSchedules = new BlockingCollection<ISchedule>(new ConcurrentQueue<ISchedule>());
            //collect tasks
            var adding = Task.Run(() =>
            {
                foreach (var schedule in storage.GetSchedules(ValidateGroups(groups), day))
                {
                    preparedSchedules.Add(schedule);
                }
            });
            //start consuming
            var result = scheduleConstructor.ConstructFromMany(preparedSchedules.GetConsumingEnumerable());
            await Task.WhenAll(adding, result);
            //sync without waiting
            return await result;
        }


        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, ScheduleRequiredFor period)
        {
            if (period != ScheduleRequiredFor.Week)
                return GetScheduleForAsync(new[] {group}, DayOfWeekFromPeriod(period));
            else
                return GetWeekScheduleForAsync(new[] {group});
        }

        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, DayOfWeek day)
        {
            return GetScheduleForAsync(new[] { group }, day);
        }

        private IEnumerable<IScheduleGroup> ValidateGroups(IEnumerable<IScheduleGroup> groups)
        {
            return GroupsMonitor.RemoveInvalidGroupsFrom(groups);
        }

        public Task<IEnumerable<IScheduleGroup>> GetAvailableGroupsAsync()
        {
            return Task.Run(() => GroupsMonitor.AvailableGroups);
        }
        private async Task<ISchedule> GetWeekScheduleForAsync(IEnumerable<IScheduleGroup>  groups)
        {
            var preparedSchedules = new BlockingCollection<ISchedule>(new ConcurrentQueue<ISchedule>());
            //collect tasks
            var tasks = new List<Task>();
            //
            var validated = ValidateGroups(groups);
            for (int i = 1; i <= 6; i++)
            {
                tasks.Add(Task.Factory.StartNew((index) =>
                {
                    foreach (var schedule in storage.GetSchedules(validated, (DayOfWeek)index))
                    {
                        preparedSchedules.Add(schedule);
                    }
                }, i));
            }
            //start consuming
            var result = scheduleConstructor.ConstructFromMany(preparedSchedules.GetConsumingEnumerable());
            tasks.Add(result);
            await Task.WhenAll(tasks);
            //sync without waiting
            return await result;
        }

        private DayOfWeek DayOfWeekFromPeriod(ScheduleRequiredFor period)
        {
            switch (period)
            {
                case ScheduleRequiredFor.Today:
                    return DateTime.Now.DayOfWeek;
                case ScheduleRequiredFor.Tomorrow:
                    return DateTime.UtcNow.AddDays(1).ToLocalTime().DayOfWeek;
                
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}