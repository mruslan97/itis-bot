using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using TableRules.Core;

namespace ScheduleBot.AspHost
{
    public class SchedulesSetup
    {
        private const string LessonHoursLabelFormat = "hh\\.mm";

        [Flags]
        private enum LessonParts : long
        {
            None = 0L,
            TeacherName = 1L << 0,
            Room = 1L << 1,
            Notation = 1L << 2,
            Name = 1L << 3,
            Evenness = 1L << 4,
            HelpSymbols = 1L << 5,
            Stream = 1L << 6,
            Course = 1L << 7,
        }
        private static readonly Regex TeacherNameRegex = new Regex("[А-Я][а-я]+ *([А-Я][\\.,] ?[А-Я][\\.,]?|[А-Я][а-я]{2,} +[А-Я][а-я]+(чна|вна|вич))");
        private static readonly Regex FourDigitsRoomNumRegex = new Regex("[1-9][0-9]{3}");
        private static readonly Regex LectureRoomNumRegex = new Regex("(10[8,9]|1310|1311|лекц)( к\\.? ?2( на Кремлевской 35).).", RegexOptions.IgnoreCase);
        private static readonly Regex NotationRegex = new Regex("\\(.{3,50}\\)");
        private static readonly Regex EvenOrOddWeekRegex = new Regex("[н, ч]\\.н\\.?", RegexOptions.IgnoreCase);
        private static readonly Regex OddWeekRegex = new Regex("н\\.н\\.?", RegexOptions.IgnoreCase);
        private static readonly Regex EvenWeekRegex = new Regex("ч\\.н\\.?", RegexOptions.IgnoreCase);

        static string ExtractTeacherName(string token)
        {
            var res = TeacherNameRegex.Match(token).Value;
            if (res.Length > 3 && !res[res.Length - 1].Equals('.') && (res[res.Length - 2].Equals('.') || res[res.Length - 3].Equals('.')))
                res += ".";
            if (res.Any() && res.Last().Equals(','))
                res = res.Substring(0, res.Length - 1) + ".";
            return res;
        }

        static string ExtractRoom(string token)
        {
            Match match = LectureRoomNumRegex.Match(token);
            if (match.Success)
                return match.Value;
            match = FourDigitsRoomNumRegex.Match(token);
            return match.Success ? match.Value : "~~";
        }

        static string ExtractNotation(string token)
        {
            var res = NotationRegex.Matches(token).Aggregate("",
                (result, match) => result + match.Value.Substring(1, match.Value.Length - 2) + ". ");
            return !String.IsNullOrWhiteSpace(res) ? res : null;
        }

        static bool? ExtractEvenness(string token)
        {
            return OddWeekRegex.IsMatch(token) ? false : EvenWeekRegex.IsMatch(token) ? true : (bool?) null;
        }

        static string ClearToken(string token, LessonParts partsToClear, string nameToRemove = null)
        {
            var sb = new StringBuilder(token);
            if (partsToClear.HasFlag(LessonParts.TeacherName))
                TeacherNameRegex.Matches(sb.ToString()).Select(match => sb.Replace(match.Value, null)).ToList();
            if (partsToClear.HasFlag(LessonParts.Room))
                sb.Replace(ExtractRoom(token), null);
            if (partsToClear.HasFlag(LessonParts.Name) && !String.IsNullOrEmpty(nameToRemove))
                sb.Replace(nameToRemove, null);
            if (partsToClear.HasFlag(LessonParts.Notation))
                NotationRegex.Matches(sb.ToString()).Select(match => sb.Replace(match.Value, null)).ToList();
            if (partsToClear.HasFlag(LessonParts.Evenness))
                EvenOrOddWeekRegex.Matches(sb.ToString()).Select(match => sb.Replace(match.Value, null)).ToList();
            if (partsToClear.HasFlag(LessonParts.Stream) && token.EndsWith("_1"))
                sb.Replace("_1", null, startIndex: sb.Length - 3, count: 1);
            if (partsToClear.HasFlag(LessonParts.Stream) && token.EndsWith("_2"))
                sb.Replace("_2", null, startIndex: sb.Length - 3, count: 1);
            if (partsToClear.HasFlag(LessonParts.HelpSymbols))
                sb.Replace("_", null)
                    .Replace(" .", null)
                    .Replace(" ,", null)
                    .Replace(". ", null)
                    .Replace(", ", null)
                    .Replace(";", null);
            if (partsToClear.HasFlag(LessonParts.Course))
                sb.Replace(ExtractCourseLabel(token), null);
            return sb.ToString().Trim();
        }
        
