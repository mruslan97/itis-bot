using System;
using System.Collections.Generic;
using System.Linq;
using GoogleSheetsSchedulesProvider.Configuration;
using GoogleSheetsSchedulesProvider.Services;
using Microsoft.Extensions.Logging;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Providers.Interfaces;
using TableRules.Core;

namespace GoogleSheetsSchedulesProvider
{
    public class ScheduleInfoProvider : IScheduleInfoProvider
    {
        private readonly GoogleApiConfig config;
        private readonly IEnumerable<ICellRule> cellRules;
        private readonly ILogger<ScheduleInfoProvider> logger;

        public ScheduleInfoProvider(GoogleApiConfig config, IEnumerable<ICellRule> cellRules, ILogger<ScheduleInfoProvider> logger)
        {
            this.config = config;
            this.cellRules = cellRules;
            this.logger = logger;
        }

        public IEnumerable<ISchedule> GetSchedules(IEnumerable<IScheduleGroup> availableGroups, DayOfWeek day)
        {
            var groupToSchedules = new Dictionary<IScheduleGroup, ISchedule>();
            var googleApi = new GoogleApiService(config.ApplicationName, config.SpreadsheetId);
            var cells = new List<(string CellValue, TableContext Context)>();
            for (var i = 1; i <= 4; i++)
                cells.AddRange(googleApi.SendRequest(i, (int) day));
            
            //prepare list
            if (!(availableGroups is IList<IScheduleGroup> availableGroupsList))
                availableGroupsList = availableGroups.ToList();

            foreach (var cell in cells)
            {
                try
                {
                    var bestRule = cellRules.Aggregate((BestRule: (ICellRule)null, BestRuleApplicability: int.MinValue),
                        (acc, currentRule) =>
                        {
                            var currentRuleApplicability = currentRule.EstimateApplicability(cell.CellValue, cell.Context, availableGroupsList);
                            if (currentRuleApplicability > acc.BestRuleApplicability)
                            {
                                acc.BestRuleApplicability = currentRuleApplicability;
                                acc.BestRule = currentRule;
                            }

                            return acc;
                        }).BestRule;
                    if (bestRule != null)
                    {

                        foreach (var schedulePair in bestRule.SerializeElems(cell.CellValue, cell.Context, availableGroupsList))
                        {
                            if (!groupToSchedules.ContainsKey(schedulePair.Group))
                            {
                                groupToSchedules.Add(schedulePair.Group,
                                    new Schedule()
                                    {
                                        ScheduleGroups = new List<IScheduleGroup>() { schedulePair.Group },
                                        ScheduleRoot = new Day
                                        {
                                            Level = ScheduleElemLevel.Day,
                                            DayOfWeek = day,
                                            Elems = new List<IScheduleElem>()
                                        }
                                    });
                            }
                            groupToSchedules[schedulePair.Group].ScheduleRoot.Elems.Add(schedulePair.ScheduleElem);
                        }
                    }
                    else
                    {
                        HandleFail(cell);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    HandleFail(cell);
                    throw;
                }
                
            }
            return groupToSchedules.Values;
        }

        private void HandleFail((string CellValue, TableContext Context) cell)
        {
            if (!string.IsNullOrWhiteSpace(cell.CellValue))
            {
                Console.WriteLine("Fail to parse:" + cell.CellValue);
                logger.LogWarning($"Rule not found for cell with value: " + cell.CellValue);
            }
        }
    }
}