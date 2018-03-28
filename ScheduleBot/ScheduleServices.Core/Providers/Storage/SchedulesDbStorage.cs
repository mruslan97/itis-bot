using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Providers.Interfaces;

namespace ScheduleServices.Core.Providers.Storage
{
    public class SchedulesDbStorage : ISchedulesStorage
    {
        private readonly ScheduleMongoDbContext context;

        public SchedulesDbStorage(ScheduleMongoDbContext context)
        {
            this.context = context;
        }

        public Task<ISchedule> GetScheduleAsync(IScheduleGroup group, DayOfWeek day)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateScheduleAsync(IScheduleGroup targetGroup, IScheduleElem scheduleRoot)
        {
            var toUpd = new ScheduleMongoDbContext.SingleGroupSchedule()
            {
                Group = targetGroup,
                ScheduleRoot = scheduleRoot
            };
            var filter = new FilterDefinitionBuilder<ScheduleMongoDbContext.SingleGroupSchedule>().Where(schedule =>
                (int)schedule.Group.GType == (int)toUpd.Group.GType);
            var upd = new UpdateDefinitionBuilder<ScheduleMongoDbContext.SingleGroupSchedule>().Set(
                schedule => schedule.ScheduleRoot, toUpd.ScheduleRoot);
            var noone =new FindOneAndUpdateOptions<ScheduleMongoDbContext.SingleGroupSchedule>() { IsUpsert = true };
            try
            {
                
                await context.Schedules.FindOneAndUpdateAsync(filter, upd, noone);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class ScheduleMongoDbContext
    {
        private IMongoDatabase database;

        public ScheduleMongoDbContext(string connectionString)
        {
            // строка подключения

            var connection = new MongoUrlBuilder(connectionString);
            // получаем клиента для взаимодействия с базой данных
            MongoClient client = new MongoClient(connectionString);
            // получаем доступ к самой базе данных
            database = client.GetDatabase(connection.DatabaseName);
            //index
            var options = new CreateIndexOptions() {Unique = true};
            Expression<Func<SingleGroupSchedule, object>> selector = (sch) => sch.Group;
            var field = new ExpressionFieldDefinition<SingleGroupSchedule>(selector);
            database.GetCollection<SingleGroupSchedule>("schedules").Indexes
                .CreateOne(new IndexKeysDefinitionBuilder<SingleGroupSchedule>().Ascending(field), options);
        }

        public IMongoCollection<SingleGroupSchedule> Schedules
        {
            get { return database.GetCollection<SingleGroupSchedule>("schedules"); }
        }

        public class SingleGroupSchedule : Schedule
        {
            public IScheduleGroup Group { get; set; }

            //hide! do not use as Schedule or ISchedule
            [BsonIgnore]
            public new ICollection<IScheduleGroup> ScheduleGroups
            {
                get => new List<IScheduleGroup> {Group};
                set => Group = value.FirstOrDefault();
            }

            public static SingleGroupSchedule FromSchedule(ISchedule schedule)
            {
                return new SingleGroupSchedule()
                {
                    ScheduleGroups = schedule.ScheduleGroups,
                    ScheduleRoot = schedule.ScheduleRoot
                };
            }

            public static ISchedule ToSchedule(SingleGroupSchedule singleGroupSchedule)
            {
                return new Schedule()
                {
                    ScheduleGroups = singleGroupSchedule.ScheduleGroups,
                    ScheduleRoot = singleGroupSchedule.ScheduleRoot
                };
            }
        }
    }
}