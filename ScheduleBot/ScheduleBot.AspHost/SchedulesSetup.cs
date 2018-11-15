﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using TablesRulesCore;

namespace ScheduleBot.AspHost
{
    public class SchedulesSetup
    {
        private static readonly Regex TeacherNameRegex = new Regex("[А-Я][а-я]+ *[А-Я][\\.,][А-Я]");
        private static readonly Regex FourDigitsRoomNumRegex = new Regex("[1-9][0-9]{3}");

        static string ExtractTeacherName(string token)
        {
            return TeacherNameRegex.Match(token).Value + ".";
        }

        static string ExtractRoom(string token)
        {
            //todo: add rooms kind of '108 к.2'
            return FourDigitsRoomNumRegex.Match(token).Value;
        }

        static string ExtractNotation(string token)
        {
            throw new NotImplementedException();
        }

        public IList<ICellRule> GetCellHandlers()
        {
            return new List<ICellRule>()
            {
                //simple cell parser
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText) => TeacherNameRegex.Matches(cellText).Count == 1 ? 50 : int.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {
                        var lesson = new Lesson()
                        {
                            BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                                "hh.mm",
                                CultureInfo.InvariantCulture),
                            Duration = TimeSpan.FromHours(1.5),
                            Level = ScheduleElemLevel.Lesson,
                            IsOnEvenWeek = cellText.Contains("ч.н") ? true :
                                cellText.Contains("н.н") ? false : (bool?) null,
                            Place = ExtractRoom(cellText),
                            Teacher = ExtractTeacherName(cellText)
                        };
                        lesson.Notation = ExtractNotation(cellText.Replace(lesson.Teacher, null)
                            .Replace(lesson.Place, ""));
                        lesson.Discipline = cellText.Replace(lesson.Teacher, null).Replace(lesson.Place, null)
                            .Replace(string.IsNullOrEmpty(lesson.Notation) ? "~~" : lesson.Notation, null).Replace("ч.н", null).Replace("н.н", null).Trim();
                        return Enumerable.Repeat(new ValueTuple<IScheduleElem, IScheduleGroup>
                            (lesson,
                            availableGroups.FirstOrDefault(group =>
                                group.Name.Contains(context.CurrentGroupLabel,
                                    StringComparison.InvariantCultureIgnoreCase)))
                            , 1);
                    }
                },
                //eng parser
                new DelegateCellRule()
                {
                    ApplicabilityEstimator = (cellText) => cellText.ToLower().Contains("англ") ? 100 : int.MinValue,
                    Serializer = (cellText, context, availableGroups) =>
                    {
                        return cellText.Substring(cellText.IndexOf("язык)") + 5)
                            .Split(",", StringSplitOptions.RemoveEmptyEntries).Select(elem => elem.Trim('.', ' '))
                            .Select(
                                teacherSet =>
                                {
                                    var lesson = new Lesson()
                                    {
                                        BeginTime = TimeSpan.ParseExact(context.CurrentTimeLabel.Substring(0, 5),
                                            "hh.mm",
                                            CultureInfo.InvariantCulture),
                                        Discipline = "Английский язык",
                                        Duration = TimeSpan.FromHours(1.5),
                                        Level = ScheduleElemLevel.Lesson,
                                        IsOnEvenWeek = teacherSet.Contains("ч.н") ? true :
                                            teacherSet.Contains("н.н") ? false : (bool?) null,
                                        Place = ExtractRoom(teacherSet),
                                        Teacher = ExtractTeacherName(teacherSet)
                                    };
                                    lesson.Notation = ExtractNotation(teacherSet.Replace(lesson.Teacher, "")
                                        .Replace(lesson.Place, ""));
                                    return new ValueTuple<IScheduleElem, IScheduleGroup>(lesson,
                                        availableGroups.FirstOrDefault(group =>
                                            group.Name.Contains(lesson.Teacher,
                                                StringComparison.InvariantCultureIgnoreCase)));
                                });
                    }
                }
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
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Е.В._1курс_1"},
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Е.В._1курс_2"},
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
                new ScheduleGroup() { GType = ScheduleGroupType.Eng, Name = "Сигачева Е.В._2курс_1"},
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
