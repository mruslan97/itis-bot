using System.Collections.Generic;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;

namespace ScheduleServices.Core.Tests.Modules
{
    [TestFixture()]
    public class GroupsMonitorShould
    {
        private GroupsMonitor monitor;

        [SetUp]
        public void SetUp()
        {
            Fixture fixture = new Fixture();

            monitor = new GroupsMonitor(new List<IScheduleGroup>()
            {
                fixture.Create<ScheduleGroup>(),
                fixture.Create<ScheduleGroup>()
            });
        }

        [Test]
        public void Confirm_WhenGroupIsPresent()
        {
            var toAdd = A.Dummy<ScheduleGroup>();
            monitor = new GroupsMonitor(new List<IScheduleGroup>() {toAdd});
            Assert.True(monitor.IsGroupPresent(toAdd));
        }

        [Test]
        public void ResponseFalse_WhenNoSuchGroup()
        {
            var other = A.Dummy<ScheduleGroup>();
            Assert.False(monitor.IsGroupPresent(other));
        }
    }
}