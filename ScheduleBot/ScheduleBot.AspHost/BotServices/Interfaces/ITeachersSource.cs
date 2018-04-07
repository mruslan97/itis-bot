using System.Collections.Generic;
using Google.Apis.Http;

namespace ScheduleBot.AspHost.BotServices.Interfaces
{
    public interface ITeachersSource
    {
        IList<string> GetTeachersNames();
    }

    public class TeachersSourceMock : ITeachersSource
    {
        private static List<string> teachers = new List<string> { "Марченко", "Абрамский", "Мартынова" };
        public IList<string> GetTeachersNames() => teachers;
    }
}