using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.BranchMerging.Strategies;

namespace ScheduleServices.Core.Modules.BranchMerging
{
    public class SchElemsMerger
    {
        public void Merge(IScheduleElem scheduleRoot, IScheduleElem resultScheduleRoot)
        {
            if (scheduleRoot.Level == resultScheduleRoot.Level)
            {
                GetStrategy(resultScheduleRoot.Level)
                    .RootToRootMerge(scheduleRoot, resultScheduleRoot, Merge);
            }
            else
            {
                if (Math.Abs(scheduleRoot.Level - resultScheduleRoot.Level) > 2)
                    throw new ScheduleConstructorException("wrong schedule trees");
                //scheduleRoot.Level == Day and resultScheduleRoot.Level == Week case f.e.
                if (scheduleRoot.Level > resultScheduleRoot.Level)
                {
                    GetStrategy(resultScheduleRoot.Level).ParentToChild(ref scheduleRoot, ref resultScheduleRoot, Merge);
                }
                else
                {
                    GetStrategy(resultScheduleRoot.Level).ChildToParent(ref scheduleRoot, ref resultScheduleRoot, Merge);
                }
            }
        }
        public MergeStrategy GetStrategy(ScheduleElemLevel level)
        {
            switch (level)
            {
                case ScheduleElemLevel.Week:
                    return new WeekMergeStrategy(this);
                case ScheduleElemLevel.Day:
                    return new DayMergeStrategy(this);
                case ScheduleElemLevel.Undefined:
                    return new UndefinedMergeStrategy(this);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
