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
                //todo: check try-catch
                var result = elemsFactory.GetSchedule();
                Dictionary<ScheduleGroupType, string> uniqGroups = new Dictionary<ScheduleGroupType, string>();


                foreach (var schedule in schedules)
                {
                    if (!schedule.ScheduleGroups.Any(group => HasConflictsWithMemory(group, uniqGroups)))
                    {
                        branchMerger.Merge(schedule.ScheduleRoot, result.ScheduleRoot);
                    }
                    else
                    {
                        throw new ScheduleConstructorException("Incompatible groups in schedule parts found");
                    }
                }

                return result;
            });
        }


        private bool HasConflictsWithMemory(IScheduleGroup group, Dictionary<ScheduleGroupType, string> memory)
        {
            if (group.Name != null)
            {
                if (!memory.ContainsKey(group.GType))
                {
                    memory.Add(group.GType, group.Name);
                    return false;
                }

                return !group.Name.Equals(memory[group.GType], StringComparison.OrdinalIgnoreCase);
            }

            throw new ArgumentNullException("Name for group cannot be null");
        }
    }
}