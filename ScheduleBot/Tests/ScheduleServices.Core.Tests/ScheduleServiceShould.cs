using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Modules;
using ScheduleServices.Core.Modules.Interfaces;
using ScheduleServices.Core.Providers.Interfaces;
using ScheduleServices.Core.Tests.Utils;

namespace ScheduleServices.Core.Tests
{
    [TestFixture]
    public class ScheduleServiceShould
    {
        private ScheduleService service;
        private IScheduleInfoProvider infoProviderFake;
        private ISchedulesStorage storageFake;
        private IGroupsMonitor monitorFake;
        private List<ISchedule> freshDays;
        private List<ISchedule> storedDays;
        private List<IScheduleGroup> groups;

        [SetUp]
        public void SetUp()
        {
            Fixture fixture = new Fixture();
            FixtureUtils.ConfigureFixtureForCreateSchedule(fixture);
            freshDays = new List<ISchedule>();
            for (int i = 0; i < 4; i++)
                freshDays.Add(FixtureUtils.CreateFixtureDaySchedule(3, 1, fixture));
            groups = new List<IScheduleGroup>();
            freshDays.ForEach(d => groups.Add(d.ScheduleGroups.First()));
            storedDays = new List<ISchedule>();
            for (int i = 0; i < 3; i++)
                storedDays.Add(FixtureUtils.CreateFixtureDaySchedule(3, 1, fixture));
            storedDays.Select((d, i) => d.ScheduleGroups = new[] {groups[i]}).ToList();
            infoProviderFake = A.Fake<IScheduleInfoProvider>();
            A.CallTo(() => infoProviderFake.GetSchedules(null, default(DayOfWeek))).WithAnyArguments().Returns(freshDays);
            storageFake = A.Fake<ISchedulesStorage>();
            A.CallTo(() => storageFake.GetSchedules(null, default(DayOfWeek))).WithAnyArguments().Returns(storedDays);
            monitorFake = A.Fake<IGroupsMonitor>();
            A.CallTo(() => monitorFake.AvailableGroups).Returns(groups);

            //service = new ScheduleService(storageFake, monitorFake, infoProviderFake, new DefaultEventArgsFactory());
        }

        [Test]
        public async Task UpdateStorage_WhenStorageDoesNotContainScheduleAndUpdateCalled()
        {
            var diffGroup = groups.Except(storedDays.Select(d => d.ScheduleGroups.FirstOrDefault())).FirstOrDefault();
            var rootToUpd = freshDays.FirstOrDefault(d => d.ScheduleGroups.FirstOrDefault().Equals(diffGroup))
                .ScheduleRoot;
            //act
            await service.UpdateSchedulesAsync(groups, 0);
            //assert
            A.CallTo(() => storageFake.UpdateScheduleAsync(diffGroup, rootToUpd)).MustHaveHappened();
        }

        [Test]
        public async Task RemoveFromStorage_WhenGetLessGroupsFromSource()
        {
            var anyCommonDay = freshDays.FirstOrDefault();
            storedDays = freshDays.ToList();
            freshDays.Clear();
            freshDays.Add(anyCommonDay);

            await service.UpdateSchedulesAsync(groups, ((Day)anyCommonDay.ScheduleRoot).DayOfWeek);
            var group = anyCommonDay.ScheduleGroups.FirstOrDefault();
            A.CallTo(() => storageFake.RemoveScheduleAsync(null, 0)).WithAnyArguments().MustHaveHappened();
            A.CallTo(() => storageFake.RemoveScheduleAsync(group, ((Day)anyCommonDay.ScheduleRoot).DayOfWeek)).MustNotHaveHappened();
        }
    }
}
