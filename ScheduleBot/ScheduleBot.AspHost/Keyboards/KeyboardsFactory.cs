using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Keyboards
{
    public class KeyboardsFactory : IKeyboardsFactory
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
            return GetKeyboardForCollectionWithItemsCount(setUpGroupsList.Where(g =>
                g.GType == ScheduleGroupType.Academic && g.Name.Contains("11-" + (highestCourseGroupNum - course + 1))), g => g.Name, courseGroupsCount[course - 1]);

        }
        private ReplyKeyboardMarkup GetKeyboardForCollectionWithItemsCount<TItem>(IEnumerable<TItem> keyboardItems, Func<TItem, string> buttonTextSelector, int itemsCount)
        {
            bool lastLineThreeButtons = itemsCount % 2 != 0 && itemsCount > 2;
            int buttonLinesCount = itemsCount > 2 ? (itemsCount / 2)  : 1;
            var markup = new ReplyKeyboardMarkup(new KeyboardButton[buttonLinesCount][]);
            if (itemsCount == 0)
                return new ReplyKeyboardMarkup(new[] {
                new []{new KeyboardButton("ничего нет")}
            });
            int i = 0;
            foreach (var item in keyboardItems)
            {
                if (!lastLineThreeButtons || i < itemsCount - 3)
                {
                    if (i % 2 == 0)
                        markup.Keyboard[i / 2] = new KeyboardButton[2];
                    markup.Keyboard[i / 2][i % 2] = new KeyboardButton(buttonTextSelector(item));
                }
                else
                {
                    if (i == itemsCount - 3)
                        markup.Keyboard[buttonLinesCount - 1] = new KeyboardButton[3];
                    markup.Keyboard[buttonLinesCount - 1][i + 3 - itemsCount] = new KeyboardButton(buttonTextSelector(item));
                }

                i++;
            }

            return markup;

        }
        
        public ReplyKeyboardMarkup GetKeyboardForCollection<TItem>(IEnumerable<TItem> keyboardItems, Func<TItem, string> buttonTextSelector)
        {
            if (keyboardItems == null)
                throw new ArgumentNullException("collection for keyboard is null");
            return GetKeyboardForCollectionWithItemsCount(keyboardItems, buttonTextSelector, keyboardItems.Count());
        }

        public ReplyKeyboardMarkup GetMainOptionsKeyboard()
        {
            return new ReplyKeyboardMarkup(new[] {
                new []{new KeyboardButton("На сегодня"), new KeyboardButton("На завтра"), new KeyboardButton("На неделю")},
                new []{new KeyboardButton("Найти преподавателя")},
                new []{new KeyboardButton("Настройки")}
            });
        }

        public ReplyKeyboardMarkup GetSettingsKeyboard()
        {
            return new ReplyKeyboardMarkup(new[] {
                new []{new KeyboardButton("Eng"), new KeyboardButton("Tech"), new KeyboardButton("Science")},
                new []{new KeyboardButton("Сменить группу")},
                new []{new KeyboardButton("Back"), new KeyboardButton("Для разработчиков") }
            });
        }
    }
}
