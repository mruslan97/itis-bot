using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ScheduleBot.AspHost.DAL;
using ScheduleBot.AspHost.DAL.Entities;
using ScheduleBot.AspHost.DAL.Repositories.Impls;
using ScheduleServices.Core.Models.ScheduleGroups;
using Shouldly;

namespace Integration.Tests
{
    [TestFixture]
    public class UsersGroupsDbRepoShould
    {
        private UsersContext context;
        private UsersContextFactory factory;
        private UsersGroupsDbRepository repository;
        private Fixture fixture;

        [OneTimeSetUp]
        public void SetUp()
        {
            var serviceProvider = new ServiceCollection().AddEntityFrameworkNpgsql()
                .BuildServiceProvider();
            var builder = new DbContextOptionsBuilder<UsersContext>();
            builder.UseNpgsql($"Server=localhost;Database=Test_ItisScheduleBot_{DateTime.UtcNow};Username=postgres;Password=postgres")
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .UseInternalServiceProvider(serviceProvider);
            context = new UsersContext(builder.Options);
            context.Database.Migrate();
            factory = new UsersContextFactory(builder.Options);
            repository = new UsersGroupsDbRepository(factory);
            fixture = new Fixture();
        }

        [Test]
        public async Task ReturnsAllUsersWithGroup_AfterNewPairAdded()
        {
            var profile = AddProfileToDb();
            var group = AddGroupToDb();
            BindGroupToUserInDb(profile, group);

            var allusers = await repository.GetAllUsersWithGroupsAsync();

            allusers.ShouldContain(u =>
                u.ChatId == profile.ChatId && 
                u.ScheduleGroups.Contains(group));
        }

        [Test]
        public async Task FindUserFromDbByChatId()
        {
            var profile = AddProfileToDb();

            var resultProfile = await repository.FindUserByChatIdAsync(profile.ChatId);

            resultProfile.Name.ShouldBe(profile.Name);
            resultProfile.ChatId.ShouldBe(profile.ChatId);
        }

        [Test]
        public async Task AttachGroupToUser()
        {
            var profile = AddProfileToDb();
            var group = AddGroupToDb();
            await repository.AddGroupToUserAsync(profile, group);

            var resUser = await repository.FindUserByChatIdAsync(profile.ChatId);

            resUser.ScheduleGroups.ShouldContain(group);
        }

        [Test]
        public async Task AddNewGroup_WhenSetSingleGroupCalled()
        {
            var profile = AddProfileToDb();
            var newGroup = AddGroupToDb();

            await repository.SetSingleGroupToUserAsync(profile, newGroup);

            var res = await repository.FindUserByChatIdAsync(profile.ChatId);

            res.ScheduleGroups.ShouldContain(newGroup);
        }

        [Test]
        public async Task RemoveOtherGroups_WhenSetSingleGroupCalled()
        {
            var profile = AddProfileToDb();
            var group = AddGroupToDb();
            BindGroupToUserInDb(profile, group);
            var newGroup = AddGroupToDb();

            profile = await repository.FindUserByChatIdAsync(profile.ChatId);

            await repository.SetSingleGroupToUserAsync(profile, newGroup);

            var res = await repository.FindUserByChatIdAsync(profile.ChatId);

            res.ScheduleGroups.ShouldNotContain(group);
        }

        [Test]
        public async Task ReplaceGroup()
        {
            var profile = AddProfileToDb();
            var group1 = AddGroupToDb();
            BindGroupToUserInDb(profile, group1);
            var group2 = AddGroupToDb();

            profile = await repository.FindUserByChatIdAsync(profile.ChatId);

            await repository.ReplaceGroupAsync(profile, group1, group2);

            var res = await repository.FindUserByChatIdAsync(profile.ChatId);

            res.ScheduleGroups.ShouldNotContain(group1);
            res.ScheduleGroups.ShouldContain(group2);
            res.ScheduleGroups.Count().ShouldBe(1);
        }

        #region Helper methods

        private Profile AddProfileToDb()
        {
            var profile = new Profile() { ChatId = fixture.Create<long>(), Name = fixture.Create<string>() };
            context.Profiles.Add(profile);
            context.SaveChanges();
            return context.Profiles.Include(p => p.ProfileAndGroups).ThenInclude(pg => pg.Group).AsNoTracking().FirstOrDefault(p => p.ChatId == profile.ChatId && p.Name == profile.Name);
        }

        private ScheduleGroup AddGroupToDb()
        {
            var group = fixture.Create<ScheduleGroup>();
            group.Id = 0;
            context.Groups.Add(group);
            context.SaveChanges();
            return context.Groups.AsNoTracking().FirstOrDefault(g => g.GType == group.GType && g.Name == group.Name);
        }

        private void BindGroupToUserInDb(Profile user, ScheduleGroup group)
        {
            context.ProfileAndGroups.Add(new ProfileAndGroup()
            {
                ProfileId = user.Id,
                GroupId = group.Id
            });
            context.SaveChanges();
        }

        #endregion


    }
}
