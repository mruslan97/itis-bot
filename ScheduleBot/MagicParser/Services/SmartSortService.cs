using System;
using System.Collections.Generic;
using System.Linq;
using MagicParser.Helpers;
using MagicParser.Models;
using MagicParser.Parsers;
using ScheduleServices.Core.Models.Interfaces;

namespace MagicParser.Services
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
                unparsedSubject.Content = ProcessTrash(unparsedSubject);
                unparsedSubject.IsOnEvenWeek = CheckWeeks(unparsedSubject);
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

                if (Keywords.Scientic().Any(p => unparsedSubject.Content.Contains(p)))
                {
                    var parsedCourses = _electiveParser.Parse(unparsedSubject, ScheduleGroupType.PickedScientic);
                    parsedSubjects.AddRange(parsedCourses);
                    continue;
                }

                if (Keywords.Tech().Any(p => unparsedSubject.Content.Contains(p)))
                {
                    var parsedCourses = _electiveParser.Parse(unparsedSubject, ScheduleGroupType.PickedTech);
                    parsedSubjects.AddRange(parsedCourses);
                    continue;
                }

                if (Keywords.English().Any(p => unparsedSubject.Content.Contains(p)))
                {
                    var engSubjects = _englishParser.Parse(unparsedSubject);
                    parsedSubjects.AddRange(engSubjects);
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
                unparsedObject.Group.StartsWith("11-7") && !unparsedObject.Content.Contains("Переточкина"))
                return new Tuple<int, int>(1, 5);

            if (unparsedObject.Content.Contains("Переточкина") && unparsedObject.Group.StartsWith("11-7"))
                return new Tuple<int, int>(6, 9);

            if (unparsedObject.Group.StartsWith("11-7"))
                return new Tuple<int, int>(1, 9);
            return new Tuple<int, int>(1, 8);
        }

        private bool? CheckWeeks(TmpObject inputSubject)
        {
            if (inputSubject.Content.Contains("н.н"))
                return false;
            if (inputSubject.Content.Contains("ч.н"))
                return true;
            return null;
        }

        private string ProcessTrash(TmpObject input)
        {
            return input.Content
                .Replace("на Кремлёвской 35", "(на Кремлёвской 35)")
                .Replace("( Введение в исскуственный интеллект)", "Введение в исскуственный интеллект")
                .Replace("Основы правоведения и противодействия коррупции Хасанов Р.А. ч.н.1308 для гр.11-508 ,","")
                .Replace("д.гл.(прак.)","")
                .Replace("гр.1","")
                .Replace("гр.2","")
                .Replace("Технологии Net- д.гл.Гумеров К.", "Технологии Net Гумеров К.А.")
                .Replace("1310- 1311","1310")
                .Replace("Кугуракова В В.", "Кугуракова В.В.")
                .Replace("корпоротивных", "корпоративных")
                .Replace("М. 13", "М.Р. 13")
                .Replace("ХайруллинА.Ф.", "Хайруллин А.Ф.")
                .Replace("Костюк Д.", "Костюк Д.И.")
                .Replace("Курс по выбору :   ,Введение в теорию и практику", "Курс по выбору :  Введение в теорию и практику")
                .Replace("Проектный практикум( рас LAB),   Курс по выбору: Разработка корпоративных приложений Сидиков М.Р. в 1302", "Курс по выбору: Разработка корпоративных приложений Сидиков М.Р. в 1302");
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