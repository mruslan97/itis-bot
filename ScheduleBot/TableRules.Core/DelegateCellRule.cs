using System;
using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace TableRules.Core
{
    public class DelegateCellRule : ICellRule

    {
        public Func<string, int> ApplicabilityEstimator { private get; set; }

        public Func<string, TableContext, IEnumerable<IScheduleGroup>, IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)>> 
            Serializer { private get; set; }

        public int EstimateApplicability(string cellText)
        {
            return ApplicabilityEstimator.Invoke(cellText);
        }

        public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText, TableContext context, IEnumerable<IScheduleGroup> availableGroups)
        {
            return Serializer.Invoke(cellText, context, availableGroups);
        }
       
    }
}
