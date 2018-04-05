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
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Пожалуйста", replyMarkup: keyboards.GetKeyboardForCollection(teachers.GetTeachersNames(), t => "f:" + t));


            return UpdateHandlingResult.Handled;
        }
    }
}