using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicParser.Models;
using MagicParser.Services;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;

namespace ScheduleServices.Core.Providers.Impls
{
    public class ScheduleInfoProvider : IScheduleInfoProvider
    {
        public Task<IEnumerable<ISchedule>> GetScheduleAsync(IEnumerable<IScheduleGroup> availableGroups, DayOfWeek day)
        {
            return Task.Run<IEnumerable<ISchedule>>(() =>
            {
                try
                {
                    var groups = availableGroups.Where(g => g.GType == ScheduleGroupType.Academic).ToList();
                    var schedules = new List<Schedule>();
                    var subjects = GetAllGroups(day);
                    foreach (var group in groups)
                    {
                        var result = new Schedule()
                        {
                            ScheduleGroups = new List<IScheduleGroup> { group },
                            ScheduleRoot = new Day()
                            {
                                Level = ScheduleElemLevel.Day,
                                DayOfWeek = day,
                                Elems = ConvertSubjects(subjects.Where(s => s.Group == group.Name))
                            }

                        };
                        schedules.Add(result);
                    }

                    return schedules;
                }
                catch (Exception e)
                {
                    throw new ScheduleConstructorException("An exception occured while constructing schedule.", e);
                }

            });
        }

        private IEnumerable<ParsedSubject> GetAllGroups(DayOfWeek day)
        {
            var googleApi = new GoogleApiService();
            var smartSorter = new SmartSortService();
            var result = new List<ParsedSubject>();
            for (var i = 1; i <= 4; i++)
            {
                result.AddRange(smartSorter.SortContent(googleApi.SendRequest(i, (int)day)));
            }
            return result;
        }

        private ICollection<IScheduleElem> ConvertSubjects(IEnumerable<ParsedSubject> oldFormatSubjects)
        {
            ICollection<IScheduleElem> result = new List<IScheduleElem>();
            foreach (var subject in oldFormatSubjects)
            {
                result.Add(new Lesson
                {
                    Discipline = subject.SubjectName,
                    Teacher = subject.Teacher,
                    Place = subject.Cabinet,
                    BeginTime = TimeSpan.Parse(subject.Time.Replace('.',':').Substring(0,5)), 
                    Duration = TimeSpan.FromMinutes(90),
                    Notation = subject.Notation,
                    Elems = null
                });
            }

            return result;
        }
    }
}
