using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using ScheduleBot.AspHost.DAL.Entities;
using DAL.Common;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost.DAL
{
    public class UsersContext : BaseDbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options)
        {
        }
        /// <summary>
        /// Users profiles
        /// </summary>
        public DbSet<Profile> Profiles { get; set; }

        /// <summary>
        /// Users profiles
        /// </summary>
        public DbSet<ProfileAndGroup> ProfileAndGroups { get; set; }
        /// <summary>
        /// Groups
        /// </summary>
        public DbSet<ScheduleGroup> Groups { get; set; }

        
    }
    public class UsersContextFactory : IDesignTimeDbContextFactory<UsersContext>
    {
        private readonly DbContextOptionsBuilder<UsersContext> options;

        public UsersContextFactory(DbContextOptionsBuilder<UsersContext> options)
        {
            this.options = options;
        }
        public UsersContext CreateDbContext(string[] args)
        {
            return new UsersContext(options.Options);
        }
    }
}