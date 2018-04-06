using System.Collections.Generic;
using System.Text.RegularExpressions;
using MagicParser.Models;
using ScheduleServices.Core.Models.Interfaces;

namespace MagicParser.Parsers
{
    public class ElectiveParser
    {
        public IEnumerable<ParsedSubject> Parse(TmpObject input, ScheduleGroupType groupType)
        {
            var result = new List<ParsedSubject>();
            input.Content = input.Content
                .Replace("Курс по выбору :", "")
                .Replace("Курс по выбору:", "")
                .Replace("Курс по выбору :   ,","");
            var subjects = input.Content.Split(',');
            var lastSubjectName = "";
            foreach (var subject in subjects)
            {
                var fixedName = Regex.Match(subject, @"[А-ЯA-Z].*").Value;
                var cabinet = Regex.Match(fixedName, @"\d+").Value;
                var teacher = Regex.Match(fixedName, @"[А-Я][а-я]+\s[А-Я].[А-Я].").Value;
                var notation = "";
                if (fixedName.Contains("("))
                    notation = fixedName.Split('(', ')')[1];
                var subjectName = fixedName
                    .Replace(teacher, "")
                    .Replace(cabinet, "")
                    .Replace("()", "")
                    .Replace("  ", "");
                if (notation.Length > 0)
                {
                    subjectName = subjectName.Replace(notation, "");
                }
                subjectName = Regex.Replace(subjectName, @"[\d-]", string.Empty);
                var parsedSubject = new ParsedSubject
                {
                    SubjectName = subjectName,
                    Time = input.Time,
                    Cabinet = cabinet,
                    Teacher = teacher,
                    Type = groupType,
                    Notation = '(' +notation + ')'
                };
                if (parsedSubject.SubjectName.Length < 5)
                    parsedSubject.SubjectName = lastSubjectName;
                else
                    lastSubjectName = subjectName;
                result.Add(parsedSubject);
            }

            return result;
        }
    }
}