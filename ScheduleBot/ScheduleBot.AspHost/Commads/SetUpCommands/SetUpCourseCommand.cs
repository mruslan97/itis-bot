using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotStorage;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public class SetUpCourseCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly KeyboardsFactory keyboards;

        public SetUpCourseCommand(KeyboardsFactory keyboards) : base("setupcourse")
        {
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("курс");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            string course = Regex.Match(args.RawInput,
                $@"[1-4]").Value;
            

            if (int.TryParse(course, out int courseNum))
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Выбери пожалуйста группу.", replyMarkup: keyboards.GetGroupsOfCourseKeyboard(courseNum));
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Нет такого курса - их всего 4...");
            }


            return UpdateHandlingResult.Handled;
        }
    }
}