using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;

namespace ScheduleServices.Core.Tests
{
    [TestFixture()]
    public class GroupsMonitorShould
    {
        private GroupsMonitor monitor;

        [SetUp]
        public void SetUp()
        {
            Fixture fixture = new Fixture();

            monitor = new GroupsMonitor(new List<IScheduleGroup>() {fixture.Create<ScheduleGroup>(), fixture.Create<ScheduleGroup>()});
            
            
        }

        [Test]
        public void Confirm_WhenGroupIsPresent()
        {
            //monitor.IsGroupPresent()
        }

    }
}
