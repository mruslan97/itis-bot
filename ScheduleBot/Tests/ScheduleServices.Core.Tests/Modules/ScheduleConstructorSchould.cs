using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;

namespace ScheduleServices.Core.Tests.Modules
{
    [TestFixture]
    public class ScheduleConstructorSchould
    {
        private static DefaultSchElemsFactory factory = new DefaultSchElemsFactory();

        private ScheduleConstructor constructor;

        [SetUp]
        public void SetUp()
        {
            constructor = new ScheduleConstructor(factory);
        }

        [Test]
        [TestCaseSource(nameof(DifferentLayersEmptySchedules))]
        public async Task CollectSchedule_FromSingleSchedule(
            ValueTuple<ISchedule, IScheduleGroup, Week, Day, Lesson> schedule)
        {
            var res = await constructor.ConstructFromMany(new[] {schedule.Item1});

            Assert.AreEqual(schedule.Item1, res);
        }

        [Test]
        public void CollectSchedule_FromTwoWithDiffLevelsAndSameGroup()
        {
            var res = constructor.ConstructFromMany(new[]
                {GetEmpty2TopLayersSchedule(factory).Item1, GetEmpty2BottomLayersSchedule(factory).Item1}).Result;
            var control = GetEmpty3LayersSchedule(factory);

            Assert.AreEqual(control.Item1, res);
        }

        [Test]
        [TestCaseSource(nameof(PartitonalLayersEmptySchedules))]
        public void DoNotChangeFull_WhenCollectsFromFullAndItsPart(
            ValueTuple<ISchedule, IScheduleGroup, Week, Day, Lesson> part)
        {
            var control = GetEmpty3LayersSchedule(factory);

            var res = constructor.ConstructFromMany(new[] {GetEmpty3LayersSchedule(factory).Item1, part.Item1}).Result;


            Assert.AreEqual(control.Item1, res);
        }

        [Test]
        public void CollectGroups_WhenTwoGroupsAreDifferentByType()
        {
            var sch1 = GetEmpty3LayersSchedule(factory);
            sch1.Item2.GType = ScheduleGroupType.Eng;
            sch1.Item2.Name = "Other";
            var sch2 = GetEmpty3LayersSchedule(factory);
            //add changed group to default schedule
            var control = GetEmpty3LayersSchedule(factory);
            control.Item1.ScheduleGroups.Add(sch1.Item2);

            var res = constructor.ConstructFromMany(new[] {sch1.Item1, sch2.Item1}).Result;

            Assert.AreEqual(control.Item1, res);
        }

        [Test]
        public void ThrowException_WhenTwoGroupsHaveSameTypeButNotEqual()
        {
            var sch1 = GetEmpty3LayersSchedule(factory);
            sch1.Item2.GType = ScheduleGroupType.Academic;
            sch1.Item2.Name = "Other";
            var sch2 = GetEmpty3LayersSchedule(factory);

            Assert.ThrowsAsync<ScheduleConstructorException>(async () =>
                await constructor.ConstructFromMany(new[] {sch1.Item1, sch2.Item1}));
        }

        [Test]
        public void CollectTwoBottomLevelSchedules_ToOneTop()
        {
            var bottomLevel1 = GetEmpty2BottomLayersSchedule(factory);
            bottomLevel1.Item4.DayOfWeek = DayOfWeek.Friday;
            var bottomLevel2 = GetEmpty2BottomLayersSchedule(factory);
            var res = constructor.ConstructFromMany(new[] {bottomLevel2.Item1, bottomLevel1.Item1}).Result;
            Assert.True(res.ScheduleRoot.Level == ScheduleElemLevel.Week);
        }

        [Test]
        public void NotSaveStatement_BetweenDifferentActions()
        {
            var control = GetEmpty3LayersSchedule(factory);
            var copy = GetEmpty3LayersSchedule(factory);
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    CollectSchedule_FromSingleSchedule(copy).Wait();
                    CollectSchedule_FromTwoWithDiffLevelsAndSameGroup();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                var res = constructor.ConstructFromMany(new[] {copy.Item1}).Result;
                Assert.AreEqual(control.Item1, res);
                Assert.AreEqual(control.Item1, copy.Item1);
            }
        }

        private static IEnumerable<(ISchedule, IScheduleGroup, Week, Day, Lesson)> DifferentLayersEmptySchedules()
        {
            yield return GetEmpty3LayersSchedule(factory);
            foreach (var schedule in PartitonalLayersEmptySchedules())
            {
                yield return schedule;
            }
        }

        private static IEnumerable<(ISchedule, IScheduleGroup, Week, Day, Lesson)> PartitonalLayersEmptySchedules()
        {
            yield return GetEmpty2BottomLayersSchedule(factory);
            yield return GetEmpty2TopLayersSchedule(factory);
        }

        private static (ISchedule, IScheduleGroup, Week, Day, Lesson) GetEmpty3LayersSchedule(ISchElemsFactory factory)
        {
            var res = factory.GetSchedule();
            var group = new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-401"};
            res.ScheduleGroups.Add(group);
            var week = factory.GetWeek();
            res.ScheduleRoot = week;
            var day = factory.GetDay();
            var lesson = factory.GetLesson("test", "teacher", "place", new TimeSpan(8, 0, 0));
            day.Elems.Add(lesson);
            res.ScheduleRoot.Elems.Add(day);
            return (res, group, null, day, lesson);
        }

        private static (ISchedule, IScheduleGroup, Week, Day, Lesson) GetEmpty2TopLayersSchedule(
            ISchElemsFactory factory)
        {
            var res = factory.GetSchedule();
            var group = new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-401"};
            res.ScheduleGroups.Add(group);
            var week = factory.GetWeek();
            res.ScheduleRoot = week;
            var day = factory.GetDay();
            res.ScheduleRoot.Elems.Add(day);
            return (res, group, week, day, null);
        }

        private static (ISchedule, IScheduleGroup, Week, Day, Lesson) GetEmpty2BottomLayersSchedule(
            ISchElemsFactory factory)
        {
            var res = factory.GetSchedule();
            var group = new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-401"};
            res.ScheduleGroups.Add(group);
            var day = factory.GetDay();
            var lesson = factory.GetLesson("test", "teacher", "place", new TimeSpan(8, 0, 0));
            day.Elems.Add(lesson);
            res.ScheduleRoot = day;
            return (res, group, null, day, lesson);
        }
    }
}