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
        public class LevenshteinBasedRule : ICellRule
        {
            private readonly int limitFailLengthPercentage;
            private readonly int maxScore;

            private readonly Dictionary<IScheduleGroup, (int Distance, string Token)> Result =
                new Dictionary<IScheduleGroup, (int Distance, string Token)>();

            private double distanceNormalizer;

            public LevenshteinBasedRule(int limitFailLengthPercentage, int maxScore, double distanceNormalizer)
            {
                this.limitFailLengthPercentage = limitFailLengthPercentage;
                this.maxScore = maxScore;
                this.distanceNormalizer = distanceNormalizer;
            }

            public int EstimateApplicability(string cellText, TableContext context,
                IEnumerable<IScheduleGroup> availableGroups)
            {
                Result.Clear();
                var tokens = cellText.Split(new[] {':', ','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var availableGroup
                    in availableGroups.Where(g =>
                        g.GType != ScheduleGroupType.Academic && g.GType != ScheduleGroupType.Eng &&
                        g.Name.EndsWith(ParsingTools.ExtractStreamNumber(context)) &&
                        g.Name.Contains(ParsingTools.ExtractCourseLabel(context))))
                {
                    var clearGroupName = ParsingTools.ClearToken(availableGroup.Name,
                        LessonParts.Notation | LessonParts.HelpSymbols | LessonParts.TeacherName);
                    var bestDistAndToken = tokens.Aggregate((Distance: Int32.MaxValue, BestToken: (string) null),
                        (bestEntry, current) =>
                        {
                            var curDistance = ParsingTools.LevenshteinDistance.Compute(ParsingTools.ClearToken(current,
                                LessonParts.Notation | LessonParts.TeacherName | LessonParts.Evenness |
                                LessonParts.Room), clearGroupName);
                            if (curDistance < bestEntry.Distance)
                            {
                                bestEntry.Distance = curDistance;
                                bestEntry.BestToken = current;
                            }

                            return bestEntry;
                        });
                    if (bestDistAndToken.BestToken != null &&
                        (bestDistAndToken.Distance / (double) bestDistAndToken.BestToken.Length * 100) <
                        limitFailLengthPercentage)
                    {
                        if (Result.TryGetValue(availableGroup, out var existingEntry))
                        {
                            if (existingEntry.Distance > bestDistAndToken.Distance)
                                Result[availableGroup] = bestDistAndToken;
                        }
                        else
                        {
                            Result[availableGroup] = bestDistAndToken;
                        }
                    }
                }

                if (Result.Any())
                    return (int) Result.Values.Min(value => maxScore - value.Distance * distanceNormalizer);
                return Int32.MinValue;
            }

            public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText,
                TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                return Result.Select(pair =>
                {
                    var lesson = new Lesson()
                    {
                        BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                            ParsingTools.LessonHoursLabelFormat,
                            CultureInfo.InvariantCulture),
                        Duration = TimeSpan.FromHours(1.5),
                        Level = ScheduleElemLevel.Lesson,
                        IsOnEvenWeek = ParsingTools.ExtractEvenness(pair.Value.Token),
                        Place = ParsingTools.ExtractRoom(pair.Value.Token),
                        Teacher = ParsingTools.ExtractTeacherName(pair.Key.Name)
                    };
                    lesson.Notation = ParsingTools.ExtractNotation(pair.Value.Token);
                    lesson.Discipline = ParsingTools.ClearToken(pair.Key.Name,
                        LessonParts.TeacherName | LessonParts.HelpSymbols | LessonParts.Stream | LessonParts.Course);
                    return new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, pair.Key);
                }).ToList();
            }
        }
    }
}