using System;
using System.Collections.Generic;
using System.Text;
using MagicParser.Impls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public static void AddGoogleApiParser(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.Get<GoogleApiConfig>();
            services.AddSingleton<IScheduleInfoProvider, ScheduleInfoProvider>(s => new ScheduleInfoProvider(config));
        }
    }
}
