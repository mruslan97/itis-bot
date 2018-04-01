using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using NUnit.Framework;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Providers.Storage;
using ScheduleServices.Core.Tests.Utils;
using Shouldly;

namespace ScheduleServices.Core.Tests.Providers.Storage
{
    [TestFixture]
    public class ScheduleDbStorageShould
    {
        private SchedulesInMemoryDbStorage storage;
        private ISchedule tree;
        [SetUp]
        public void SetUp()
        {
            
            //var weekRel = new TypeRelay(typeof(IScheduleElem), typeof(Week));
           
            
            storage = new SchedulesInMemoryDbStorage(new DefaultSchElemsFactory());
            var fixture = new Fixture();
            FixtureUtils.ConfigureFixtureForCreateSchedule(fixture);
            tree = fixture.Create<Schedule>();
            var day = fixture.Create<Day>();
            day.Elems = fixture.CreateMany<Lesson>(4).Cast<IScheduleElem>().ToList();
            var week = fixture.Create<Week>();
            week.Elems = new List<IScheduleElem>() {day};
            tree.ScheduleRoot = week;
            tree.ScheduleGroups = fixture.CreateMany<ScheduleGroup>().Cast<IScheduleGroup>().ToList();


        }

        [Test]
        public void BeAlive()
        {
            
            storage.UpdateScheduleAsync(tree.ScheduleGroups.FirstOrDefault(), tree.ScheduleRoot).Result.ShouldBeTrue();
            Assert.Pass();
        }

    }
}