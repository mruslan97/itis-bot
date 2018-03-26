using System;
using System.Collections.Generic;
using System.Text;

namespace MegaParser.Helpers
{
    public class Keywords
    {
        public static List<string> Lecture()
        {
            return new List<string>
            {
                "108",
                "109",
                "1310",
            };
        }

        public static List<string> PhysCulture()
        {
            return new List<string>
            {
                "Уникс",
                "УНИКС"
            };
        }

        public static List<string> ElectiveCourse()
        {
            return new List<string>
            {
                "Курс по выбору",
                "Физика"
            };
        }

        public static List<string> English()
        {
            return new List<string>
            {
                "Иностранный язык"
            };
        }
    }
}
