using System;
using System.Collections.Generic;
using System.Linq;
using MagicParser.Configuration;
using MagicParser.Models;
using MagicParser.Services;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Providers.Interfaces;

namespace MagicParser.Impls
{
    public class ScheduleInfoProvider : IScheduleInfoProvider
    {
        private readonly GoogleApiConfig config;

        public ScheduleInfoProvider(GoogleApiConfig config)
        {
            this.config = config;
        }
        public IEnumerable<ISchedule> GetSchedules(IEnumerable<IScheduleGroup> availableGroups, DayOfWeek day)
        {
            try
            {
                var groups = availableGroups.Where(g => g.GType == ScheduleGroupType.Academic).ToList();
                var schedules = new List<Schedule>();
                var subjects = GetAllGroups(day);
                foreach (var group in groups)
                {
                    var result = new Schedule
                    {
                        ScheduleGroups = new List<IScheduleGroup> {group},
                        ScheduleRoot = new Day
                        {
                            Level = ScheduleElemLevel.Day,
                            DayOfWeek = day,
                            Elems = ConvertSubjects(subjects.Where(s =>
                                s.Group == group.Name && s.Type != ScheduleGroupType.Eng))
                        }
                    };

                    if (result.ScheduleRoot.Elems.Count != 0)
                        schedules.Add(result);
                }

                var englishGroups = availableGroups.Where(g => g.GType == ScheduleGroupType.Eng).ToList();
                var englishSubjects = subjects.Where(s => s.Type == ScheduleGroupType.Eng);
                foreach (var engGroup in englishGroups)
                {
                    var result = new Schedule
                    {
                        ScheduleGroups = new List<IScheduleGroup> {engGroup},
                        ScheduleRoot = new Day
                        {
                            Level = ScheduleElemLevel.Day,
                            DayOfWeek = day,
                            Elems = ConvertSubjects(englishSubjects.Where(s =>
                                s.Teacher.Contains(engGroup.Name.Substring(0, engGroup.Name.IndexOf(' ')))
                                && s.Type == ScheduleGroupType.Eng
                                && s.Flow.ToString() == engGroup.Name[engGroup.Name.Length - 1].ToString()
                                && s.Course.ToString() == engGroup.Name.Substring(engGroup.Name.IndexOf('_') + 1, 1)))
                        }
                    };

                    if (result.ScheduleRoot.Elems.Count != 0)
                        schedules.Add(result);
                }

                var scienticGroups = availableGroups.Where(g => g.GType == ScheduleGroupType.PickedScientic).ToList();
                var scienticSubjects = subjects.Where(s => s.Type == ScheduleGroupType.PickedScientic);
                foreach (var scienticGroup in scienticGroups)
                {
                    var result = new Schedule
                    {
                        ScheduleGroups = new List<IScheduleGroup> {scienticGroup},
                        ScheduleRoot = new Day
                        {
                            Level = ScheduleElemLevel.Day,
                            DayOfWeek = day,
                            Elems = ConvertSubjects(scienticSubjects.Where(s =>
                                scienticGroup.Name.Contains(s.SubjectName)))
                        }
                    };

                    if (result.ScheduleRoot.Elems.Count != 0)
                        schedules.Add(result);
                }

                var techGroups = availableGroups.Where(g => g.GType == ScheduleGroupType.PickedTech).ToList();
                var techSubjects = subjects.Where(s => s.Type == ScheduleGroupType.PickedTech);
                foreach (var techGroup in techGroups)
                {
                    var result = new Schedule
                    {
                        ScheduleGroups = new List<IScheduleGroup> {techGroup},
                        ScheduleRoot = new Day
                        {
                            Level = ScheduleElemLevel.Day,
                            DayOfWeek = day,
                            Elems = ConvertSubjects(techSubjects.Where(s =>
                                techGroup.Name.Contains(s.SubjectName)
                                && s.Teacher == techGroup.Name.Split('_')[1]
                                && s.Course.ToString() == techGroup.Name.Split('_')[2].Substring(0, 1)))
                        }
                    };

                    if (result.ScheduleRoot.Elems.Count != 0)
                        schedules.Add(result);
                }


                return schedules;
            }
            catch (Exception e)
            {
                throw new ScheduleConstructorException("An exception occured while constructing schedule.", e);
            }
        }

        private IEnumerable<ParsedSubject> GetAllGroups(DayOfWeek day)
        {
            var googleApi = new GoogleApiService(config.ApplicationName, config.SpreadsheetId);
            var smartSorter = new SmartSortService();
            var result = new List<ParsedSubject>();
            for (var i = 1; i <= 4; i++) result.AddRange(smartSorter.SortContent(googleApi.SendRequest(i, (int) day)));

            return result;
        }

        private ICollection<IScheduleElem> ConvertSubjects(IEnumerable<ParsedSubject> oldFormatSubjects)
        {
            ICollection<IScheduleElem> result = new List<IScheduleElem>();
            foreach (var subject in oldFormatSubjects)
                if (subject.SubjectName.Any(char.IsLetter))
                    result.Add(new Lesson
                    {
                        Discipline = subject.SubjectName,
                        Teacher = subject.Teacher,
                        Place = subject.Cabinet,
                        BeginTime = TimeSpan.Parse(subject.Time.Replace('.', ':').Substring(0, 5)),
                        Duration = TimeSpan.FromMinutes(90),
                        Notation = subject.Notation,
                        IsOnEvenWeek = subject.IsOnEvenWeek,
                        Elems = null
                    });

            return result;
        }
    }
}