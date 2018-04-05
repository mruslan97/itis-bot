using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;

namespace ScheduleServices.Core.Providers.Storage
{
    public class SchedulesInMemoryDbStorage : ISchedulesStorage
    {
        private readonly ISchElemsFactory factory;

        private ConcurrentDictionary<string, ICollection<IScheduleElem>> storage =
            new ConcurrentDictionary<string, ICollection<IScheduleElem>>();

        public SchedulesInMemoryDbStorage(ISchElemsFactory factory)
        {
            this.factory = factory;
        }


        public IEnumerable<ISchedule> GetSchedules(IEnumerable<IScheduleGroup> availableGroups, DayOfWeek day)
        {
            return GetSchedules(availableGroups, days => days.Where(d => d.DayOfWeek == day));
        }

        private IEnumerable<ISchedule> GetSchedules(IEnumerable<IScheduleGroup> availableGroups, Func<IEnumerable<Day>,IEnumerable<Day>> modificator)
        {
            string key;
            ISchedule schedule;
            foreach (var availableGroup in availableGroups)
            {
                key = KeyFromGroup(availableGroup);
                if (storage.TryGetValue(key, out ICollection<IScheduleElem> days))
                {
                    foreach (var resDay in modificator(days.OfType<Day>()))
                    {
                        schedule = factory.GetSchedule();

                        schedule.ScheduleRoot = (IScheduleElem)resDay.Clone();
                        schedule.ScheduleGroups.Add(availableGroup);
                        yield return schedule;
                    }
                }
            }
        }

        private string KeyFromGroup(IScheduleGroup group)
        {
            return group.GType.ToString() + "^" + group.Name;
        }

        public Task<bool> UpdateScheduleAsync(IScheduleGroup targetGroup, IScheduleElem scheduleRoot)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    string key = KeyFromGroup(targetGroup);
                    storage.AddOrUpdate(key,
                        //if new key
                        keyS =>
                        {
                            switch (scheduleRoot.Level)
                            {
                                case ScheduleElemLevel.Week:
                                    return ((IScheduleElem) scheduleRoot.Clone()).Elems;
                                case ScheduleElemLevel.Day:
                                    return new List<IScheduleElem>() {scheduleRoot};
                                default:
                                    throw new ArgumentOutOfRangeException(
                                        $"Db cannot add root with level {scheduleRoot.Level.ToString()}");
                            }
                        },
                        //if already exists
                        (keyS, days) =>
                        {
                            switch (scheduleRoot.Level)
                            {
                                case ScheduleElemLevel.Week:
                                    return ((IScheduleElem) scheduleRoot.Clone()).Elems;
                                case ScheduleElemLevel.Day:
                                    var casted = (Day) scheduleRoot;
                                    var toUpd = days.OfType<Day>().FirstOrDefault(d => d.DayOfWeek == casted.DayOfWeek);
                                    if (toUpd != null)
                                        days.Remove(toUpd);
                                    days.Add((IScheduleElem) casted.Clone());
                                    return days;
                                default:
                                    throw new ArgumentOutOfRangeException(
                                        $"Db cannot update root with level {scheduleRoot.Level.ToString()}");
                            }
                        });
                    return true;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    //todo: logger
                    return false;
                }
            });
        }

        public IEnumerable<ISchedule> GetAll(IEnumerable<IScheduleGroup> availableGroups)
        {
            return GetSchedules(availableGroups, d => d);
        }

        public async Task RunVisitor(IDynamicElemVisitor visitor)
        {
            var queue = new BlockingCollection<IScheduleElem>(new ConcurrentQueue<IScheduleElem>());
            var consumer = Task.Run(() =>
            {
                foreach (var scheduleElem in queue.GetConsumingEnumerable())
                {
                    visitor.VisitElem(scheduleElem);
                }
            });
            foreach (var day in storage.Values.SelectMany(week => week))
            {
                queue.Add(day);
            }
            queue.CompleteAdding();
            await consumer;
        }
    }


    /* public class SingleGroupSchedule : Schedule
     {
         public IScheduleGroup Group { get; set; }
 
         //hide! do not use as Schedule or ISchedule
         [BsonIgnore]
         public new ICollection<IScheduleGroup> ScheduleGroups
         {
             get => new List<IScheduleGroup> {Group};
             set => Group = value.FirstOrDefault();
         }
 
         public static SingleGroupSchedule FromSchedule(ISchedule schedule)
         {
             return new SingleGroupSchedule()
             {
                 ScheduleGroups = schedule.ScheduleGroups,
                 ScheduleRoot = schedule.ScheduleRoot
             };
         }
 
         public static ISchedule ToSchedule(SingleGroupSchedule singleGroupSchedule)
         {
             return new Schedule()
             {
                 ScheduleGroups = singleGroupSchedule.ScheduleGroups,
                 ScheduleRoot = singleGroupSchedule.ScheduleRoot
             };
         }
     }*/
}