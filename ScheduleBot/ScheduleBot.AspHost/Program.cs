using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace ScheduleBot.AspHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.CreateDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "/logs");
            Directory.CreateDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "/logs/messages");
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://+:8443")
                .UseNLog()
                .Build();
        }
    }
}