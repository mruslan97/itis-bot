using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Modules.BranchMerging.Strategies;

namespace ScheduleServices.Core.Modules.BranchMerging
{
    public class SchElemsMerger
    {
        private readonly ISchElemsFactory defaultFactory;

        public SchElemsMerger(ISchElemsFactory defaultFactory)
        {
            this.defaultFactory = defaultFactory;
        }

        public void Merge(ref IScheduleElem sourceNode, ref IScheduleElem targetNode)
        {
            if (!TryMerge(ref sourceNode, ref targetNode))
            {
                IScheduleElem commonParent = AddToCommonParent(sourceNode, targetNode);
                targetNode = commonParent;
            }
        }

        

        public bool TryMerge(ref IScheduleElem sourceNode, ref IScheduleElem targetNode)
        {
            if (sourceNode.Level == targetNode.Level)
            {
                return GetStrategy(targetNode.Level)
                    .TryRootToRootMerge(sourceNode, targetNode, TryMerge);
            }
            else
            {
                if (Math.Abs(sourceNode.Level - targetNode.Level) > 2 &&
                    targetNode.Level != ScheduleElemLevel.Undefined)
                    throw new ScheduleConstructorException("wrong schedule trees");
                //sourceNode.Level == Day and targetNode.Level == Week case f.e.
                if (sourceNode.Level > targetNode.Level)
                { 
                    //todo: if target is undefinded?
                    GetStrategy(targetNode.Level).ChildToParent(ref sourceNode, ref targetNode, TryMerge);
                    return true;
                }
                else
                {
                    GetStrategy(targetNode.Level).ParentToChild(ref sourceNode, ref targetNode, TryMerge);
                    return true;
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

        private IScheduleElem AddToCommonParent(IScheduleElem sourceNode, IScheduleElem targetNode)
        {
            var res = CreateDefault(targetNode.Level - 1);
            res.Elems.Add(sourceNode);
            res.Elems.Add(targetNode);
            return res;
        }

        private IScheduleElem CreateDefault(ScheduleElemLevel level)
        {
            switch (level)
            {
                case ScheduleElemLevel.Week:
                    return defaultFactory.GetWeek();
                case ScheduleElemLevel.Day:
                    return defaultFactory.GetDay();
                case ScheduleElemLevel.Undefined:
                    return defaultFactory.GetUndefined();
                default:
                    throw new ArgumentOutOfRangeException($"Cannot create default for {level}");
            }
        }
    }
}