using System.Collections.Generic;
using ScheduleServices.Core.Models.Interfaces;

namespace TablesRulesCore
{
    public interface ICellRule
    {
        int EstimateApplicability(string cellText);
        IEnumerable<IScheduleElem> SerializeElems(string cellText, TableContext context);
    }
}