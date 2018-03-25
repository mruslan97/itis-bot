using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.BranchMerging.Strategies;

namespace ScheduleServices.Core.Modules.BranchMerging
{
    
    public class SchElemsMerger
    {
        public void Merge(ref IScheduleElem sourceNode,ref IScheduleElem targetNode)
        {
            if (sourceNode.Level == targetNode.Level)
            {
                GetStrategy(targetNode.Level)
                    .RootToRootMerge(sourceNode, targetNode, Merge);
            }
            else
            {
                if (Math.Abs(sourceNode.Level - targetNode.Level) > 2 && targetNode.Level != ScheduleElemLevel.Undefined)
                    throw new ScheduleConstructorException("wrong schedule trees");
                //sourceNode.Level == Day and targetNode.Level == Week case f.e.
                if (sourceNode.Level > targetNode.Level)
                {
                    GetStrategy(targetNode.Level).ChildToParent(ref sourceNode, ref targetNode, Merge);
                }
                else
                {
                    GetStrategy(targetNode.Level).ParentToChild(ref sourceNode, ref targetNode, Merge);
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
