using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FakeItEasy;
using NUnit.Framework;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Tests.BotServices
{
    [TestFixture]
    public class InMemoryBotStorageShould
    {
        private InMemoryBotStorage storage;
        private IScheduleService fakeService;
        private INotifiactionSender fakeNotificator;
        private IEnumerable<IScheduleGroup> availableGroups;
        private Fixture fixt = new Fixture();

        [SetUp]
        public void SetUp()
        {
            availableGroups = fixt.CreateMany<ScheduleGroup>(10);
            fakeService = A.Fake<IScheduleService>();
            fakeNotificator = A.Fake<INotifiactionSender>();
            storage = new InMemoryBotStorage(fakeService, fakeNotificator);
            IScheduleGroup @out;
            A.CallTo(() => fakeService.GroupsMonitor.TryGetCorrectGroup(null, out @out)).WithAnyArguments().Returns(true)
                .AssignsOutAndRefParametersLazily(
                    call => new List<object>() {call.Arguments[0]});
            
        }

        [Theory]
        public async Task NotifyUser_WhenGroupScheduleChanged()
        {
            var anyGroup = availableGroups.FirstOrDefault();
            var chat = fixt.Create<Chat>();
            await storage.TryAddGroupToChatAsync(anyGroup, chat);

            anyGroup.RaiseScheduleChanged(null, new ParamEventArgs<DayOfWeek>() {Param = DayOfWeek.Friday});

            A.CallTo(() => fakeNotificator.SendNotificationsForIdsAsync(null, null)).WhenArgumentsMatch(args =>
            {
                var ids = args.Get<IEnumerable<long>>(0);
                return ids.Count() == 1 && ids.Contains(chat.Id);
            }).MustHaveHappened();
        }
    }
}