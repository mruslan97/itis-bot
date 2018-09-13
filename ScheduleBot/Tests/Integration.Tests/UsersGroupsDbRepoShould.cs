using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ScheduleBot.AspHost.DAL;
using ScheduleBot.AspHost.DAL.Entities;
using ScheduleBot.AspHost.DAL.Repositories.Impls;
using ScheduleServices.Core.Models.Interfaces;
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
            using (var dbcontext = factory.CreateDbContext())
            {
                var groups = dbcontext.Groups.ToList();
                groups.ShouldContain(group1);
                groups.ShouldContain(group2);
                groups.Where(g => g.Name == group2.Name).ShouldHaveSingleItem();
            }

            
        }

        [Test]
        public async Task ReplaceSameGroup_ShouldNotAddNewGroup()
        {
            var profile = AddProfileToDb();
            var group1 = AddGroupToDb();
            BindGroupToUserInDb(profile, group1);
            var group2 = new ScheduleGroup()
            {
                Id = group1.Id,
                GType = group1.GType,
                Name = group1.Name
            };

            profile = await repository.FindUserByChatIdAsync(profile.ChatId);

            await repository.ReplaceGroupAsync(profile, group1, group2);

            var res = await repository.FindUserByChatIdAsync(profile.ChatId);

            res.ScheduleGroups.ShouldContain(group2);
            res.ScheduleGroups.Count().ShouldBe(1);
            using (var dbcontext = factory.CreateDbContext())
            {
                var groups = dbcontext.Groups.ToList();
                groups.ShouldContain(group1);
                groups.ShouldContain(group2);
                groups.Where(g => g.Name == group2.Name).ShouldHaveSingleItem();
            }


        }

        [Test]
        public async Task NotFall_WhenAnyCollectionIsEmpty_OnFiltering()
        {
            //take fakes and erase table
            var usedGroups = Enumerable.Range(0, 3).Select(i => AddGroupToDb()).ToList();
            var usedGroupsSourceFakes = usedGroups.Select(g => (ScheduleGroup)g.Clone()).ToList();
            using (var newContext = factory.CreateDbContext())
            {
                newContext.Groups.RemoveRange(newContext.Groups.ToList());
                newContext.SaveChanges();
            }
            //act 1 - empty db - all to add
            await repository.SyncGroupsFromSource(usedGroupsSourceFakes);
            //act 2 - empty source
            await repository.SyncGroupsFromSource(new List<IScheduleGroup>());

            //empty unused
            using (var newContext = factory.CreateDbContext())
            {
                newContext.Groups.AddRange(usedGroups);
                newContext.SaveChanges();
            }
            //act 3 - empty unused (dbGroups == sourceGroups)
            await repository.SyncGroupsFromSource(usedGroupsSourceFakes);

            //act 4 - empty source when db has values
            var unusedGroups = Enumerable.Range(0, 2).Select(i => AddGroupToDb()).ToList();
            await repository.SyncGroupsFromSource(new List<IScheduleGroup>());

            //erase
            using (var newContext = factory.CreateDbContext())
            {
                newContext.Groups.RemoveRange(newContext.Groups.ToList());
                newContext.SaveChanges();
            }

            //all to update
            usedGroups = Enumerable.Range(0, 3).Select(i => AddGroupToDb()).ToList();
            usedGroupsSourceFakes = usedGroups.Select(g =>
            {
                var group =  (ScheduleGroup) g.Clone();
                if (group.GType == ScheduleGroupType.PickedTech)
                    group.GType = 0;
                else
                    group.GType++;
                return group;
            }).ToList();
            //act 5 - all exists, no add, no remove, update required.
            await repository.SyncGroupsFromSource(usedGroupsSourceFakes);
        }

        [Test]
        public async Task RemoveUnusedGroups_AfterFiltering()
        {
            var unusedGroups = Enumerable.Range(0, 2).Select(i => AddGroupToDb()).ToList();
            var usedGroups = Enumerable.Range(0, 3).Select(i => AddGroupToDb()).ToList();
            var usedGroupsSourceFakes = usedGroups.Select(g => (ScheduleGroup) g.Clone()).ToList();

            await repository.SyncGroupsFromSource(usedGroupsSourceFakes);

            using (var newContext = factory.CreateDbContext())
            {
                var stored = newContext.Groups.ToList();
                stored.ShouldNotContain(unusedGroups[0]);
                stored.ShouldNotContain(unusedGroups[1]);
            }
        }

        [Test]
        public async Task AddNewGroups_AfterFiltering()
        {
            //add some groups to db
            var oldGroups = Enumerable.Range(0, 3).Select(i => AddGroupToDb()).ToList();
            //prepare some new
            var newGroups = Enumerable.Range(0, 2).Select(i =>
            {
                var group = fixture.Create<ScheduleGroup>();
                group.Id = 0;
                return group;
            }).ToList();
            

            await repository.SyncGroupsFromSource(newGroups.Concat(oldGroups));

            using (var newContext = factory.CreateDbContext())
            {
                var storedGroups = newContext.Groups.ToList();
                storedGroups.ShouldContain(newGroups[0]);
                storedGroups.ShouldContain(newGroups[1]);
                storedGroups.Where(sg => sg.Name == newGroups[0].Name).ShouldHaveSingleItem();
                storedGroups.Where(sg => sg.Name == newGroups[1].Name).ShouldHaveSingleItem();
            }
        }

        [Test]
        public async Task UpdateGroupsInDb_AfterFiltering()
        {
            //add some groups to db
            var oldGroups = Enumerable.Range(0, 2).Select(i => AddGroupToDb()).ToList();
            //update them
            var newGroups = oldGroups.Select(g =>
            {
                var group = (ScheduleGroup)g.Clone();
                if (group.GType == ScheduleGroupType.PickedTech)
                    group.GType = 0;
                else
                    group.GType++;
                return group;
            }).ToList();


            await repository.SyncGroupsFromSource(newGroups);

            using (var newContext = factory.CreateDbContext())
            {
                var storedGroups = newContext.Groups.ToList();
                storedGroups.ShouldNotContain(oldGroups[0]);
                storedGroups.ShouldNotContain(oldGroups[1]);
                storedGroups.Where(sg => sg.Name == newGroups[0].Name).ShouldHaveSingleItem();
                storedGroups.Where(sg => sg.Name == newGroups[1].Name).ShouldHaveSingleItem();
            }
        }

        [Test]
        public async Task UpdateIdsInSource_AfterFiltering()
        {
            //add some groups to db
            var oldGroups = Enumerable.Range(0, 2).Select(i => AddGroupToDb()).ToList();
            //update them
            var groupPairs = oldGroups.Select(g =>
            {
                var group = (ScheduleGroup)g.Clone();
                group.Id = -1;
                return new {DbGroup = g, SourceGroup = group};
            }).ToList();


            await repository.SyncGroupsFromSource(groupPairs.Select(gp => gp.SourceGroup));

            groupPairs.ShouldAllBe(gp => gp.SourceGroup.Id != -1 && gp.SourceGroup.Id == gp.DbGroup.Id);
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
