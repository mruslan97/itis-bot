using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleServices.Core;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.GetScheduleCommands
{
    public class GetForWeekCommand : AbstractGetForCommand
    {

        public GetForWeekCommand(IScheduleServise scheduler, IBotDataStorage storage) : base(name: "week", storage: storage, scheduler: scheduler)
        {
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("недел");
            }
            else
                return true;
        }

        public override Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            return HandleCommandForPeriod(update, args, ScheduleRequiredFor.Week);
        }
    }
}