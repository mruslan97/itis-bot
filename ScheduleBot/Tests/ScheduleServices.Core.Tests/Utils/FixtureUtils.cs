using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture;
using AutoFixture.Kernel;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleServices.Core.Tests.Utils
{
    public class FixtureUtils
    {
        public static void ConfigureFixtureForCreateSchedule(Fixture fixture)
        {
            fixture.Customizations.Add(new TypeRelay(typeof(IScheduleElem), typeof(Week)));
            fixture.Customizations.Add(new TypeRelay(typeof(IScheduleGroup), typeof(ScheduleGroup)));
            fixture.Customize<IScheduleElem>((composer => composer.Without((elem => elem.Elems))));
            fixture.Customize<Schedule>((composer => composer.Without((elem => elem.ScheduleRoot))));
            fixture.Customize<Week>((composer => composer.Without((elem => elem.Elems))));
            fixture.Customize<Day>((composer => composer.Without((elem => elem.Elems))));
            fixture.Customize<Lesson>((composer => composer.Without((elem => elem.Elems))));
        }

        public static Schedule CreateFixtureDaySchedule(int lessons, int groups, Fixture configuredFixture)
        {
            return CreateFixtureDayScheduleWithOption(lessons, groups, configuredFixture, true);
        }
        public static Schedule CreateFixtureWeekSchedule(int lessons, int days, int groups, Fixture configuredFixture)
        {
            var tree = configuredFixture.Create<Schedule>();
            tree.ScheduleRoot = configuredFixture.Create<Week>();
            tree.ScheduleRoot.Elems = new List<IScheduleElem>();
            for (int i = 1; i <= days; i++)
            {
                var day = (Day)CreateFixtureDayScheduleWithOption(lessons, groups, configuredFixture, false).ScheduleRoot;
                day.DayOfWeek = (DayOfWeek) i;
                tree.ScheduleRoot.Elems.Add(day);
            }
            tree.ScheduleGroups = configuredFixture.CreateMany<ScheduleGroup>(groups).Cast<IScheduleGroup>().ToList();
            return tree;
        }
        private static Schedule CreateFixtureDayScheduleWithOption(int lessons, int groups, Fixture configuredFixture, bool withGroups)
        {
            var tree = configuredFixture.Create<Schedule>();
            var day = configuredFixture.Create<Day>();
            day.Elems = configuredFixture.CreateMany<Lesson>(lessons).Cast<IScheduleElem>().ToList();
            if (withGroups)
                tree.ScheduleGroups = configuredFixture.CreateMany<ScheduleGroup>(groups).Cast<IScheduleGroup>().ToList();
            tree.ScheduleRoot = day;
            return tree;
        }
    }
}
