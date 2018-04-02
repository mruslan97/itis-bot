using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Helpers;
using ScheduleServices.Core;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleBot.AspHost.Commads.GetScheduleCommands
{
    public  abstract class AbstractGetForCommand : CommandBase<DefaultCommandArgs>
    {
        protected readonly IScheduleServise Scheduler;
        protected readonly IBotDataStorage Storage;

        public AbstractGetForCommand(string name, IScheduleServise scheduler, IBotDataStorage storage) : base(name: name)
        {
            this.Scheduler = scheduler;
            this.Storage = storage;
        }


        public abstract override Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args);

        protected async Task<UpdateHandlingResult> HandleCommandForPeriod(Update update, DefaultCommandArgs args, ScheduleRequiredFor period)
        {
            var userGroups = await Storage.GetGroupsForChatAsync(update.Message.Chat);
            if (userGroups != null)
            {
                //string answer = JsonConvert.SerializeObject(
                //    await Scheduler.GetScheduleForAsync(userGroups,
                //        period));

                var answer =
                    CustomSerializator.ProcessSchedule(await Scheduler.GetScheduleForAsync(userGroups, period));

                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    answer, ParseMode.Html);
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "А группа?");
            }


            return UpdateHandlingResult.Handled;
        }
    }
}