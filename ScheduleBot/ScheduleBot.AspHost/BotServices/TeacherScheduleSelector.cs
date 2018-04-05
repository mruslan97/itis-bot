using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleServices.Core;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleBot.AspHost.BotServices
{
    public class TeacherScheduleSelector : IScheduleSelector
    {
        public string TeacherName { get; set; }

        public IEnumerable<ISchedule> SelectSchedulesFromSource(IEnumerable<ISchedule> source)
        {
            if (String.IsNullOrEmpty(TeacherName))
                throw new InvalidOperationException();
            var firstSpace = TeacherName.IndexOf(' ', 1);
            TeacherName = (firstSpace > 0 ? TeacherName.Substring(0, firstSpace) : TeacherName).Trim().ToLowerInvariant();
            return source
                .Where(sch =>
                    sch.ScheduleRoot is Day day && day.Elems != null && day.Elems.OfType<Lesson>()
                        .Any(l => l.Teacher?.ToLowerInvariant().Contains(TeacherName) ?? false))
                .Select(sch =>
                {
                    sch.ScheduleRoot.Elems = sch.ScheduleRoot.Elems.OfType<Lesson>()
                        .Where(l => l.Teacher?.ToLowerInvariant().Contains(TeacherName) ?? false).Select(l => (IScheduleElem)
                            new LessonWithGroup()
                            {
                                RelatedGroup = sch.ScheduleGroups.FirstOrDefault(),
                                Teacher = l.Teacher,
                                BeginTime = l.BeginTime,
                                Discipline = l.Discipline,
                                Duration = l.Duration,
                                IsOnEvenWeek = l.IsOnEvenWeek,
                                Place = l.Place,
                                Notation = l.Notation,
                                Level = l.Level,
                                Elems = null
                            }).ToList();
                    sch.ScheduleGroups = null;
                    return sch;
                });
        }

        public class LessonWithGroup : Lesson
        {
            public IScheduleGroup RelatedGroup { get; set; }
        }
    }
}