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
                .Replace("Иностранный язык ( английский язык)","")
                .Replace("  ", "");
            var subjects = input.Content.Split(',');
            foreach (var subject in subjects)
            {
                var cabinet = Regex.Match(subject, @"\d+").Value;
                var teacher = subject.Replace(cabinet, "");
                var flow = input.Group[input.Group.Length-1] == '1' ? 1 : 2;
                result.Add(new ParsedSubject
                {
                    SubjectName = "Иностранный язык",
                    Time = input.Time,
                    Cabinet = cabinet,
                    Teacher = teacher,
                    Type = ScheduleGroupType.Eng,
                    Group = input.Group,
                    Flow = flow,
                    Course = input.Course
            });
            }
            return result;
        }
    }
}
