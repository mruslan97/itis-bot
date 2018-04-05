using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost.Tests.BotServices
{
    [TestFixture]
    public class InMemoryBotStorageShould
    {
        private InMemoryBotStorage storage;
        private IScheduleService fakeService;
        private INotifiactionSender fakeNotificator;
        private IEnumerable<IScheduleGroup> availableGroups;
        [SetUp]
        public void SetUp()
        {
            var fixt = new Fixture();
            availableGroups = fixt.CreateMany<ScheduleGroup>(10);
            fakeService = A.Fake<IScheduleService>();
            fakeNotificator = A.Fake<INotifiactionSender>();
            storage = new InMemoryBotStorage(fakeService, fakeNotificator);
        }

    }
}
