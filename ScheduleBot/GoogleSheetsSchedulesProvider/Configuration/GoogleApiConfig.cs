using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScheduleServices.Core.Providers.Interfaces;
using TablesRulesCore;

namespace GoogleSheetsSchedulesProvider.Configuration
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
            services.AddSingleton<IScheduleInfoProvider, ScheduleInfoProvider>(s =>
                new ScheduleInfoProvider(
                    config,
                    s.GetRequiredService<IEnumerable<ICellRule>>()
                    )
            );
        }
    }
}
