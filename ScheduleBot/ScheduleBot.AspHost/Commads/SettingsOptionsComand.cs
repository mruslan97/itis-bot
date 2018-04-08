using System;
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

    public class ForDevelopersCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly IKeyboardsFactory keyboards;

        public ForDevelopersCommand(IKeyboardsFactory keyboards) : base("devnull")
        {
            this.keyboards = keyboards;
        }

        Random Random = new Random(3);

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.ToLowerInvariant().Contains("разработчиков") ||
                       jokes.Any(x => update.Message.Text.Contains(x.Item3));
            }
            else
                return true;
        }

        //jokeid, messange, button, isserial, seriaid, id_in_seria
        private List<(int, string, string, bool, int, int)> jokes = new List<(int, string, string, bool, int, int)>()
        {
            (1, "r u sure?", ":$ sudo rm -rf /", false, 0, 0),
            (2, "почти", "попытаться", false, 0, 0),
            (3, "moloдой cheлоvек, proydemte", "черт, дудос полиция", false, 0, 0),
            (4, "саня это я, я в боте", "вернуть сотку", false, 0, 0),
            (5, "еще разок", "еще разок", false, 0, 0),
            (6, "press X to win", "X", false, 0, 0),
            (7, "111011111010001111001111111", "yep", false, 0, 0),
            (8, "заходит как то математик в бар...", "вы серьезно?", false, 0, 0),
            (9, "push me", "and then just touch me", true, 1, 0),
            (10, "till i can get my", "satisfaction", true, 1, 1),
            (11, "давай ты еще раз нажмешь?", "нажать", false, 0, 0),
            (12, "это же не машинное обучение, я телеграм бот", "обучить бота", false, 0, 0),
            (13, "найди уже себе кого нибудь и у него нажимай", "надавить на бота", false, 0, 0),
            (14, "больше всего тебе нравится видимо на кнопки жмакать", "жмакнуть", false, 0, 0),
            (15, "О-о-о-о!", "моя оборона", false, 0, 0),
            (16, "попробуй еще раз", "давай!", false, 0, 0),
            (17, "я не забуду касания твоих рук...", "❤️", true, 3, 0),
            (18, "трогай меня везде", "🍓", true, 3, 1),
            (19, "я сейчас за тобой приду", "ха ха", true, 2, 0),
            (20, "я уже вижу твою наглую морду", "врешь", true, 2, 1),
            (21, "обернись", ".....", true, 2, 2),
            (22, "ну и пожалуйста, ну и не нужно", "пф", true, 2, 3),
            (23, "2 юнит теста", "0 интеграционных", false, 0, 0),
            (24, "Сказано же, для РАЗ-РА-БОТ-ЧИ-КОВ", "но... я...", false, 0, 0)
        };

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            ValueTuple<int, string, string, bool, int, int> next;
            var lastjoke = jokes.FirstOrDefault(x => update.Message.Text.Contains(x.Item3));
            if (update.Message.Text.ToLowerInvariant().Contains("разработчиков"))
            {
                next = jokes[23];
                goto NEWFACE;
            }


            if (lastjoke.Item4 && jokes.Any(j => j.Item5 == lastjoke.Item5 && j.Item6 == lastjoke.Item6 + 1))
            {
                next = jokes.FirstOrDefault(j => j.Item5 == lastjoke.Item5 && j.Item6 == lastjoke.Item6 + 1);
            }
            else
            {
                var nextId = Random.Next(jokes.Count) + 1;
                while (lastjoke.Item1 == nextId || nextId == 24)
                    nextId = Random.Next(jokes.Count) + 1;
                next = jokes[nextId - 1];
                if (next.Item4)
                {
                    nextId = Random.Next(jokes.Count) + 1;
                    while (lastjoke.Item1 == nextId || nextId == 24)
                        nextId = Random.Next(jokes.Count) + 1;
                    next = jokes[nextId - 1];
                    if (next.Item4)
                        next = jokes.FirstOrDefault(j => j.Item4 && j.Item5 == next.Item5 && j.Item6 == 0);
                }
            }

            if (next.Item1 == 1)
                next.Item3 = update.Message.Chat.Username + next.Item3;
            NEWFACE:
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id, next.Item2, replyMarkup: new ReplyKeyboardMarkup(new[]
                {
                    new[] {new KeyboardButton(next.Item3)},
                    new[] {new KeyboardButton("Back⬅️") }
                }));

            return UpdateHandlingResult.Handled;
        }
    }
}