using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleServices.Core.Modules.BranchMerging.Strategies
{
    public class WeekMergeStrategy : MergeStrategy
    {
        public WeekMergeStrategy(SchElemsMerger schElemsMerger) : base(schElemsMerger)
        {
        }

        public override void RootToRootMerge(IScheduleElem source, IScheduleElem target,
            Action<IScheduleElem, IScheduleElem> recurrentStep)
        {
            if (source == null || source.Elems == null || !source.Elems.Any())
            {
                return;
            }

            var sourceWeek = (Week)source;
            var targetWeek = (Week)target;
            if (targetWeek.Elems == null || !targetWeek.Elems.Any())
            {
                targetWeek.Elems = sourceWeek.Elems;
                return;
            }


            targetWeek.Elems = targetWeek.Elems.Concat(sourceWeek.Elems).GroupBy(elem => ((Day)elem).DayOfWeek,
                (dayOfWeek, elems) =>
                {
                    var count = elems.Count();
                    if (count > 2)
                        throw new ScheduleConstructorException("Several same day's of week");
                    if (count == 1)
                        return elems.First();
                    var first = elems.First();
                    //next step merge
                    recurrentStep.Invoke(elems.ElementAt(1), first);
                    return first;
                }).ToList();
        }

        public override void ParentToChild(ref IScheduleElem sourceParent, ref IScheduleElem targetChild,
            Action<IScheduleElem, IScheduleElem> recurrentStep)
        {
            if (sourceParent == null || sourceParent.Elems == null)
            {
                //todo: init elems by list 
                throw new ArgumentOutOfRangeException(
                    "source parent cannot be null and should have initialized collection");
            }

            //var sourceWeek = (Week) sourceParent;
            var targetDay = (Day)targetChild;
            var days = sourceParent.Elems.Cast<Day>().ToList();
            var commonDays = days.Count(diw => diw.DayOfWeek == targetDay.DayOfWeek);
            if (commonDays > 1)
                throw new ScheduleConstructorException("Several same day's of week");
            if (commonDays == 1)
            {
                var common = days.FirstOrDefault(diw => diw.DayOfWeek == targetDay.DayOfWeek);
                //swap target and source to save all into week branch
                SchElemsMerger.GetStrategy(ScheduleElemLevel.Day)
                    .RootToRootMerge(targetDay, common, recurrentStep);
            }
            else
            {
                sourceParent.Elems.Add(targetDay);
            }

            targetChild = sourceParent;
        }

        public override void ChildToParent(ref IScheduleElem sourceChild, ref IScheduleElem targetParent,
            Action<IScheduleElem, IScheduleElem> recurrentStep)
        {
            if (sourceChild == null)
            {
                return;
            }

            if (targetParent == null)
                throw new ArgumentNullException("target branch is null");
            if (targetParent.Elems == null)
                targetParent.Elems = new List<IScheduleElem>();
            if (!targetParent.Elems.Any())
                targetParent.Elems.Add(sourceChild);
            else
            {
                var sourceDay = (Day)sourceChild;
                var days = targetParent.Elems.Cast<Day>().ToList();
                var commonDays = days.Count(diw => diw.DayOfWeek == sourceDay.DayOfWeek);
                if (commonDays > 1)
                    throw new ScheduleConstructorException("Several same day's of week");
                if (commonDays == 1)
                {
                    var common = days.FirstOrDefault(diw => diw.DayOfWeek == sourceDay.DayOfWeek);
                    //swap target and source to save all into week branch
                    SchElemsMerger.GetStrategy(ScheduleElemLevel.Day)
                        .RootToRootMerge(sourceDay, common, recurrentStep);
                }
                else
                {
                    targetParent.Elems.Add(sourceDay);
                }
            }
        }
    }
}
