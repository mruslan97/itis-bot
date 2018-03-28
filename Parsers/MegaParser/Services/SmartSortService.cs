using System;
using System.Collections.Generic;
using System.Linq;
using MegaParser.Helpers;
using MegaParser.Models;
using MegaParser.Parsers;

namespace MegaParser.Services
{
    public class SmartSortService
    {
        private readonly LectureParser _lectureParser = new LectureParser();
        private readonly SeminarParser _seminarParser = new SeminarParser();
        private readonly PhysCultureParser _physCultureParser = new PhysCultureParser();
        private readonly ElectiveParser _electiveParser = new ElectiveParser();
        private readonly EnglishParser _englishParser = new EnglishParser();

        public List<ParsedSubject> SortContent(List<TmpObject> inputSubjects)
        {
            var parsedSubjects = new List<ParsedSubject>();
            foreach (var unparsedSubject in inputSubjects)
            {
                if (Keywords.Lecture().Any(l => unparsedSubject.Content.Contains(l))
                    && !Keywords.NotLecture().Any(n => unparsedSubject.Content.Contains(n)))
                {
                    var parsedSubject = _lectureParser.Parse(unparsedSubject);
                    var marker = SetMarker(unparsedSubject);
                    parsedSubjects.AddRange(ShareSubjects(parsedSubject, marker));
                    continue;
                }

                if (Keywords.PhysCulture().Any(p => unparsedSubject.Content.Contains(p)))
                {
                    var parsedSubject = _physCultureParser.Parse(unparsedSubject);
                    var marker = SetMarker(unparsedSubject);
                    parsedSubjects.AddRange(ShareSubjects(parsedSubject, marker));
                    continue;
                }

                if (Keywords.ElectiveCourse().Any(p => unparsedSubject.Content.Contains(p)))
                {
                    var parsedSubject = _electiveParser.Parse(unparsedSubject);
                    var marker = SetMarker(unparsedSubject);
                    parsedSubjects.AddRange(ShareSubjects(parsedSubject, marker));
                    continue;
                }

                if (Keywords.English().Any(p => unparsedSubject.Content.Contains(p)))
                {
                    var parsedSubject = _englishParser.Parse(unparsedSubject);
                    var marker = SetMarker(unparsedSubject);
                    parsedSubjects.AddRange(ShareSubjects(parsedSubject, marker));
                    continue;
                }

                parsedSubjects.Add(_seminarParser.Parse(unparsedSubject));
            }

            return parsedSubjects;
        }

        private Tuple<int, int> SetMarker(TmpObject unparsedObject)
        {
            if (unparsedObject.Content.Contains("Абрамский"))
            {
                if (unparsedObject.Group.StartsWith("11-7"))
                    return new Tuple<int, int>(1, 5);
                if (unparsedObject.Group.StartsWith("11-6"))
                    return new Tuple<int, int>(1, 5);
            }

            if (unparsedObject.Content.Contains("Марченко"))
            {
                if (unparsedObject.Group.StartsWith("11-7"))
                    return new Tuple<int, int>(6, 9);
                if (unparsedObject.Group.StartsWith("11-6"))
                    return new Tuple<int, int>(6, 8);
            }

            if ((unparsedObject.Content.Contains("Макаев") || unparsedObject.Content.Contains("Мартынова")) &&
                unparsedObject.Group.StartsWith("11-7"))
                return new Tuple<int, int>(1, 5);

            if (unparsedObject.Content.Contains("Переточкина") && unparsedObject.Group.StartsWith("11-7"))
                return new Tuple<int, int>(6, 9);

            if (unparsedObject.Group.StartsWith("11-7"))
                return new Tuple<int, int>(1, 9);
            return new Tuple<int, int>(1, 8);
        }

        private IEnumerable<ParsedSubject> ShareSubjects(ParsedSubject parsedSubject, Tuple<int, int> marker)
        {
            var group = parsedSubject.Group.Substring(0, parsedSubject.Group.Length - 1);
            var sharedSubjects = new List<ParsedSubject>();
            for (var i = marker.Item1; i <= marker.Item2; i++)
                sharedSubjects.Add(new ParsedSubject
                {
                    Cabinet = parsedSubject.Cabinet,
                    Group = group + i,
                    Notation = parsedSubject.Notation,
                    SubjectName = parsedSubject.SubjectName,
                    Teacher = parsedSubject.Teacher,
                    Time = parsedSubject.Time
                });

            return sharedSubjects;
        }
    }
}