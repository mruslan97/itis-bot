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

        public List<ParsedSubject> Parse(List<TmpObject> inputSubjects)
        {
            var parsedSubjects = new List<ParsedSubject>();
            foreach (var unparsedSubject in inputSubjects)
            {
                var lectures = Keywords.Lecture();
                if (lectures.Any(l => unparsedSubject.Content.Contains(l)))
                {
                    parsedSubjects.Add(_lectureParser.Parse(unparsedSubject));
                    continue;
                }

                var physicalCulture = Keywords.PhysCulture();
                if (physicalCulture.Any(p => unparsedSubject.Content.Contains(p)))
                {
                    parsedSubjects.Add(_physCultureParser.Parse(unparsedSubject));
                    continue;
                }

                var electives = Keywords.ElectiveCourse();
                if (electives.Any(p => unparsedSubject.Content.Contains(p)))
                {
                    parsedSubjects.Add(_electiveParser.Parse(unparsedSubject));
                    continue;
                }

                var english = Keywords.English();
                if (english.Any(p => unparsedSubject.Content.Contains(p)))
                {
                    parsedSubjects.Add(_englishParser.Parse(unparsedSubject));
                    continue;
                }

                parsedSubjects.Add(_seminarParser.Parse(unparsedSubject));
            }

            return parsedSubjects;
        }
    }
}