using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.Updating
{
    public class UpdateJob : IJob
    {
        //UNCOMMENT ALL if you wanna to upd cource by cource;


        //private static int lastUpdatedCourse = 0;
        private static DayOfWeek lastUpdatedDayOfWeek = DateTime.Now.DayOfWeek - 1;
        private readonly IScheduleService scheduleService;
        //private static int lowestCourseGroupNum = -1;

        public UpdateJob(IScheduleService scheduleService)
        {
            this.scheduleService = scheduleService;
            /*if (lowestCourseGroupNum < 0)
                lowestCourseGroupNum = scheduleServise.GroupsMonitor.AvailableGroups
                    .Where(g => g.GType == ScheduleGroupType.Academic).Min(g =>
                        int.TryParse(g.Name.Substring(g.Name.IndexOf("11-") + 3, 1), out int val) ? val : int.MaxValue);*/
        }

        public async void Execute()
        {
            //lastUpdatedCourse %= 4;
            //var currentUpdateCourse = lastUpdatedCourse + 1;
            
            var currentUpdateDayOfWeek = lastUpdatedDayOfWeek +
                                         //lastUpdatedCourse != 0 ? 0 :
                                         1;
            if (currentUpdateDayOfWeek == DayOfWeek.Saturday || currentUpdateDayOfWeek == DayOfWeek.Sunday)
                lastUpdatedDayOfWeek = DayOfWeek.Sunday;
            else
                lastUpdatedDayOfWeek = currentUpdateDayOfWeek;

            try
            {
                await scheduleService.UpdateSchedulesAsync(
                    scheduleService
                        .GroupsMonitor
                        .AvailableGroups.ToList(),
                    currentUpdateDayOfWeek);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            
        }

        
    }
}