using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public class ChangeAcademicGroupCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly KeyboardsFactory keyboards;

        public ChangeAcademicGroupCommand(KeyboardsFactory keyboards) : base(name: "changecourse")
        {
            this.keyboards = keyboards;
        }
        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("сменить группу");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                $"Выбери курс:", replyMarkup: keyboards.GetCoursesKeyboad());

            return UpdateHandlingResult.Handled;
        }
    }
}
