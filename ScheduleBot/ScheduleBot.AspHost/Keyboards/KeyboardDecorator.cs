using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Keyboards
{
    public class AdditionalCoursesKeyboardDecorator : IKeyboardsFactory
    {
        private readonly IKeyboardsFactory keyboards;

        public AdditionalCoursesKeyboardDecorator(IKeyboardsFactory impl)
        {
            keyboards = impl;
        }

        public ReplyKeyboardMarkup GetCoursesKeyboad()
        {
            return keyboards.GetCoursesKeyboad();
        }

        public ReplyKeyboardMarkup GetGroupsOfCourseKeyboard(int course)
        {
            return keyboards.GetGroupsOfCourseKeyboard(course);
        }

        public ReplyKeyboardMarkup GetKeyboardForCollection<TItem>(IEnumerable<TItem> keyboardItems,
            Func<TItem, string> buttonTextSelector)
        {
            return keyboards.GetKeyboardForCollection(keyboardItems,
                (item) => buttonTextSelector(item).Substring(0, buttonTextSelector(item).IndexOf("_")));
        }

        public ReplyKeyboardMarkup GetMainOptionsKeyboard()
        {
            return keyboards.GetMainOptionsKeyboard();
        }

        public ReplyKeyboardMarkup GetSettingsKeyboard()
        {
            return keyboards.GetSettingsKeyboard();
        }
    }
    public class EngKeyboardDecorator : IKeyboardsFactory
    {
        private readonly IKeyboardsFactory keyboards;

        public EngKeyboardDecorator(IKeyboardsFactory impl)
        {
            keyboards = impl;
        }

        public ReplyKeyboardMarkup GetCoursesKeyboad()
        {
            return keyboards.GetCoursesKeyboad();
        }

        public ReplyKeyboardMarkup GetGroupsOfCourseKeyboard(int course)
        {
            return keyboards.GetGroupsOfCourseKeyboard(course);
        }

        public ReplyKeyboardMarkup GetKeyboardForCollection<TItem>(IEnumerable<TItem> keyboardItems, Func<TItem, string> buttonTextSelector)
        {
            return keyboards.GetKeyboardForCollection(keyboardItems, (item) => buttonTextSelector(item).Substring(0, buttonTextSelector(item).IndexOf("_")));
        }

        public ReplyKeyboardMarkup GetMainOptionsKeyboard()
        {
            return keyboards.GetMainOptionsKeyboard();
        }

        public ReplyKeyboardMarkup GetSettingsKeyboard()
        {
            return keyboards.GetSettingsKeyboard();
        }
    }

}
