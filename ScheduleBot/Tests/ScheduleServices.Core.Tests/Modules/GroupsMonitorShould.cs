using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;
using Shouldly;

namespace ScheduleServices.Core.Tests.Modules
{
    [TestFixture]
    public class GroupsMonitorShould
    {
        private GroupsMonitor monitor;
        private Fixture fixture = new Fixture();
        private IEnumerable<IScheduleGroup> presetted;
        [SetUp]
        public void SetUp()
        {

            presetted = fixture.CreateMany<ScheduleGroup>(5);
            monitor = new GroupsMonitor(presetted);
        }

        [Test]
        public void Confirm_WhenGroupIsPresent()
        {
            var toAdd = fixture.Create<ScheduleGroup>();
            monitor = new GroupsMonitor(new List<IScheduleGroup>() {toAdd});
            Assert.True(monitor.IsGroupPresent(toAdd));
        }

        [Test]
        public void ResponseFalse_WhenNoSuchGroup()
        {
            var other = fixture.Create<ScheduleGroup>();
            Assert.False(monitor.IsGroupPresent(other));
        }

        [Test]
        public void RemoveBadGroups()
        {
            var infected = presetted.Append(fixture.Create<ScheduleGroup>()).ToList();
            
            Assert.AreEqual(presetted, monitor.RemoveInvalidGroupsFrom(infected));
        }

        [Test]
        public void ReturnAllGroups_ByRequest()
        {
            var available = monitor.AvailableGroups;
            
            presetted.ShouldBeSubsetOf(monitor.AvailableGroups);
            available.ShouldBeSubsetOf(presetted);
            
        }
        [Test]
        public void HaveImmutableGroups()
        {
            var changed = monitor.AvailableGroups.FirstOrDefault();
            changed.Name = "bad name";
            
            Assert.False(monitor.IsGroupPresent(changed));
            Assert.False(monitor.AvailableGroups.Contains(changed));
        }

        [Test]
        public void ReturnStoredGroup_ByOtherWithSameName()
        {
            var stored = presetted.FirstOrDefault();
            var bad = fixture.Create<ScheduleGroup>();
            bad.Name = stored.Name;
            
            monitor.TryGetCorrectGroup(bad, out IScheduleGroup restored).ShouldBe(true);
            restored.ShouldBe(stored);
           
        }

        [Test]
        public void ResponseFalse_WhenTryGetByIncorrectGroup()
        {
            var bad = fixture.Create<ScheduleGroup>();
            monitor.TryGetCorrectGroup(bad, out IScheduleGroup restored).ShouldBe(false);
        }
    }
}