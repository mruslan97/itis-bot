﻿using System;
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

        public override void RootToRootMerge(IScheduleElem source, IScheduleElem target,
            Action<IScheduleElem, IScheduleElem> recurrentStep)
        {
            if (source == null || source.Elems == null || !source.Elems.Any())
            {
                return;
            }

            var sourceDay = (Day)source;
            var targetDay = (Day)target;

            if (sourceDay.DayOfWeek != targetDay.DayOfWeek)
            {
                //todo: throw exc?
                return;
            }

            if (targetDay.Elems == null || !targetDay.Elems.Any())
            {
                targetDay.Elems = sourceDay.Elems;
                return;
            }


            targetDay.Elems = targetDay.Elems.Concat(sourceDay.Elems).Cast<Lesson>().OrderBy(elem => elem.BeginTime)
                .ToList<IScheduleElem>();
            Lesson prev = null;
            foreach (var lesson in targetDay.Elems.Cast<Lesson>())
            {
                if (prev == null)
                {
                    prev = lesson;
                    continue;
                }

                if (lesson.BeginTime < prev.BeginTime + prev.Duration &&
                    (lesson.IsOnEvenWeek == null || prev.IsOnEvenWeek == null ||
                     prev.IsOnEvenWeek == lesson.IsOnEvenWeek))
                    throw new ScheduleConstructorException(String.Format(
                        "Day's lessons merge exception: time intersects: {0}, {1}", JsonConvert.SerializeObject(prev),
                        JsonConvert.SerializeObject(lesson)));
            }
        }

        public override void ParentToChild(ref IScheduleElem sourceParent, ref IScheduleElem targetChild,
            Action<IScheduleElem, IScheduleElem> recurrentStep)
        {
            throw new NotImplementedException();
        }

        public override void ChildToParent(ref IScheduleElem sourceChild, ref IScheduleElem targetParent,
            Action<IScheduleElem, IScheduleElem> recurrentStep)
        {
            throw new NotImplementedException();
        }
    }
}
