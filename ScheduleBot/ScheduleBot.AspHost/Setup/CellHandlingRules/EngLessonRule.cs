using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using TableRules.Core;

namespace ScheduleBot.AspHost.Setup
{
    public partial class CellHandlingRules
    {
        public class EngLessonRule : ICellRule
        {
            public int EstimateApplicability(string cellText, TableContext context,
                IEnumerable<IScheduleGroup> availableGroups) =>
                cellText.ToLower().Contains("англ") ? 100 : Int32.MinValue;

            public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText,
                TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                var streamNumber = ParsingTools.ExtractStreamNumber(context);
                var course = ParsingTools.ExtractCourseLabel(context);
                var engGroups = availableGroups
                    .Where(g => g.GType == ScheduleGroupType.Eng && g.Name.EndsWith(streamNumber) &&
                                g.Name.Contains(course))
                    .Select(g => (Group: g, Teacher: ParsingTools.ExtractTeacherName(g.Name))).ToList();
                return cellText.Substring(cellText.IndexOf("язык)") + 5)
                    .Split(",", StringSplitOptions.RemoveEmptyEntries).Select(elem => elem.Trim('.', ' '))
                    .Select(
                        teacherSet =>
                        {
                            var teacherFromSet = ParsingTools.ExtractTeacherName(teacherSet);
                            if (String.IsNullOrWhiteSpace(teacherFromSet))
                                teacherFromSet = ParsingTools.ClearToken(teacherSet,
                                    LessonParts.Evenness | LessonParts.Room | LessonParts.Notation);
                            var bestGroupEntry = engGroups.FirstOrDefault(groupEntry =>
                            {
                                return groupEntry.Teacher.Contains(teacherFromSet) ||
                                       teacherFromSet.Contains(groupEntry.Teacher) ||
                                       ParsingTools.LevenshteinDistance.Compute(groupEntry.Teacher, teacherFromSet) < 3;
                            });
                            var lesson = new Lesson()
                            {
                                BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                                    ParsingTools.LessonHoursLabelFormat,
                                    CultureInfo.InvariantCulture),
                                Discipline = "Английский язык",
                                Duration = TimeSpan.FromHours(1.5),
                                Level = ScheduleElemLevel.Lesson,
                                IsOnEvenWeek = teacherSet.Contains("ч.н") ? true :
                                    teacherSet.Contains("н.н") ? false : (bool?) null,
                                Place = ParsingTools.ExtractRoom(teacherSet),
                                Teacher = bestGroupEntry.Teacher
                            };
                            lesson.Notation = ParsingTools.ExtractNotation(teacherSet.Replace(lesson.Teacher, "")
                                .Replace(lesson.Place, ""));
                            return new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, bestGroupEntry.Group);
                        });
            }
        }
    }
}