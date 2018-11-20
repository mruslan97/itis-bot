using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace TableRules.Core
{
    public interface ICellRule
    {
        int EstimateApplicability(string cellText, IEnumerable<IScheduleGroup> availableGroups);

        IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> 
            SerializeElems(string cellText, TableContext context, IEnumerable<IScheduleGroup> availableGroups);
    }
}