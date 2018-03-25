using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleServices.Core.Factories;
using ScheduleServices.Core.Factories.Interafaces;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleGroups;
using ScheduleServices.Core.Modules;

namespace ScheduleServices.Core.Tests.Modules
{
    [TestFixture]
    public class ScheduleConstructorSchould
    {
        private DefaultSchElemsFactory factory = new DefaultSchElemsFactory();
        [Test]
        public void CollectSchedule_FromSingleSchedule()
        {
            ScheduleConstructor constructor = new ScheduleConstructor(factory);
            var schedule = GetEmpty3LayersSchedule(factory);

            var res = constructor.ConstructFromMany(new[] {schedule}).Result;
            Assert.True(res.Equals(schedule));


        }

        private static ISchedule GetEmpty3LayersSchedule(ISchElemsFactory factory)
        {
            var res = factory.GetSchedule();
            res.ScheduleGroups.Add(new ScheduleGroup() {GType = ScheduleGroupType.Academic, Name = "11-401"});
            res.ScheduleRoot = factory.GetWeek();
            var day = factory.GetDay();
            day.Elems.Add(factory.GetLesson("test", "teacher", "place", new TimeSpan(8, 0, 0)));
            res.ScheduleRoot.Elems.Add(day);
            return res;
        }
    }
}
