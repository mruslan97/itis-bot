using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Keyboards
{
    public class CustomKeyboards
    {
        public static ReplyKeyboardMarkup Courses()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("1 курс"), new KeyboardButton("2 курс")},
                new[] {new KeyboardButton("3 курс"), new KeyboardButton("4 курс")}
            });
        }

        public static ReplyKeyboardMarkup FirstCourse()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("11-701"), new KeyboardButton("11-702") },
                new[] {new KeyboardButton("11-703"), new KeyboardButton("11-704") },
                new[] {new KeyboardButton("11-705"), new KeyboardButton("11-706") },
                new[] {new KeyboardButton("11-707"), new KeyboardButton("11-708"), new KeyboardButton("11-709"),  },
            });
        }

        public static ReplyKeyboardMarkup SecondCourse()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("11-601"), new KeyboardButton("11-602") },
                new[] {new KeyboardButton("11-603"), new KeyboardButton("11-604") },
                new[] {new KeyboardButton("11-605"), new KeyboardButton("11-606") },
                new[] {new KeyboardButton("11-607"), new KeyboardButton("11-608") }
            });
        }

        public static ReplyKeyboardMarkup ThirdCourse()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("11-501"), new KeyboardButton("11-502") },
                new[] {new KeyboardButton("11-503"), new KeyboardButton("11-504") },
                new[] {new KeyboardButton("11-505"), new KeyboardButton("11-506") },
                new[] {new KeyboardButton("11-507"), new KeyboardButton("11-508") }
            });
        }

        public static ReplyKeyboardMarkup LastCourse()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("11-401"), new KeyboardButton("11-402") },
                new[] {new KeyboardButton("11-403"), new KeyboardButton("11-404") },
                new[] {new KeyboardButton("11-405"), new KeyboardButton("11-406") },
                new[] {new KeyboardButton("11-407"), new KeyboardButton("11-408") }
            });
        }

        public static ReplyKeyboardMarkup Days()
        {
            return new ReplyKeyboardMarkup(new[] {
                new []{new KeyboardButton("На сегодня")},
                new []{new KeyboardButton("На завтра")},
                new []{new KeyboardButton("На неделю")},
                new []{new KeyboardButton("Настройки")}
            });
        }
    }
}
