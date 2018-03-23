﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using System.Collections.Concurrent;

namespace ScheduleServices.Core
{
    public class ScheduleService : IScheduleServise
    {
        private readonly ISchedulesStorage storage;
        private readonly ScheduleConstructor scheduleConstructor;

        public ScheduleService(ISchedulesStorage storage)
        {
            this.storage = storage;
            this.scheduleConstructor = new ScheduleConstructor();
        }

        public event EventHandler UpdatedEvent;

        public Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, ScheduleRequiredFor period)
        {
            throw new NotImplementedException();
        }

        public async Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day)
        {
            var preparedSchedules = new BlockingCollection<ISchedule>(new ConcurrentQueue<ISchedule>());
            var tasks = new List<Task<ISchedule>>();
            //collect tasks
            foreach (var requiredGroup in ValidateGroups(groups))
            {
                tasks.Add(storage.GetScheduleAsync(requiredGroup, day));
            }

            //start consuming
            var result = scheduleConstructor.ConstructFromMany(preparedSchedules.GetConsumingEnumerable());
            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks);
                tasks.Remove(completed);
                //sync because task is completed
                preparedSchedules.Add(await completed);
            }

            return await result;
        }


        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, ScheduleRequiredFor period)
        {
            throw new NotImplementedException();
        }

        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, DayOfWeek day)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<IScheduleGroup> ValidateGroups(IEnumerable<IScheduleGroup> groups)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IScheduleGroup>> GetAvailibleGroupsAsync()
        {
            throw new NotImplementedException();
        }
    }
}