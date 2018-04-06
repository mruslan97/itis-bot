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
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Туда")
                }
                
            });
            var reply = teachers.GetTeachersNames().Take(10).Aggregate((acc, teacher) => acc + "\n" + "/f:" + teacher) + "\n0";
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Список преподавателей: \n" + reply, replyMarkup: inlineKeyboard);


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
                return (update.CallbackQuery.Data == "Туда" || update.CallbackQuery.Data == "Сюда") && update.CallbackQuery.Message.Text.Contains("препод");
            return false;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            if (update.CallbackQuery != null)
            {
               
                if (int.TryParse(update.CallbackQuery.Message.Text.Last().ToString(), out var index))
                {
                   
                    var number = update.CallbackQuery.Data == "Туда" ? 1 : -1;
                    var current = number + index;
                    var reply = teachers.GetTeachersNames().Skip(10 * current).Take(10).Aggregate((acc, teacher) => acc + "\n" + "/f:" + teacher) + "\n" + current;
                    InlineKeyboardMarkup inlineKeyboard;
                    if (current > 0)
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Туда")
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Сюда")
                            }
                        });
                    }
                    else
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Туда")
                            }
                        });
                    }
                    //Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, update.CallbackQuery.Data);
                    await Bot.Client.EditMessageTextAsync(update.CallbackQuery.From.Id, update.CallbackQuery.Message.MessageId,
                        reply, replyMarkup: inlineKeyboard);
                }
                
            }


            return UpdateHandlingResult.Handled;
        }
    }
}