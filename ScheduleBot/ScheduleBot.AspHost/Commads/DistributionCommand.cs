using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Commads.CommandArgs;
using ScheduleBot.AspHost.Keyboards;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.AspHost.Commads
{
    public class DistributionCommand : CommandBase<DefaultCommandArgs>
    {
        private readonly INotifiactionSender notificator;
        private readonly IBotDataStorage storage;
        private readonly IKeyboardsFactory keyboards;
        private readonly SecretKey secret;
        private readonly ILogger<DistributionCommand> logger;
        public class SecretKey
        {
            public string Key { get; set; }
        }
        public DistributionCommand(INotifiactionSender notificator, IBotDataStorage storage, IKeyboardsFactory keyboards, SecretKey secret, ILogger<DistributionCommand> logger = null) : base("sendsudo")
        {
            this.notificator = notificator;
            this.storage = storage;
            this.keyboards = keyboards;
            this.secret = secret;
            this.logger = logger;
        }
        

        protected override bool CanHandleCommand(Update update)
        {
            if (!base.CanHandleCommand(update))
            {
                return update.Message.Text.StartsWith("sudo send");
            }
            else
                return true;
        }

        

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            try
            {
                try
                {
                    await Bot.Client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);

                }
                catch (Exception e)
                {
                    logger?.LogWarning(e, "Notificator cannot remove this message. {0}", update.Message.Text);
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id, $"I cannot to send it. Please check-out me, ensure it's supergroup chat and I have permissions to delete. Remove secret key above manually.");
                    return UpdateHandlingResult.Handled;
                }

                var indexOfKey = update.Message.Text.IndexOf("key=") + 4;
                var keyFromMessage = update.Message.Text.Substring(indexOfKey);
                var indexOfDivider = keyFromMessage.IndexOf(' ');
                keyFromMessage = keyFromMessage.Substring(0, indexOfDivider).Trim();
                if (!String.IsNullOrWhiteSpace(keyFromMessage) && !String.IsNullOrWhiteSpace(secret.Key) &&
                    keyFromMessage == secret.Key)
                {
                    var message = update.Message.Text.Substring(indexOfKey + secret.Key.Length + 1);
                    var userIds = storage.GetAllUsersChatIds();
                    logger?.LogInformation($"Message \"{message}\" will be sent to {userIds?.Count()} users.");
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id, $"Message \"{message}\" will be sent to {userIds?.Count()} users from {update.Message?.From?.Username}.");
                    await notificator.SendPureMessageForIdsAsync(userIds, message);
                }
                else
                {
                    await Bot.Client.SendTextMessageAsync(
                        update.Message.Chat.Id, $"Bad pass");
                    logger?.LogCritical("Failed attempt to send notification", JsonConvert.SerializeObject(update));
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Notificator fails, {0}", update?.Message?.Text);
            }
            return UpdateHandlingResult.Handled;
            
        }
    }
}