        static IEnumerable<(IScheduleElem, IScheduleGroup)> PrepareLectureOrSeminar(Lesson lesson, TableContext context,
            IEnumerable<IScheduleGroup> groupsForLecture)
        {
            if (lesson.Place.StartsWith("108") || lesson.Place.StartsWith("109"))
                return groupsForLecture.Select(g => new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, g));
            else
                return Enumerable.Repeat(new ValueTuple<IScheduleElem, IScheduleGroup>(lesson,
                        groupsForLecture.FirstOrDefault(group =>
                            @group.Name.Contains(context.CurrentGroupLabel,
                                StringComparison.InvariantCultureIgnoreCase)))
                    , 1);
        }

        public class SimpleLessonRule : ICellRule
        {
            public int EstimateApplicability(string cellText, TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                return TeacherNameRegex.Matches(cellText).Count == 1 ? 50 : Int32.MinValue;
            }

            public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText,
                TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                var lesson = new Lesson()
                {
                    BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                        LessonHoursLabelFormat,
                        CultureInfo.InvariantCulture),
                    Duration = TimeSpan.FromHours(1.5),
                    Level = ScheduleElemLevel.Lesson,
                    IsOnEvenWeek = ExtractEvenness(cellText),
                    Place = ExtractRoom(cellText),
                    Teacher = ExtractTeacherName(cellText)
                };
                lesson.Notation = ExtractNotation(cellText);
                lesson.Discipline = ClearToken(cellText,
                    LessonParts.TeacherName | LessonParts.Evenness | LessonParts.Notation | LessonParts.Room);
                return PrepareLectureOrSeminar(lesson, context,
                    availableGroups.Where(g =>
                        g.GType == ScheduleGroupType.Academic &&
                        g.Name.StartsWith(context.CurrentGroupLabel.Substring(0, 4))));
            }
        }

        public class EngLessonRule : ICellRule
        {
            public int EstimateApplicability(string cellText, TableContext context,
                IEnumerable<IScheduleGroup> availableGroups) =>
                cellText.ToLower().Contains("англ") ? 100 : Int32.MinValue;

            public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText,
                TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                var streamNumber = ExtractStreamNumber(context);
                var course = ExtractCourseLabel(context);
                var engGroups = availableGroups
                    .Where(g => g.GType == ScheduleGroupType.Eng && g.Name.EndsWith(streamNumber) &&
                                g.Name.Contains(course))
                    .Select(g => (Group: g, Teacher: ExtractTeacherName(g.Name))).ToList();
                return cellText.Substring(cellText.IndexOf("язык)") + 5)
                    .Split(",", StringSplitOptions.RemoveEmptyEntries).Select(elem => elem.Trim('.', ' '))
                    .Select(
                        teacherSet =>
                        {
                            var teacherFromSet = ExtractTeacherName(teacherSet);
                            if (string.IsNullOrWhiteSpace(teacherFromSet))
                                teacherFromSet = ClearToken(teacherSet, LessonParts.Evenness | LessonParts.Room | LessonParts.Notation);
                            var bestGroupEntry = engGroups.FirstOrDefault(groupEntry =>
                            {
                                return groupEntry.Teacher.Contains(teacherFromSet) ||
                                       teacherFromSet.Contains(groupEntry.Teacher) ||
                                       LevenshteinDistance.Compute(groupEntry.Teacher, teacherFromSet) < 3;
                            });
                            var lesson = new Lesson()
                            {
                                BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                                    LessonHoursLabelFormat,
                                    CultureInfo.InvariantCulture),
                                Discipline = "Английский язык",
                                Duration = TimeSpan.FromHours(1.5),
                                Level = ScheduleElemLevel.Lesson,
                                IsOnEvenWeek = teacherSet.Contains("ч.н") ? true :
                                    teacherSet.Contains("н.н") ? false : (bool?)null,
                                Place = ExtractRoom(teacherSet),
                                Teacher = bestGroupEntry.Teacher
                            };
                            lesson.Notation = ExtractNotation(teacherSet.Replace(lesson.Teacher, "")
                                .Replace(lesson.Place, ""));
                            return new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, bestGroupEntry.Group);
                        });
            }
        }
        public class LevenshteinBasedRule : ICellRule
        {
            private readonly int limitFailLengthPercentage;
            private readonly int maxScore;
            private readonly Dictionary<IScheduleGroup, (int Distance, string Token)> Result = new Dictionary<IScheduleGroup, (int Distance, string Token)>();
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
                var tokens = cellText.Split(new []{ ':', ','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var availableGroup 
                    in availableGroups.Where(g => g.GType != ScheduleGroupType.Academic && g.GType != ScheduleGroupType.Eng &&
                                g.Name.EndsWith(ExtractStreamNumber(context)) &&
                                g.Name.Contains(ExtractCourseLabel(context))))
                {
                    var clearGroupName =
                        ClearToken(availableGroup.Name, LessonParts.Notation | LessonParts.HelpSymbols | LessonParts.TeacherName);
                    var bestDistAndToken = tokens.Aggregate((Distance: Int32.MaxValue, BestToken: (string) null),
                        (bestEntry, current) =>
                        {
                            var curDistance = LevenshteinDistance.Compute(ClearToken(current,
                                LessonParts.Notation | LessonParts.TeacherName | LessonParts.Evenness | LessonParts.Room), clearGroupName);
                            if (curDistance < bestEntry.Distance)
                            {
                                bestEntry.Distance = curDistance;
                                bestEntry.BestToken = current;
                            }

                            return bestEntry;
                        });
                    if (bestDistAndToken.BestToken != null && (bestDistAndToken.Distance / (double) bestDistAndToken.BestToken.Length * 100) < limitFailLengthPercentage)
                    {
                        if (Result.TryGetValue(availableGroup,out var existingEntry))
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
                    return (int)Result.Values.Min(value => maxScore - value.Distance * distanceNormalizer);
                return Int32.MinValue;
            }

            public IEnumerable<(IScheduleElem ScheduleElem, IScheduleGroup Group)> SerializeElems(string cellText, TableContext context, IEnumerable<IScheduleGroup> availableGroups)
            {
                return Result.Select(pair =>
                {
                    var lesson = new Lesson()
                    {
                        BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5), LessonHoursLabelFormat,
                            CultureInfo.InvariantCulture),
                        Duration = TimeSpan.FromHours(1.5),
                        Level = ScheduleElemLevel.Lesson,
                        IsOnEvenWeek = ExtractEvenness(pair.Value.Token),
                        Place = ExtractRoom(pair.Value.Token),
                        Teacher = ExtractTeacherName(pair.Key.Name)
                    };
                    lesson.Notation = ExtractNotation(pair.Value.Token);
                    lesson.Discipline = ClearToken(pair.Key.Name,
                        LessonParts.TeacherName | LessonParts.HelpSymbols | LessonParts.Stream | LessonParts.Course);
                    return new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, pair.Key);
                }).ToList();

            }
        }

        public IList<ICellRule> GetCellHandlers()
        {
            return new List<ICellRule>()
            {
                //simple cell parser
                new SimpleLessonRule(),
                //eng parser
                new EngLessonRule(),
                //common leveinshtein distance rule
                new LevenshteinBasedRule(50, 100, 3d),
                //physic culture
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("УНИКС", StringComparison.InvariantCultureIgnoreCase) ? 80 : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {
                        var lesson = new Lesson()
                        {
                            BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                                LessonHoursLabelFormat,
                                CultureInfo.InvariantCulture),
                            Duration = TimeSpan.FromHours(1.5),
                            Level = ScheduleElemLevel.Lesson,
                            IsOnEvenWeek = ExtractEvenness(cellText),
                            Place = "УНИКС",
                            Teacher = string.Empty
                        };
                        lesson.Notation = ExtractNotation(cellText);
                        lesson.Discipline = "Физкультура";
                        //evaluate only academic groups of the same course
                        return availableGroups
                            .Where(g => g.GType == ScheduleGroupType.Academic &&
                                        g.Name.StartsWith(context.CurrentGroupLabel.Substring(0, 4))).Select(g =>
                                new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, g));
                    }
                },
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("проектный практикум", StringComparison.InvariantCultureIgnoreCase) ? Int32.MaxValue : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) => Enumerable.Empty<(IScheduleElem, IScheduleGroup)>()
                },
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("Кириллович А.") ? Int32.MaxValue : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) => new SimpleLessonRule().SerializeElems(cellText, context, availableGroups).Select(entry =>
                    {
                        var lesson = entry.ScheduleElem as Lesson;
                        if (lesson != null && lesson.Discipline.Contains("Кириллович А."))
                        {
                            lesson.Discipline = lesson.Discipline.Replace("Кириллович А.", null);
                            lesson.Teacher = "Кириллович А.";
                        }
                        return entry;
                    })
                },
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("Основы правоведения и противодействия коррупции Хасанов Р.А",
                                                                       StringComparison.InvariantCultureIgnoreCase)
                                                                   && cellText.Contains("Курс по выбору:",
                                                                       StringComparison.InvariantCultureIgnoreCase)  ? Int32.MaxValue : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {
                        var parts = cellText.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var res = new SimpleLessonRule().SerializeElems(parts[0], context, availableGroups).Select(entry =>
                        {
                            var lesson = entry.ScheduleElem as Lesson;
                            if (lesson != null && lesson.Discipline.Contains("для гр.11-508"))
                            {
                                lesson.Discipline = lesson.Discipline.Replace("для гр.11-508", null);
                                entry.Group = availableGroups.FirstOrDefault(g => g.Name.Contains("11-508"));
                            }
                            return entry;
                        });
                        res = res.Concat(
                            new LevenshteinBasedRule(50, 100, 3d).SerializeElems(parts[1], context, availableGroups));
                        return res;
                    }
                },
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("Основы правоведения и проти",
                                                                       StringComparison.InvariantCultureIgnoreCase)
                                                                   && cellText.Contains("Управление проектами",
                                                                       StringComparison.InvariantCultureIgnoreCase)  ? Int32.MaxValue : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {
                        var parts = cellText.Split(" , ", StringSplitOptions.RemoveEmptyEntries);
                        var res = new SimpleLessonRule().SerializeElems(parts[0], context, availableGroups);
                        res = res.Concat(
                            new SimpleLessonRule().SerializeElems(parts[1], context, availableGroups));
                        return res;
                    }
                },
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("Методология научных исследований : ",
                                                                       StringComparison.InvariantCultureIgnoreCase)  ? Int32.MaxValue : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {

                        var lesson = new Lesson()
                        {
                            Discipline = "Методология научных исследований",
                            Teacher = string.Empty,
                            Place = string.Empty,
                            BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                                LessonHoursLabelFormat,
                                CultureInfo.InvariantCulture),
                            Duration = TimeSpan.FromHours(1.5),
                            Level = ScheduleElemLevel.Lesson,
                            Notation = cellText.Split(" : ")[1]
                        };
                        return PrepareLectureOrSeminar(lesson, context,
                            availableGroups.Where(g =>
                                g.GType == ScheduleGroupType.Academic &&
                                g.Name.StartsWith(context.CurrentGroupLabel.Substring(0, 4))));
                    }
                },
            };
        }

        private static string ExtractCourseLabel(TableContext context)
        {
            var groupDigit = (int) char.GetNumericValue(context.CurrentGroupLabel[3]);
            switch (groupDigit)
            {
                case 7:
                    return "1курс";
                case 6:
                    return "2курс";
                case 5:
                    return "3курс";
                case 4:
                    return "4курс";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static string ExtractCourseLabel(string token)
        {
            var courseIdx = token.IndexOf("курс");
            if (courseIdx > 0)
                return token.Substring(courseIdx - 1, 5);
            return string.Empty;
        }

        private static string ExtractStreamNumber(TableContext context)
        {
            return context.CurrentGroupLabel.EndsWith('1') ? "1" : "2";
        }

        public IList<IScheduleGroup> GetGroups()
        {
            return new List<IScheduleGroup>()
            {
                //template
                /*
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-01"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-02"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-03"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-04"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-05"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-06"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-07"},
                new ScheduleGroup() { GType = ScheduleGroupType.Academic, Name = "11-08"},
                */
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-401"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-402"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-403"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-404"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-405"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-406"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-407"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-408"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-501"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-502"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-503"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-504"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-505"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-506"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-507"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-508"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-601"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-602"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-603"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-604"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-605"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-606"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-607"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-608"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-701"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-702"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-703"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-704"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-705"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-706"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-707"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-708"},
                new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-709"},


                //ENG GROUPS
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мартынова Е.В._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мартынова Е.В._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Н.А._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Н.А._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Макаев Х.Ф._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Баранова А.Р._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мельникова О.К._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Саляхова Г.И._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Саляхова Г.И._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Исмагилова Г.К._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Шамсутдинова Э.Х._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Переточкина С.М._1курс_2"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Саляхова Г.И._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Исмагилова Г.К._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Мартынова Е.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Н.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Махмутова А.Н._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Маршева Т.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сакаева Л.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Шамсутдинова Э.Х._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Яхин М.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сабирова Р.Н._2курс_1"},

                //Scientic groups
                    //third course
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Биоинформатика_Булыгина Е.А._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Вычислительная статистика_Новиков П.А._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Обработка текстов на естественном языке_Тутубаллина Е.В._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Введение в искусственный интеллект_Таланов М.О._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Физика_Мутыгуллина А.А._3курс_1"},
                    //second course
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "(Таланов) Введение в искусственный интеллект_Таланов М.О._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "(Закиров) Введение в искусственный интеллект_Закиров Л.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "(Кугуракова) Введение в искусственный интеллект_Кугуракова В.В._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Введение в робототехнику_Магид Е.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedScientic, Name = "Методы оптимизации_Фазылов В.Р._2курс_1"},

                //tech groups
                    //third course
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Введение в теорию и практику анимации_Костюк Д.И._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Шахова) Проектирование веб- интерфейсов_Шахова И.С._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Технологии Net_Гумеров К.А._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Эффективная разработка_Якупов А.Ш._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Скриптинг_Хусаинов Р.Р._3курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Гиниятуллин) Проектирование веб- интерфейсов_Гиниятуллин Р.Г._3курс_1"},
                    //second course
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Сидиков) Разработка корпоративных приложений_Сидиков М.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "(Аршинов) Разработка корпоративных приложений_Аршинов М.Р._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Интернет - программирование Django_Абрамский М.М._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Ruby_Бажанов В.А._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "PHP-_Кошарский И.Е._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Разработка мобильных приложений_Шахова И.С._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Визуализация данных_Костюк Д.И._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Программирование на С++_Сагитов А.Г._2курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Цифровая живопись_Евстафьев М.Е._2курс_1"},
                    //4th course
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Кроссплатформенное прикладное программирование_Магид Е.А._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Мобильные информационные системы_Хайруллин А.Ф._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Интернет вещей_Даутов Р.И._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Механизмы защиты удаленного доступа_Зиятдинов М.Т._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Визуализация данных_Костюк Д.И._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Проектирование человеко- машинных интерфейсов_Зайдуллин С.С._4курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.PickedTech, Name = "Аспектно-ориентированное проектирование и разработка_Костюк Д.И._4курс_1"},

            };
        }

        public IList<ICompatibleGroupsRule> GetGroupsRules()
        {
            return new List<ICompatibleGroupsRule>()
            {

                new SpecEngGroupsRule("1stCourse_1stStreamMartynova", (academicName, engName) =>
                {
                    if (engName.EndsWith("1") &&
                       engName.Contains("1курс"))
                    {
                        if (academicName.StartsWith("11-7") && Regex.IsMatch(academicName, $@"[1-4]$"))
                            return true;
                        else
                            return false;
                    }
                    return false;
                } ),
                new SpecEngGroupsRule("1stCourse_1stStreamNoMartynova", (academicName, engName) =>
                {
                    if (engName.EndsWith("1") &&
                        engName.Contains("1курс") && !engName.Contains("мартынова"))
                    {
                        if (academicName.StartsWith("11-7") && Regex.IsMatch(academicName, $@"[1-5]$"))
                            return true;
                        else
                            return false;
                    }
                    return false;
                } ),
                new CommonEngGroupsRule("1stCourse_2stStream", "2", "1курс", "11-7", $@"[6-9]$"),
                new CommonEngGroupsRule("2stCourse_1stStream", "1", "2курс", "11-6", $@"[1-8]$"),
                new CommonTypedRule("ScienticThirdCourse", ScheduleGroupType.PickedScientic, "11-5", "3курс"),
                new CommonTypedRule("ScienticSecondCourse", ScheduleGroupType.PickedScientic, "11-6", "2курс"),
                new CommonTypedRule("TechSecondCourse", ScheduleGroupType.PickedTech, "11-6", "2курс"),
                new CommonTypedRule("TechThirdCourse", ScheduleGroupType.PickedTech, "11-5", "3курс"),
                new CommonTypedRule("Tech4Course", ScheduleGroupType.PickedTech, "11-4", "4курс"),
            };
        }

        private class CommonTypedRule : CompatibleGroupsFuncRule
        {
            public CommonTypedRule(string name, ScheduleGroupType secondType,
                string academicStarts, string targetCourse, string stream = "1",
                string academicRegexp = null) : base(name, (g1, g2) => true)
            {
                CheckFunc = (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup typed = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == secondType)
                        typed = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == secondType)
                        typed = second;
                    if (typed == null || academic == null)
                        return false;
                    if (typed.Name.ToLowerInvariant().EndsWith(stream) &&
                        typed.Name.ToLowerInvariant().Contains(targetCourse))
                    {
                        var trimmed = (academic.Name.Trim());
                        return trimmed.StartsWith(academicStarts) && (academicRegexp == null || Regex.IsMatch(trimmed, academicRegexp));
                    }

                    return false;
                };
            }
        }

        private class CommonEngGroupsRule : CompatibleGroupsFuncRule
        {
            public CommonEngGroupsRule(string name, string engEndsWith, string engContains, string academicStarts, string academicRegexp) : base(name, (g1, g2) => true)
            {
                CheckFunc = (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup eng = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == ScheduleGroupType.Eng)
                        eng = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == ScheduleGroupType.Eng)
                        eng = second;
                    if (eng == null || academic == null)
                        return false;
                    if (eng.Name.ToLowerInvariant().EndsWith(engEndsWith) &&
                        eng.Name.ToLowerInvariant().Contains(engContains))
                    {
                        var trimmed = (academic.Name.Trim());
                        if (trimmed.StartsWith(academicStarts) && Regex.IsMatch(trimmed, academicRegexp))
                            return true;
                        else
                            return false;
                    }

                    return false;
                };
            }
        }

        private class SpecEngGroupsRule : CompatibleGroupsFuncRule
        {
            public SpecEngGroupsRule(string name, Func<string, string, bool> academicAndEngNamesFunc) : base(name, (g1, g2) => true)
            {
                CheckFunc = (first, second) =>
                {
                    IScheduleGroup academic = null;
                    IScheduleGroup eng = null;
                    if (first.GType == ScheduleGroupType.Academic)
                        academic = first;
                    if (first.GType == ScheduleGroupType.Eng)
                        eng = first;
                    if (second.GType == ScheduleGroupType.Academic)
                        academic = second;
                    if (second.GType == ScheduleGroupType.Eng)
                        eng = second;
                    if (eng == null || academic == null)
                        return false;
                    return academicAndEngNamesFunc(academic.Name.Trim().ToLowerInvariant(),
                        eng.Name.Trim().ToLowerInvariant());
                };
            }

        }
    }
    static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
