using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using NUnit.Framework;
using ScheduleBot.AspHost;

namespace Integration.Tests
{
    [TestFixture]
    public class HostIntegrationFixture
    {
        protected IWebHost Host;
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "/logs");
            Directory.CreateDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "/logs/messages");
            Host = new WebHostBuilder()
                .UseContentRoot(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString())
                .UseEnvironment("Testing")
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://+:8443")
                .UseNLog()
                .Build();
            Host.Start();
        }
        
        [OneTimeTearDown]
        public void TearDown()
        {
            Host.StopAsync().Wait();
        }

    }
}