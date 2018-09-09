﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.DAL;
using ScheduleBot.AspHost.DAL.Repositories.Impls;
using ScheduleBot.AspHost.DAL.Repositories.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using Shouldly;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Tests.BotServices
{
    [TestFixture]
    public class InMemoryBotStorageShould
    {
        private InMemoryBotStorage storage;
        private IScheduleService fakeService;
        private INotifiactionSender fakeNotificator;
        private IUsersGroupsRepository fakeRepository;
        private IEnumerable<IScheduleGroup> availableGroups;
        private Fixture fixt = new Fixture();

        [SetUp]
        public void SetUp()
        {
            availableGroups = fixt.CreateMany<ScheduleGroup>(10);
            fakeService = A.Fake<IScheduleService>();
            fakeNotificator = A.Fake<INotifiactionSender>();
            fakeRepository = A.Fake<IUsersGroupsRepository>();
            storage = new InMemoryBotStorage(fakeService, fakeNotificator, fakeRepository);
            IScheduleGroup @out;
            A.CallTo(() => fakeService.GroupsMonitor.TryGetCorrectGroup(null, out @out)).WithAnyArguments().Returns(true)
                .AssignsOutAndRefParametersLazily(
                    call => new List<object>() {call.Arguments[0]});
            A.CallTo(() => fakeService.GroupsMonitor.AvailableGroups).WithAnyArguments().Returns(availableGroups);
            A.CallTo(() => fakeService.GroupsMonitor.TryFindGroupByName("", out @out)).WithAnyArguments().Returns(true)
                .AssignsOutAndRefParametersLazily(
                    call => new List<object>() { availableGroups.FirstOrDefault(g => g.Name.ToLowerInvariant().Contains(call.Arguments[0]?.ToString().ToLowerInvariant())) });

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

        [Theory]
        public async Task ReturnsAllGroups_AssociatedWithUser()
        {
            var tuple = await CreateAndAddChatWithTwoGroups();

            var usersGroups = await storage.GetGroupsForChatAsync(tuple.chat);

            tuple.twoGroups.ForEach(originalGroup => 
                usersGroups.ShouldContain(originalGroup)
                );
            Assert.IsTrue(usersGroups.Count() == tuple.twoGroups.Count);
        }
        
        private async Task<(List<IScheduleGroup> twoGroups, Chat chat)> CreateAndAddChatWithTwoGroups()
        {
            var twoGroups = availableGroups.Take(1).ToList();
            var chat = fixt.Create<Chat>();
            foreach (var scheduleGroup in twoGroups)
            {
                (scheduleGroup as ScheduleGroup).Id = 0;
                await storage.TryAddGroupToChatAsync(scheduleGroup, chat);
            }

            return (twoGroups, chat);
        }

        [Theory]
        //todo: move to integration?
        public async Task SaveUserGroups_BetweenSeveralInstances()
        {
            //preparing 'real' repo
            var serviceProvider = new ServiceCollection().AddEntityFrameworkNpgsql()
                .BuildServiceProvider();
            var builder = new DbContextOptionsBuilder<UsersContext>();
            builder.UseNpgsql($"Server=localhost;Database=Test_ItisScheduleBot_InMemory_{DateTime.UtcNow};Username=postgres;Password=postgres")
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .UseInternalServiceProvider(serviceProvider);
            var context = new UsersContext(builder.Options);
            context.Database.Migrate();
            var repository = new UsersGroupsDbRepository(new UsersContextFactory(builder.Options));


            storage = new InMemoryBotStorage(fakeService, fakeNotificator, repository);
            var tuple = await CreateAndAddChatWithTwoGroups();

            var userGroupsFromFirstInstance = (await storage.GetGroupsForChatAsync(tuple.chat)).ToList();
            await Task.Delay(1000);
            var userGroupsFromSecondInstance = (await (new InMemoryBotStorage(fakeService, fakeNotificator, repository)).GetGroupsForChatAsync(tuple.chat)).ToList();
            userGroupsFromFirstInstance.ForEach(originalGroup =>
                userGroupsFromSecondInstance.ShouldContain(originalGroup)
            );
            Assert.IsTrue(userGroupsFromFirstInstance.Count == userGroupsFromSecondInstance.Count);
        }
    }
}