using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Keyboards
{
    public class KeyboardsFactory
    {
        private readonly IEnumerable<IScheduleGroup> setUpGroupsList;
        private readonly int highestCourseGroupNum;
        private readonly int[] courseGroupsCount = new int[4];

        public KeyboardsFactory(IEnumerable<IScheduleGroup> setUpGroupsList)
        {
            this.setUpGroupsList = setUpGroupsList;
            highestCourseGroupNum = setUpGroupsList.Where(g => g.GType == ScheduleGroupType.Academic).Max(g =>
                    int.TryParse(g.Name.Substring(g.Name.IndexOf("11-") + 3, 1), out int val) ? val : int.MinValue);
            for (int i = 0; i < courseGroupsCount.Length; i++)
                courseGroupsCount[i] = setUpGroupsList.Count(g =>
                    g.GType == ScheduleGroupType.Academic && g.Name.Contains("11-" + (highestCourseGroupNum - i)));
        }

        public ReplyKeyboardMarkup GetCoursesKeyboad()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] {new KeyboardButton("\u0031\u20E3 курс"), new KeyboardButton("\u0032\u20E3 курс") },
                new[] {new KeyboardButton("\u0033\u20E3 курс"), new KeyboardButton("\u0034\u20E3 курс") }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="course">one-based course num</param>
        /// <returns></returns>
        public ReplyKeyboardMarkup GetGroupsOfCourseKeyboard(int course)
        {
            //ok, think about it ;)
            if (course < 1 || course > 4)
                throw new ArgumentOutOfRangeException($"course cannot be {course}");
            bool lastLineThreeButtons = courseGroupsCount[course - 1] % 2 != 0;
            int buttonLinesCount = courseGroupsCount[course - 1] / 2;
            var markup = new ReplyKeyboardMarkup(new KeyboardButton[buttonLinesCount][]);
            int i = 0;
            foreach (var group in setUpGroupsList.Where(g =>
                    g.GType == ScheduleGroupType.Academic && g.Name.Contains("11-" + (highestCourseGroupNum - course + 1))))
            {
                if (!lastLineThreeButtons || i < courseGroupsCount[course - 1] - 3)
                {
                    if (i % 2 == 0)
                        markup.Keyboard[i / 2] = new KeyboardButton[2];
                    markup.Keyboard[i / 2][i % 2] = new KeyboardButton(group.Name);
                }
                else
                {
                    if (i  == courseGroupsCount[course - 1] - 3)
                        markup.Keyboard[buttonLinesCount - 1] = new KeyboardButton[3];
                    markup.Keyboard[buttonLinesCount - 1][i + 3 - courseGroupsCount[course - 1]] = new KeyboardButton(group.Name);
                }

                i++;
            }

            return markup;

        }

        public ReplyKeyboardMarkup GetPeriodOptionsKeyboard()
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
