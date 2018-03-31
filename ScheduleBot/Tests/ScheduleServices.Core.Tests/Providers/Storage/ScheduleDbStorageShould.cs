using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Providers.Storage;
using Shouldly;

namespace ScheduleServices.Core.Tests.Providers.Storage
{
    [TestFixture]
    public class ScheduleDbStorageShould
    {
        private SchedulesInMemoryDbStorage storage;
        private ScheduleMongoDbContext context;
        private ISchedule tree;
        [SetUp]
        public void SetUp()
        {
            
            //var weekRel = new TypeRelay(typeof(IScheduleElem), typeof(Week));
            context = new ScheduleMongoDbContext("mongodb://localhost:27017/scheduleunits");
            
            storage = new SchedulesInMemoryDbStorage(context);
            var fixture = new Fixture();
            var gen = new TypeGenerator();
            fixture.Customizations.Add(new TypeRelay(typeof(IScheduleElem), typeof(Week)));
            fixture.Customizations.Add(new TypeRelay(typeof(IScheduleGroup), typeof(ScheduleGroup)));
            fixture.Customize<IScheduleElem>((composer => composer.Without((elem => elem.Elems))));
            fixture.Customize<Schedule>((composer => composer.Without((elem => elem.ScheduleRoot))));
            fixture.Customize<Week>((composer => composer.Without((elem => elem.Elems))));
            fixture.Customize<Day>((composer => composer.Without((elem => elem.Elems))));
            fixture.Customize<Lesson>((composer => composer.Without((elem => elem.Elems))));
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
            storage = new SchedulesInMemoryDbStorage(context);
            storage.UpdateScheduleAsync(tree.ScheduleGroups.FirstOrDefault(), tree.ScheduleRoot).Result.ShouldBeTrue();
            Assert.Pass();
        }

    }
}