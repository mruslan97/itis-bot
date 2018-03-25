using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Modules.BranchMerging;

namespace ScheduleServices.Core.Modules
{
    public class ScheduleConstructor
    {
        private readonly ISchElemsFactory elemsFactory;
        private SchElemsMerger branchMerger;

        public ScheduleConstructor(ISchElemsFactory elemsFactory)
        {
            this.elemsFactory = elemsFactory;
            this.branchMerger = new SchElemsMerger();
        }

        public Task<ISchedule> ConstructFromMany(IEnumerable<ISchedule> schedules) 
        {
            return Task.Run<ISchedule>(() =>
            {
                try
                {
                    var result = elemsFactory.GetSchedule();
                    Dictionary<ScheduleGroupType, IScheduleGroup> uniqGroups = new Dictionary<ScheduleGroupType, IScheduleGroup>();


                    foreach (var schedule in schedules)
                    {
                        if (!schedule.ScheduleGroups.Any(group => HasConflictsWithMemory(group, uniqGroups)))
                        {
                            var incomingRoot = schedule.ScheduleRoot;
                            var currentResultRoot = result.ScheduleRoot;
                            branchMerger.Merge(ref incomingRoot, ref currentResultRoot);
                            result.ScheduleRoot = currentResultRoot;
                        }
                        else
                        {
                            throw new ScheduleConstructorException("Incompatible groups in schedule parts found");
                        }
                    }

                    foreach (var group in uniqGroups.Values)
                    {
                        result.ScheduleGroups.Add(group);
                    }
                    return result;
                }
                catch (Exception e)
                {
                    throw new ScheduleConstructorException("An exception occured while constructing schedule.", e);
                }
                
            });
        }


        private bool HasConflictsWithMemory(IScheduleGroup group, Dictionary<ScheduleGroupType, IScheduleGroup> memory)
        {
            if (group != null)
            {
                if (!memory.ContainsKey(group.GType))
                {
                    memory.Add(group.GType, group);
                    return false;
                }

                return !group.Equals(memory[group.GType]);
            }

            throw new ArgumentNullException("Name for group cannot be null");
        }
    }
}