using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleServices.Core.Modules.BranchMerging.Strategies
{
    public class UndefinedMergeStrategy : MergeStrategy
    {
        public UndefinedMergeStrategy(SchElemsMerger schElemsMerger)
        {
        }

        public override bool TryRootToRootMerge(IScheduleElem source, IScheduleElem target, ReccurentStep recurrentStep)
        {
            //no body
            return true;
        }

        public override void ParentToChild(ref IScheduleElem sourceParent, ref IScheduleElem targetChild, ReccurentStep recurrentStep)
        {
            throw new NotSupportedException();
        }

        public override void ChildToParent(ref IScheduleElem sourceChild, ref IScheduleElem targetParent, ReccurentStep recurrentStep)
        {
            targetParent = sourceChild;
        }
    }
}
