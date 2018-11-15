using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace TablesRulesCore
{
    public interface ICellRule
    {
        int EstimateApplicability(string cellText);

        IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> 
            SerializeElems(string cellText, TableContext context, IEnumerable<IScheduleGroup> availableGroups);
    }
}