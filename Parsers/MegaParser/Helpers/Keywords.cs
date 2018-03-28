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

        public static List<string> Trash()
        {
            return new List<string>
            {
                "Информатика Абрамский М.М. в 108 к.2 , Марченко А.А. в 1310-1311 к.2 Кремлевская 35  ( 28.02 не будет с перносом на 11.04 в 11.50 в 1310-1311) 9н.							",
                "Физика д.гл. Мутыгуллина А.А. ( лекция) в 112 ф.ф.  ,Основы правоведения и противодействия коррупции Хасанов Р.А. ч.н.1308 для гр.11-508							",
                "Методология научных исследований : Абрамский М.М. 1409, Кугуракова В.В  1408, Шахова И.С. 1412, Голицына И.Н.1308 (гр.Гайсина)							",
                "Методология научных исследований  Голицына И.Н.гр. № 1 ( практика) 1508( гр.401, 404, 407,408)							",
                "Методология научных исследований  Голицына И.Н.гр. № 2 (практика) 1508 (402,403,405,406)							",
            };
        }
    }
}
