using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Keyboards
{
    public interface IKeyboardsFactory
    {
        ReplyKeyboardMarkup GetCoursesKeyboad();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="course">one-based course num</param>
        /// <returns></returns>
        ReplyKeyboardMarkup GetGroupsOfCourseKeyboard(int course);

        ReplyKeyboardMarkup GetKeyboardForCollection<TItem>(IEnumerable<TItem> keyboardItems, Func<TItem, string> buttonTextSelector);
        ReplyKeyboardMarkup GetPeriodOptionsKeyboard();
        ReplyKeyboardMarkup GetSettingsKeyboard();
    }
}