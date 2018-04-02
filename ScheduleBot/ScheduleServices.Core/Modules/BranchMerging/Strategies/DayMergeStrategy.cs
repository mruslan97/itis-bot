using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleServices.Core.Modules.BranchMerging.Strategies
{
    public class DayMergeStrategy : MergeStrategy
    {
        public DayMergeStrategy(SchElemsMerger schElemsMerger) : base(schElemsMerger)
        {
        }

        public override bool TryRootToRootMerge(IScheduleElem source, IScheduleElem target,
            ReccurentStep recurrentStep)
        {
            if (source == null || source.Elems == null || !source.Elems.Any())
            {
                return true;
            }

            var sourceDay = (Day) source;
            var targetDay = (Day) target;

            if (sourceDay.DayOfWeek != targetDay.DayOfWeek)
            {
                return false;
            }

            if (targetDay.Elems == null || !targetDay.Elems.Any())
            {
                targetDay.Elems = sourceDay.Elems;
                return true;
            }


            targetDay.Elems = targetDay.Elems.Union(sourceDay.Elems).Cast<Lesson>().OrderBy(elem => elem.BeginTime)
                .ToList<IScheduleElem>();
            Lesson prev = null;
            foreach (var lesson in targetDay.Elems.Cast<Lesson>())
            {
                if (prev == null)
                {
                    prev = lesson;
                    continue;
                }

                if (lesson.BeginTime <= prev.BeginTime + prev.Duration &&
                    (lesson.IsOnEvenWeek == null || prev.IsOnEvenWeek == null ||
                     prev.IsOnEvenWeek == lesson.IsOnEvenWeek))
                    //throw new ScheduleConstructorException(
                //todo: throw exc!!!!!
                     Console.Out.WriteLine(String.Format(
                        "Day's lessons merge exception: time intersects: {0}, {1}", JsonConvert.SerializeObject(prev),
                        JsonConvert.SerializeObject(lesson)));
            }

            return true;
        }

        public override void ParentToChild(ref IScheduleElem sourceParent, ref IScheduleElem targetChild,
            ReccurentStep recurrentStep)
        {
            throw new NotImplementedException();
        }

        public override void ChildToParent(ref IScheduleElem sourceChild, ref IScheduleElem targetParent,
            ReccurentStep recurrentStep)
        {
            throw new NotImplementedException();
        }
    }
}