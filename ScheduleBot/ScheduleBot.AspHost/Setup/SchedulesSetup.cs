using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using TableRules.Core;

namespace ScheduleBot.AspHost.Setup
{
    public class SchedulesSetup
    {
        public IList<ICellRule> GetCellHandlers()
        {
            return new List<ICellRule>()
            {
                //simple cell parser
                new CellHandlingRules.SimpleLessonRule(),
                //eng parser
                new CellHandlingRules.EngLessonRule(),
                //common leveinshtein distance rule
                new CellHandlingRules.LevenshteinBasedRule(50, 100, 3d),
                //physic culture
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText, groups) => cellText.Contains("УНИКС", StringComparison.InvariantCultureIgnoreCase) ? 80 : Int32.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {
                        var lesson = new Lesson()
                        {
                            BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5), ParsingTools.LessonHoursLabelFormat,
                                CultureInfo.InvariantCulture),
                            Duration = TimeSpan.FromHours(1.5),
                            Level = ScheduleElemLevel.Lesson,
                            IsOnEvenWeek = ParsingTools.ExtractEvenness(cellText),
                            Place = "УНИКС",
                            Teacher = string.Empty
                        };
                        lesson.Notation = ParsingTools.ExtractNotation(cellText);
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
                    Serializer = (cellText, context, availableGroups) => new CellHandlingRules.SimpleLessonRule().SerializeElems(cellText, context, availableGroups).Select(entry =>
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
                        var res = new CellHandlingRules.SimpleLessonRule().SerializeElems(parts[0], context, availableGroups).Select(entry =>
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
                            new CellHandlingRules.LevenshteinBasedRule(50, 100, 3d).SerializeElems(parts[1], context, availableGroups));
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
                        var res = new CellHandlingRules.SimpleLessonRule().SerializeElems(parts[0], context, availableGroups);
                        res = res.Concat(
                            new CellHandlingRules.SimpleLessonRule().SerializeElems(parts[1], context, availableGroups));
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
                            BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5), ParsingTools.LessonHoursLabelFormat,
                                CultureInfo.InvariantCulture),
                            Duration = TimeSpan.FromHours(1.5),
                            Level = ScheduleElemLevel.Lesson,
                            Notation = cellText.Split(" : ")[1]
                        };
                        return CellHandlingRules.PrepareLectureOrSeminar(lesson, context,
                            availableGroups.Where(g =>
                                g.GType == ScheduleGroupType.Academic &&
                                g.Name.StartsWith(context.CurrentGroupLabel.Substring(0, 4))));
                    }
                },
            };
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
}
