using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotServices;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads.SetUpCommands
{
    public abstract class SetUpGroupCommand : CommandBase<DefaultCommandArgs>
    {
        protected IScheduleService Scheduler;
        protected IKeyboardsFactory Keyboards;
        protected IBotDataStorage Storage;

        public SetUpGroupCommand(IBotDataStorage storage, IScheduleService scheduler, IKeyboardsFactory keyboards,
            string command) : base(command)
        {
            this.Storage = storage;
            this.Scheduler = scheduler;
            this.Keyboards = keyboards;
        }


        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            string groupName;
            try
            {
                groupName = args.RawInput.Trim();
            }
            catch (Exception e)
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Нет такой группы :(");
                return UpdateHandlingResult.Handled;
            }

            if (Scheduler.GroupsMonitor.TryFindGroupByName(groupName, out IScheduleGroup group)
                && await Storage.TryAddGroupToChatAsync(group, update.Message.Chat))
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Установлено!", replyMarkup: Keyboards.GetMainOptionsKeyboard());
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Нет такой группы :(");
            }


            return UpdateHandlingResult.Handled;
        }
    }

    public class SetUpAcademicGroupCommand : SetUpGroupCommand
    {
        public SetUpAcademicGroupCommand(IBotDataStorage storage, IScheduleService scheduler,
            IKeyboardsFactory keyboards) : base(storage, scheduler, keyboards, "setgroup")
        {
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().StartsWith("11-");
            }
            else
                return true;
        }
    }

    public class NotFoundGroupCommand : SetUpGroupCommand
    {
        public NotFoundGroupCommand(IBotDataStorage storage, IScheduleService scheduler,
            IKeyboardsFactory keyboards) : base(storage, scheduler, keyboards, "notfound")
        {
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("ничего нет");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Прости...", replyMarkup: Keyboards.GetMainOptionsKeyboard());


            return UpdateHandlingResult.Handled;
        }
    }
}