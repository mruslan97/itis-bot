using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules.BranchMerging.Strategies
{
    public abstract class MergeStrategy
    {
        protected SchElemsMerger SchElemsMerger { get; }


        protected MergeStrategy(SchElemsMerger schElemsMerger)
        {
            SchElemsMerger = schElemsMerger;
        }


        public abstract void RootToRootMerge(IScheduleElem source, IScheduleElem target,
            Action<IScheduleElem, IScheduleElem> recurrentStep);

        public abstract void ParentToChild(ref IScheduleElem sourceParent, ref IScheduleElem targetChild,
            Action<IScheduleElem, IScheduleElem> recurrentStep);

        public abstract void ChildToParent(ref IScheduleElem sourceChild, ref IScheduleElem targetParent,
            Action<IScheduleElem, IScheduleElem> recurrentStep);
    }
}
