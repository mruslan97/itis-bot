using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MagicParser.Models;
using ScheduleServices.Core.Models.Interfaces;

namespace MagicParser.Parsers
{
    public class EnglishParser 
    {
        public IEnumerable<ParsedSubject> Parse(TmpObject input)
        {
            var result = new List<ParsedSubject>();
            input.Content = input.Content
                .Replace("Иностранный язык (английский язык) \r\n ", "")
                .Replace("Иностранный язык (английский язык) \n","")
                .Replace("  ", "");
            var subjects = input.Content.Split(',');
            foreach (var subject in subjects)
            {
                var cabinet = Regex.Match(subject, @"\d+").Value;
                var teacher = subject.Replace(cabinet, "");
                result.Add(new ParsedSubject
                {
                    SubjectName = "Иностранный язык",
                    Time = input.Time,
                    Cabinet = cabinet,
                    Teacher = teacher,
                    Type = ScheduleGroupType.Eng
            });
            }
            return result;
        }
    }
}
