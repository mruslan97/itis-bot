using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core.Providers.Interfaces
{
    public interface IScheduleInfoProvider
    {
        //Task<ISchedule> GetScheduleAsync(IScheduleGroup group, DayOfWeek day);
        IEnumerable<ISchedule> GetSchedules(IEnumerable<IScheduleGroup> availableGroups, DayOfWeek day);
        /*
         * var groups = monitor.AvaliableGroups.Where(type == Academic);
         * var List<ISchedule> schedules = new List;
         * foreach (var group in groups)
         * {
         *     var result = new Schedule()
         *     {
         *           ScheduleGroups = new List<>() { group }
         *           ScheduleRoot = new Day()
         *           {
         *              Level = Day,
         *              DayOfWeek = day,
         *              Elems = GetLessonsFromDll
         *              (..
         *                new Lesson() { Level = Lesson, teacher = xxx, discipline = xxx, Elems = null } 
         *              .);
         *           }
         *      };
         *     schedules.Add(result); 
         * }
         *
         * foreach (var group in monitor.Avaliable.Where(type != Academic)
         * {
         *     if (type == eng)
         *     {
         *         group.Name == surname
         *     }
         * }
         * return schedules
         */
        
    }
}