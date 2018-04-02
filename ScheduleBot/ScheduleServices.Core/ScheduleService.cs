using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using ScheduleServices.Core.Extensions;
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
        public event EventHandler UpdatedEvent;

        public ScheduleService(ISchedulesStorage storage, IGroupsMonitor groupsMonitor,
            IScheduleInfoProvider freshInfoProvider)
        {
            this.storage = storage;
            this.freshInfoProvider = freshInfoProvider;
            this.scheduleConstructor = new ScheduleConstructor(new DefaultSchElemsFactory());
            this.GroupsMonitor = groupsMonitor;
            
            try
            {
                for (int i = 1; i <= 6; i++)
                    UpdateSchedulesAsync(groupsMonitor.AvailableGroups, (DayOfWeek)i).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("UPDATE INTERRUPTED");
            }
            
            Console.WriteLine("UPDATED");
        }

        #region overloads

        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, ScheduleRequiredFor period)
        {
            if (period != ScheduleRequiredFor.Week)
                return GetScheduleForAsync(new[] {group}, DayOfWeekFromPeriod(period));
            else
                return GetWeekScheduleForAsync(new[] {group});
        }

        public Task<ISchedule> GetScheduleForAsync(IScheduleGroup @group, DayOfWeek day)
        {
            return GetScheduleForAsync(new[] {group}, day);
        }

        public async Task UpdateSchedulesAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day)
        {
            var validated = ValidateGroups(groups);
            var tfresh = Task.Run(() => freshInfoProvider.GetSchedules(validated, day));
            var tstored = Task.Run(() => storage.GetSchedules(validated, day));
            await Task.WhenAll(tfresh, tstored);
            //sync, already completed
            var fresh = await tfresh;
            var stored = await tstored;
            var goodFresh = fresh.Where(schedule =>
            {
                var checkresult = schedule.ScheduleRoot.CheckElemIsCorrect();
                if (!checkresult.Successed)
                    foreach (var error in checkresult.ErrorsList)
                        Console.WriteLine("[" + DateTime.Now + "] + WHILE CHECK FRESH SCHEDULE ERROR FOUND:" + error);
                //return checkresult.Successed;
                return true;
            });
            //left outer join: all from fresh and matching from storage or null
            List<Task> updateTasks = new List<Task>();
            foreach (var pair in goodFresh.LeftJoin(stored, sch => sch.ScheduleGroups.FirstOrDefault(),
                sch => sch.ScheduleGroups.FirstOrDefault(),
                (fromFresh, fromStorage) => new {Fresh = fromFresh, Stored = fromStorage}))
            {
                if (pair.Stored != null)
                    updateTasks.Add(CompareAndAddIfNotEqual(pair.Fresh, pair.Stored));
                else
                    updateTasks.Add(storage.UpdateScheduleAsync(pair.Fresh.ScheduleGroups.FirstOrDefault(),
                        pair.Fresh.ScheduleRoot));
            }

            await Task.WhenAll(updateTasks);

            Task CompareAndAddIfNotEqual(ISchedule freshSchedule, ISchedule storedSchedule)
            {
                return Task.Run(() =>
                {
                    if (!freshSchedule.ScheduleRoot.Equals(storedSchedule.ScheduleRoot))
                        return storage.UpdateScheduleAsync(freshSchedule.ScheduleGroups.FirstOrDefault(),
                            freshSchedule.ScheduleRoot);
                    return Task.CompletedTask;
                });
            }
        }

        public Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, ScheduleRequiredFor period)
        {
            if (period != ScheduleRequiredFor.Week)
                return GetScheduleForAsync(groups, DayOfWeekFromPeriod(period));
            else
                return GetWeekScheduleForAsync(groups);
        }

        #endregion


        public async Task<ISchedule> GetScheduleForAsync(IEnumerable<IScheduleGroup> groups, DayOfWeek day)
        {
            try
            {
                var preparedSchedules = new BlockingCollection<ISchedule>(new ConcurrentQueue<ISchedule>());
                //collect tasks
                var adding = Task.Run(() =>
                {
                    foreach (var schedule in storage.GetSchedules(ValidateGroups(groups), day))
                    {
                        preparedSchedules.Add(schedule);
                    }
                }).ContinueWith((t) => preparedSchedules.CompleteAdding());
                //start consuming
                var result = scheduleConstructor.ConstructFromMany(preparedSchedules.GetConsumingEnumerable());
                await Task.WhenAll(adding, result);
                //sync without waiting
                var res = (await result).OrderScheduleRootAndChildren();
                if (res.ScheduleRoot.Level == ScheduleElemLevel.Undefined)
                    Console.Out.WriteLine("[" + DateTime.Now + $"] + UNDEFINED DAY {day.ToString()} SCHEDULE FOUND, groups:" + JsonConvert.SerializeObject(groups));
                return res;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                throw;
            }
           
        }


        private IEnumerable<IScheduleGroup> ValidateGroups(IEnumerable<IScheduleGroup> groups)
        {
            return GroupsMonitor.RemoveInvalidGroupsFrom(groups);
        }


        private async Task<ISchedule> GetWeekScheduleForAsync(IEnumerable<IScheduleGroup> groups)
        {
            try
            {
                var preparedSchedules = new BlockingCollection<ISchedule>(new ConcurrentQueue<ISchedule>());
                //collect tasks
                var tasks = new List<Task>();
                //start consuming
                var result = scheduleConstructor.ConstructFromMany(preparedSchedules.GetConsumingEnumerable());

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


                await Task.WhenAll(tasks).ContinueWith((t) => preparedSchedules.CompleteAdding());
                var res = (await result).OrderScheduleRootAndChildren();
                if (res.ScheduleRoot.Level == ScheduleElemLevel.Undefined)
                    Console.Out.WriteLine("[" + DateTime.Now + "] + UNDEFINED WEEK SCHEDULE FOUND, groups:" + JsonConvert.SerializeObject(groups));
                return res;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                throw;
            }
            
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