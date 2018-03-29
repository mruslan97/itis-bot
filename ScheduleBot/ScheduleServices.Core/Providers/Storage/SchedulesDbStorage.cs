using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ScheduleServices.Core.Models;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules.Interfaces;
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

        public Task<IEnumerable<ISchedule>> GetScheduleAsync(IEnumerable<IScheduleGroup> availableGroups, DayOfWeek day)
        {
            //context.Schedules.Where(sc => sc.ScheduleGroups)
            return null;
        }

        public async Task<bool> UpdateScheduleAsync(IScheduleGroup targetGroup, IScheduleElem scheduleRoot)
        {
           
            var stored = context.Schedules.FirstOrDefault(sc =>
                sc.Group.GType == targetGroup.GType && sc.Group.Name == targetGroup.Name);
            if (stored != null)
            {
                stored.Group = targetGroup;
                stored.ScheduleRoot = scheduleRoot;
            }
            else
            {
                context.Schedules.Add(new SingleGroupSchedule() {Group = targetGroup, ScheduleRoot = scheduleRoot});
            }

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            

        }

        
    }

    [MongoDatabase("scheduleunits")]
    public class ScheduleMongoDbContext : DbContext
    {
        private string connectionString;
        public ScheduleMongoDbContext(string connectionString)
            : this(new DbContextOptions<ScheduleMongoDbContext>(), connectionString)
        {
        }
        public ScheduleMongoDbContext(DbContextOptions<ScheduleMongoDbContext> zooDbContextOptions, string connectionString)
            : base(zooDbContextOptions)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*var connectionString = "mongodb://localhost";
            //optionsBuilder.UseMongoDb(connectionString);

            
            //optionsBuilder.UseMongoDb(mongoUrl);
            */

            //settings.SslSettings = new SslSettings
            //{
            //    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            //};
            //optionsBuilder.UseMongoDb(settings);
            var mongoUrl = new MongoUrl(connectionString);
            MongoClientSettings settings = MongoClientSettings.FromUrl(mongoUrl);
            MongoClient mongoClient = new MongoClient(settings);
            optionsBuilder.UseMongoDb(mongoClient);
        }
        public DbSet<SingleGroupSchedule> Schedules { get; set; }



    }

    public class SingleGroupSchedule : Schedule
    {
        public IScheduleGroup Group { get; set; }

        //hide! do not use as Schedule or ISchedule
        [BsonIgnore]
        public new ICollection<IScheduleGroup> ScheduleGroups
        {
            get => new List<IScheduleGroup> { Group };
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