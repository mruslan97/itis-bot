using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.BranchMerging;
using ScheduleServices.Core.Modules.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using ScheduleServices.Core.Providers.Storage;

namespace ScheduleServices.Core.Config
{
    public static class Configure
    {
        public static void AddDefaultScheduleServiceCore(this IServiceCollection services, IEnumerable<IScheduleGroup> groupsList, IEnumerable<ICompatibleGroupsRule> rules)
        {
            services.AddSingleton<IScheduleService, ScheduleService>();
            services.AddSingleton<ISchElemsFactory, DefaultSchElemsFactory>();
            services.AddTransient<IGroupsMonitor, GroupsMonitor>(provider => new GroupsMonitor(groupsList, rules));
            services.AddTransient<ISchedulesStorage, SchedulesInMemoryDbStorage>();
            services.AddTransient<SchElemsMerger>();
        }
    }
}
