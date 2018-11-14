using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace TablesRulesCore
{
    public class DelegateCellRule : ICellRule

    {
        public Func<string, int> ApplicabilityEstimator { private get; set; }
        public Func<string, TableContext, IEnumerable<IScheduleElem>> Serializer { private get; set; }
        public int EstimateApplicability(string cellText)
        {
            return ApplicabilityEstimator.Invoke(cellText);
        }

        public IEnumerable<IScheduleElem> SerializeElems(string cellText, TableContext context)
        {
            return Serializer.Invoke(cellText, context);
        }
    }
}
