using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TableRules.Core;

namespace ScheduleBot.AspHost.Setup
{
    [Flags]
    public enum LessonParts : long
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

    public static class ParsingTools
    {
        public static readonly Regex TeacherNameRegex = new Regex("[А-Я][а-я]+ *([А-Я][\\.,] ?[А-Я][\\.,]?|[А-Я][а-я]{2,} +[А-Я][а-я]+(чна|вна|вич))");
        public static readonly Regex FourDigitsRoomNumRegex = new Regex("[1-9][0-9]{3}");
        public static readonly Regex LectureRoomNumRegex = new Regex("(10[8,9]|1310|1311|лекц)( к\\.? ?2( на Кремлевской 35).).", RegexOptions.IgnoreCase);
        public static readonly Regex NotationRegex = new Regex("\\(.{3,50}\\)");
        public static readonly Regex EvenOrOddWeekRegex = new Regex("[н, ч]\\.н\\.?", RegexOptions.IgnoreCase);
        public static readonly Regex OddWeekRegex = new Regex("н\\.н\\.?", RegexOptions.IgnoreCase);
        public static readonly Regex EvenWeekRegex = new Regex("ч\\.н\\.?", RegexOptions.IgnoreCase);
        public const string LessonHoursLabelFormat = "hh\\.mm";

        public static string ExtractTeacherName(string token)
        {
            var res = TeacherNameRegex.Match(token).Value;
            if (res.Length > 3 && !res[res.Length - 1].Equals('.') && (res[res.Length - 2].Equals('.') || res[res.Length - 3].Equals('.')))
                res += ".";
            if (res.Any() && res.Last().Equals(','))
                res = res.Substring(0, res.Length - 1) + ".";
            return res;
        }

        public static string ExtractRoom(string token)
        {
            Match match = LectureRoomNumRegex.Match(token);
            if (match.Success)
                return match.Value;
            match = FourDigitsRoomNumRegex.Match(token);
            return match.Success ? match.Value : "~~";
        }

        public static string ExtractNotation(string token)
        {
            var res = NotationRegex.Matches(token).Aggregate("",
                (result, match) => result + match.Value.Substring(1, match.Value.Length - 2) + ". ");
            return !String.IsNullOrWhiteSpace(res) ? res : null;
        }

        public static bool? ExtractEvenness(string token)
        {
            return OddWeekRegex.IsMatch(token) ? false : EvenWeekRegex.IsMatch(token) ? true : (bool?) null;
        }

        public static string ClearToken(string token, LessonParts partsToClear, string nameToRemove = null)
        {

            var sb = new StringBuilder(token);
            if (partsToClear.HasFlag(LessonParts.Stream) && (token.EndsWith("_1") || token.EndsWith("_2")))
                sb.Remove(sb.Length - 2, 2);
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

        public static string ExtractCourseLabel(TableContext context)
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

        public static string ExtractCourseLabel(string token)
        {
            var courseIdx = token.IndexOf("курс");
            if (courseIdx > 0)
                return token.Substring(courseIdx - 1, 5);
            return string.Empty;
        }

        public static string ExtractStreamNumber(TableContext context)
        {
            return context.CurrentGroupLabel.EndsWith('1') ? "1" : "2";
        }
        public static class LevenshteinDistance
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
}