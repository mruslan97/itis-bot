using System;
using System.Collections.Generic;
using System.Text;
using MagicParser.Impls;
using MagicParser.Parsers;
using MagicParser.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleServices.Core.Providers.Interfaces;

namespace MagicParser.Configuration
{
    public class GoogleApiConfig
    {
        public string ApplicationName { get; set; }
        public  string SpreadsheetId { get; set; }
    }
    
    public static class ConfigureExtension
    {
        //tell me someone why it's not working pls
        /*private class SmartServiceFactory : ScheduleInfoProvider.ISortServiceFactory
        {
            private readonly IServiceProvider prov;

            public SmartServiceFactory(IServiceProvider prov)
            {
                this.prov = prov;
            }
            public SmartSortService Get()
            {
                var s = prov.GetRequiredService<ISchedulesStorage>();
                return prov.GetService<SmartSortService>();
            }
        }*/
        public static void AddGoogleApiParser(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.Get<GoogleApiConfig>();
            services.AddSingleton<IScheduleInfoProvider, ScheduleInfoProvider>(s => 
                new ScheduleInfoProvider(
                    config,
                    s.GetRequiredService<SmartSortService>()
                    )
            );
            //parsers
            services.AddTransient<EnglishParser>();
            services.AddTransient<LectureParser>();
            services.AddTransient<SeminarParser>();
            services.AddTransient<PhysCultureParser>();
            services.AddTransient<ElectiveParser> ();
            services.AddTransient<SmartSortService>();
        }
    }
}
