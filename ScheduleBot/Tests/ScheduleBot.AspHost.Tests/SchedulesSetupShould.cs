using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace ScheduleBot.AspHost.Tests
{
    [TestFixture]
    public class SchedulesSetupShould
    {
        [Test]
        public void ParseOptionalCoursesCells()
        {
            var cellText =
                "Курс по выбору: Биоинформатика Булыгина Е.А. 1309, Вычислительная статистика Новиков П.А. 1404, " +
                "Обработка текстов на естественном языке Тутубаллина Е.В. 1307, Таланов М.О.( Введение в исскуственный" +
                " интеллект) 1408 ( 15.03 занятия переносится на 31.05).";

        }
    }
}
