using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;

namespace ScheduleBot.AspHost.Updating
{
    public class UpdatesScheduler
    {
        private  Registry registry;
        readonly IServiceProvider services;
        public const int MinutesBetweenRefreshing = 5;

        public UpdatesScheduler(IServiceProvider services)
        {
            this.services = services;
        }
        public void Start()
        {
            if (registry != null)
            {
                JobManager.StopAndBlock();
                JobManager.RemoveAllJobs();
            }
            registry = new Registry();
            var job = services.GetRequiredService<UpdateJob>();
            registry.Schedule(job).ToRunEvery(MinutesBetweenRefreshing).Minutes();
            JobManager.Initialize(registry);
        }

        
    }
}
