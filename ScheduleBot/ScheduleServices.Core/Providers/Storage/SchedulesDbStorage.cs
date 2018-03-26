using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
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

        public Task<bool> UpdateScheduleAsync(IScheduleGroup targetGroup, ISchedule schedule)
        {
            throw new NotImplementedException();
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
            
        }
        public IMongoCollection<Schedule> Schedules
        {
            get { return database.GetCollection<Schedule>("schedules"); }
        }
    }
}