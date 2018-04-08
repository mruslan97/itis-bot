using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Helpers;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.ScheduleElems;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Commads.TeacherSearchCommands
{
    public class GetTeachersListCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly ITeachersSource teachers;
        private readonly IKeyboardsFactory keyboards;

        public GetTeachersListCommand(ITeachersSource teachers, IKeyboardsFactory keyboards) : base("leclist")
        {
            this.teachers = teachers;
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
                return update.Message.Text.ToLowerInvariant().Contains("преподавател");
            return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var replyList = teachers.GetTeachersNames().Where(t => t.Length>2).Take(10).ToList();
            var teachersButtonsCount = replyList.Count/2;
            var endButtons = new[]
            {
                InlineKeyboardButton.WithCallbackData("➡️")
            };
            var keyboard = new InlineKeyboardButton[teachersButtonsCount + 1][];
            var keyboardCounter = 0;
            for (int i = 0; i < keyboard.Length - 1; i++)
            {
                keyboard[i] = new[] {InlineKeyboardButton.WithCallbackData(replyList[keyboardCounter]), InlineKeyboardButton.WithCallbackData(replyList[keyboardCounter+1]) };
                keyboardCounter += 2;
            }

            keyboard[keyboard.Length - 1] = endButtons;
            var inlineKeyboard = new InlineKeyboardMarkup(keyboard);

            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Список преподавателей 1", replyMarkup: inlineKeyboard);


            return UpdateHandlingResult.Handled;
        }
    }

    public class UpdTeachersListCommand : InlineCommand
    {
        private readonly ITeachersSource teachers;
        private readonly IKeyboardsFactory keyboards;

        public UpdTeachersListCommand(ITeachersSource teachers, IKeyboardsFactory keyboards) : base("updleclist")
        {
            this.teachers = teachers;
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (base.CanHandleCommand(update))
                return (update.CallbackQuery.Data == "➡️" || update.CallbackQuery.Data == "⬅️") &&
                       update.CallbackQuery.Message.Text.Contains("препод");
            return false;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update)
        {
            if (update.CallbackQuery != null)
            {
                if (int.TryParse(update.CallbackQuery.Message.Text.Last().ToString(), out var index))
                {
                    var number = update.CallbackQuery.Data == "➡️" ? 1 : -1;
                    var current = number + index;
                    var replyList = teachers.GetTeachersNames().Skip(10 * current).Take(10).ToList();
                    var teachersButtonsCount = replyList.Count/2;
                    InlineKeyboardMarkup inlineKeyboard;
                    if (current > 0)
                    {
                        var endButtons = replyList.Count < 10 ? new[]
                        {
                            InlineKeyboardButton.WithCallbackData("⬅️")
                        } : new[]
                        {
                            InlineKeyboardButton.WithCallbackData("⬅️"),
                            InlineKeyboardButton.WithCallbackData("➡️")
                        };
                        var keyboard = new InlineKeyboardButton[teachersButtonsCount + 1][];
                        var keyboardCounter = 0;
                        for (int i = 0; i < keyboard.Length - 1; i++)
                        {
                            keyboard[i] = new[] { InlineKeyboardButton.WithCallbackData(replyList[keyboardCounter]), InlineKeyboardButton.WithCallbackData(replyList[keyboardCounter + 1]) };
                            keyboardCounter += 2;
                        }
                        keyboard[keyboard.Length - 1] = endButtons;
                        inlineKeyboard = new InlineKeyboardMarkup(keyboard);
                    }
                    else
                    {
                        var endButtons = new[]
                        {
                            InlineKeyboardButton.WithCallbackData("➡️")
                        };
                        var keyboard = new InlineKeyboardButton[teachersButtonsCount + 1][];
                        var keyboardCounter = 0;
                        for (int i = 0; i < keyboard.Length - 1; i++)
                        {
                            keyboard[i] = new[] { InlineKeyboardButton.WithCallbackData(replyList[keyboardCounter]), InlineKeyboardButton.WithCallbackData(replyList[keyboardCounter + 1]) };
                            keyboardCounter += 2;
                        }

                        keyboard[keyboard.Length - 1] = endButtons;
                        inlineKeyboard = new InlineKeyboardMarkup(keyboard);
                    }

                    //Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, update.CallbackQuery.Data);
                    await Bot.Client.EditMessageTextAsync(update.CallbackQuery.From.Id,
                        update.CallbackQuery.Message.MessageId,
                        "Список преподавателей " + current, replyMarkup: inlineKeyboard);
                }
            }


            return UpdateHandlingResult.Handled;
        }
    }
}