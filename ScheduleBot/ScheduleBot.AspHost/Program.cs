using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;

namespace ScheduleBot.AspHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var testGroup = new ScheduleGroup {GType = ScheduleGroupType.Academic, Name = "11-507"};
            //var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BotStorage\\" + "usersgroups.xml";
            //var xdoc = XDocument.Load(path);
            //Console.WriteLine(testGroup.GType.ToString());
            //var element = xdoc.Element("users")
            //    ?.Elements("user").Where(u =>
            //        u.Element("chatId")?.Value == "259862692" &&
            //        u.Element("groupType")?.Value == testGroup.GType.ToString());

            //xdoc.Element("users")
            //    .Add(new XElement("user",
            //        new XAttribute("name", "ruslan"),
            //        new XElement("groupType", testGroup.GType.ToString()),
            //        new XElement("chatId", "259862692"),
            //        new XElement("value", testGroup.Name)));
            //xdoc.Save(path);
            //Console.ReadKey();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();
    }
}
