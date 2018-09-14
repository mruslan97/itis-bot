﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Base class for the bot commands
    /// </summary>
    /// <typeparam name="TCommandArgs">Type of the command argument this command accepts</typeparam>
    public abstract class CommandBase<TCommandArgs> : ICommand<TCommandArgs>
        where TCommandArgs : ICommandArgs, new()
    {
        /// <summary>
        /// Command name without leading '/'
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Instance of the bot this command is operating for
        /// </summary>
        protected IBot Bot { get; set; }

        /// <summary>
        /// Initializes a new bot command with specified command name
        /// </summary>
        /// <param name="name">This command's name without leading '/'</param>
        protected CommandBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Indicates whether this handler should receive the update for handling by quickly checking the update type such as text, photo, or etc.
        /// </summary>
        /// <param name="bot">Instance of the bot this command is operating for</param>
        /// <param name="update">Update for the bot</param>
        /// <returns><value>true</value> if this handler should get the update; otherwise <value>false</value></returns>
        public virtual bool CanHandleUpdate(IBot bot, Update update)
        {
            Bot = Bot ?? bot;

            bool isTextMessage = new[] {update.Message?.Type, update.EditedMessage?.Type}
                .Contains(MessageType.Text);

            return isTextMessage && CanHandleCommand(update);
        }

        /// <summary>
        /// Handles the update for bot. This method will be called only if CanHandleUpdate returns <value>true</value>
        /// </summary>
        /// <param name="bot">Instance of the bot this command is operating for</param>
        /// <param name="update">The update to be handled</param>
        /// <returns>Result of handling this update</returns>
        public virtual async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            Bot = Bot ?? bot;
            var args = ParseInput(update);
            return await HandleCommand(update, args);
        }

        /// <summary>
        /// Parses the text input of update into this command's arguments
        /// </summary>
        /// <param name="update">Update to be parsed</param>
        /// <returns>Instance of this command's arguments</returns>
        protected virtual TCommandArgs ParseInput(Update update)
        {
            var args = new TCommandArgs
            {
                RawInput = update.Message.Text,
            };
            var argsInputMatch = Regex.Match(update.Message.Text,
                $@"^/{Name}(?:(?:@{Bot.UserName}(?:\s(?<args>.*))?)|\s(?<args>.*)|)$",
                RegexOptions.IgnoreCase); // ToDo use message entities instead
            if (argsInputMatch.Success)
            {
                args.ArgsInput = argsInputMatch.Groups[1].Value; // ToDo unit test
            }

            return args;
        }

        /// <summary>
        /// Indicates whether this command wants to handle the update by quickly checking the update type such as text, photo, or etc.
        /// </summary>
        /// <param name="update">The update to be handled</param>
        /// <returns><value>true</value> if this command should handle the update; otherwise <value>false</value></returns>
        protected virtual bool CanHandleCommand(Update update)
        {

            bool canHandle = false;
            try
            {
                var zippedMessageEntity = update.Message.Entities
                    .Zip(update.Message.EntityValues, (entity, val) => new
                    {
                        MessageEntity = entity,
                        Value = val
                    })
                    .SingleOrDefault(zipped =>
                        zipped.MessageEntity.Type == MessageEntityType.BotCommand &&
                        zipped.MessageEntity.Offset == 0
                    );

                if (zippedMessageEntity != null)
                {
                    canHandle = Regex.IsMatch(zippedMessageEntity.Value,
                        $@"^/{Name}(?:@{Bot.UserName})?$", RegexOptions.IgnoreCase);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAILED HANDLE MESSAGE, ex: {JsonConvert.SerializeObject(e)}");
            }

            return canHandle;
        }

        /// <summary>
        /// Handle the command update
        /// </summary>
        /// <param name="update">Command update to be handled</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Result of handling this update</returns>
        public abstract Task<UpdateHandlingResult> HandleCommand(Update update, TCommandArgs args);
    }
}