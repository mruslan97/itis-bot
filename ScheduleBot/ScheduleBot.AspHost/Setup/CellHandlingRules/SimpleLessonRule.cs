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
        public class SimpleLessonRule : ICellRule
        {
            public int EstimateApplicability(string cellText, TableContext context,
                IEnumerable<IScheduleGroup> availableGroups)
            {
                return ParsingTools.TeacherNameRegex.Matches(cellText).Count == 1 ? 50 : Int32.MinValue;
            }

            public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText,
                TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                var lesson = new Lesson()
                {
                    BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                        ParsingTools.LessonHoursLabelFormat,
                        CultureInfo.InvariantCulture),
                    Duration = TimeSpan.FromHours(1.5),
                    Level = ScheduleElemLevel.Lesson,
                    IsOnEvenWeek = ParsingTools.ExtractEvenness(cellText),
                    Place = ParsingTools.ExtractRoom(cellText),
                    Teacher = ParsingTools.ExtractTeacherName(cellText)
                };
                lesson.Notation = ParsingTools.ExtractNotation(cellText);
                lesson.Discipline = ParsingTools.ClearToken(cellText,
                    LessonParts.TeacherName | LessonParts.Evenness | LessonParts.Notation | LessonParts.Room);
                return PrepareLectureOrSeminar(lesson, context,
                    availableGroups.Where(g =>
                        g.GType == ScheduleGroupType.Academic &&
                        g.Name.StartsWith(context.CurrentGroupLabel.Substring(0, 4))));
            }
        }
    }
}