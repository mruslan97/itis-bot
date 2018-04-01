using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;

namespace ScheduleServices.Core.Extensions
{
    public static class DefaultScheduleExtensions
    {
        public static ISchedule OrderScheduleRootAndChildren(this ISchedule schedule)
        {
            schedule.ScheduleRoot.OrderChildrenByDefault();
            return schedule;
        }
        public static IScheduleElem OrderChildrenByDefault(this IScheduleElem scheduleElem)
        {
            switch (scheduleElem.Level)
            {
                case ScheduleElemLevel.Week:
                    scheduleElem.Elems = scheduleElem.Elems.Select(d => d.OrderChildrenByDefault()).Cast<Day>()
                        .OrderBy(d => d.DayOfWeek).Cast<IScheduleElem>().ToList();
                    break;
                case ScheduleElemLevel.Day:
                    scheduleElem.Elems = scheduleElem.Elems.Cast<Lesson>().OrderBy(l => l.BeginTime)
                        .Cast<IScheduleElem>().ToList();
                    break;
                case ScheduleElemLevel.Undefined:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("scheduleElem",
                        $"Impossible to order children for such level {scheduleElem.Level}");
            }

            return scheduleElem;
        }

        public static CheckResult CheckElemIsCorrect(this IScheduleElem scheduleElem)
        {
            var res = new CheckResult();
            if (scheduleElem == null)
                return res.AddError("Null schedule elem");
            try
            {
                switch (scheduleElem.Level)
                {
                    case ScheduleElemLevel.Week:
                        if (!(scheduleElem is Week))
                            res.AddError($"level week is not week level {scheduleElem.Level}, type: {scheduleElem.GetType()}");
                        if (scheduleElem.Elems == null)
                            res.AddError("null children: " + JsonConvert.SerializeObject(scheduleElem));
                        else
                        {
                            res = scheduleElem.Elems?.Aggregate(res,
                                      (result, elem) => result + CheckElemIsCorrect(elem)) ?? res;
                            var nonUniqueDays = scheduleElem.Elems?.OfType<Day>()?.GroupBy(d => d.DayOfWeek)
                                ?.FirstOrDefault(group => @group.Count() > 1);
                            if (nonUniqueDays != null)
                                res.AddError($"not unique days in week: {nonUniqueDays.Key}");
                        }

                        break;
                    case ScheduleElemLevel.Day:
                        if (!(scheduleElem is Day))
                            res.AddError($"level day is not day: level {scheduleElem.Level}, type: {scheduleElem.GetType()}");
                        else
                        {
                            var day = (Day) scheduleElem;
                            if (day.DayOfWeek > DayOfWeek.Saturday || day.DayOfWeek < DayOfWeek.Sunday)
                                res.AddError($"unknown dayofweek: {day.DayOfWeek}");
                        }

                        if (scheduleElem.Elems == null)
                        {
                            res.AddError("null children: " + JsonConvert.SerializeObject(scheduleElem));
                        }
                        else
                        {
                            res = scheduleElem.Elems?.Aggregate(res,
                                      (result, elem) => result + CheckElemIsCorrect(elem)) ?? res;
                            scheduleElem.Elems?.OfType<Lesson>()?.OrderBy(l => l.BeginTime).Aggregate((prev, current) =>
                            {
                                if (current.BeginTime <= prev.BeginTime + prev.Duration &&
                                    (current.IsOnEvenWeek == null || prev.IsOnEvenWeek == null ||
                                     prev.IsOnEvenWeek == current.IsOnEvenWeek))
                                    res.AddError($"incompatible lessons in day {((Day)scheduleElem).DayOfWeek}:" +
                                                 JsonConvert.SerializeObject(prev) +
                                                 JsonConvert.SerializeObject(current));
                                return current;
                            });
                        }

                        break;
                    case ScheduleElemLevel.Lesson:
                        if (!(scheduleElem is Lesson))
                            res.AddError("level lesson is not lesson" + JsonConvert.SerializeObject(scheduleElem));
                        if (scheduleElem.Elems != null)
                        {
                            res.AddError("not null children for lesson: " +
                                         JsonConvert.SerializeObject(scheduleElem));
                        }

                        break;
                    default:
                        res.AddError($"Impossible to check for such level {scheduleElem.Level}");
                        break;
                }

                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return res.AddExeption(e);
            }
        }

        public class CheckResult
        {
            public CheckResult()
            {
                Successed = true;
            }

            public CheckResult(params string[] errorMessages)
            {
                Successed = false;
                foreach (var errorMessage in errorMessages)
                {
                    AddError(errorMessage);
                }
            }

            public CheckResult(params Exception[] exceptions)
            {
                Successed = false;
                foreach (var errorMessage in exceptions)
                {
                    AddExeption(errorMessage);
                }
            }

            public bool Successed { get; set; }
            private IList<string> errors;

            public IEnumerable<string> ErrorsList
            {
                get
                {
                    if (errors == null)
                        errors = new List<string>();
                    return errors;
                }
            }

            public CheckResult AddExeption(Exception ex)
            {
                Successed = false;
                if (errors == null)
                    errors = new List<string>();
                errors.Add(ex.ToString());
                return this;
            }

            public CheckResult AddError(string error)
            {
                Successed = false;
                if (errors == null)
                    errors = new List<string>();
                errors.Add(error);
                return this;
            }

            public static CheckResult operator +(CheckResult first, CheckResult second)
            {
                if (first.Successed)
                {
                    if (second.Successed)
                        return first;
                    return second;
                }

                if (second.errors != null)
                    first.errors = first.errors.Concat(second.errors).ToList();
                return first;
            }
        }
    }
}