﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Commads
{
    public class SettingsOptionsCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IKeyboardsFactory keyboards;

        public SettingsOptionsCommand(IKeyboardsFactory keyboards) : base("getsettings")
        {
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("настройки");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var answer = "Меню настроек. 🛠️\n" +
                         "<b>Eng</b> - выбор преподавателя по английскому языку\n" +
                         "<b>Tech</b> - курс по выбору(лабы)\n" +
                         "<b>Science</b> - курс по выбору, научный блок.(физика, машинное обучение) \n" +
                         "<b>Сменить группу</b> - смена академической группы\n" +
                         @"<b>Для разработчиков</b> - без комментариев ¯\_(ツ)_/¯";
                             
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                answer, replyMarkup: keyboards.GetSettingsKeyboard(), parseMode:ParseMode.Html);


            return UpdateHandlingResult.Handled;
        }
    }

    public class SettingsBackCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IKeyboardsFactory keyboards;

        public SettingsBackCommand(IKeyboardsFactory keyboards) : base("backtomain")
        {
            this.keyboards = keyboards;
        }

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("back");
            }
            else
                return true;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id, "Главное меню", replyMarkup: keyboards.GetMainOptionsKeyboard());


            return UpdateHandlingResult.Handled;
        }
    }

    
}