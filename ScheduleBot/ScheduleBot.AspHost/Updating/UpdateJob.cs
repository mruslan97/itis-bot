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
        private readonly IScheduleServise scheduleServise;
        //private static int lowestCourseGroupNum = -1;

        public UpdateJob(IScheduleServise scheduleServise)
        {
            this.scheduleServise = scheduleServise;
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
            if (currentUpdateDayOfWeek == DayOfWeek.Sunday)
                currentUpdateDayOfWeek++;

            //lastUpdatedCourse = currentUpdateCourse;
            lastUpdatedDayOfWeek = currentUpdateDayOfWeek;
            try
            {
                await scheduleServise.UpdateSchedulesAsync(
                    scheduleServise
                        .GroupsMonitor
                        .AvailableGroups
                        .Where(g =>
                            g.GType == ScheduleGroupType.Academic 
                            //&& g.Name.Contains("11-" + (lowestCourseGroupNum + currentUpdateCourse - 1))
                            ),
                    currentUpdateDayOfWeek);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        
    }
}