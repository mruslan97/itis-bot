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

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
                return update.CallbackQuery != null;
            return false;
        }
    }
}