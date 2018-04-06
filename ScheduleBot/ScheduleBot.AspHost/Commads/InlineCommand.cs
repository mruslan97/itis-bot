using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads
{
    public abstract class InlineCommand : CommandBase<DefaultCommandArgs>
    {
        protected InlineCommand(string name) : base(name)
        {
        }

        public override bool CanHandleUpdate(IBot bot, Update update)
        {
            Bot = Bot ?? bot;
            return CanHandleCommand(update);
        }
        public override async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            Bot = Bot ?? bot;
            return await HandleCommand(update);
        }

        public override  Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            throw new NotImplementedException();
        }

        public abstract Task<UpdateHandlingResult> HandleCommand(Update update);

        protected override bool CanHandleCommand(Update update)
        {
            return update.CallbackQuery != null;
        }
    }
}