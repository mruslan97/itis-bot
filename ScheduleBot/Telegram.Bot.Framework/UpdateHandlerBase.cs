﻿using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Base class for Telegram bot update handlers
    /// </summary>
    public abstract class UpdateHandlerBase : IUpdateHandler
    {
        /// <summary>
        /// Indicates whether this handler should receive the update for handling by quickly checking the update type such as text, photo, or etc.
        /// </summary>
        /// <param name="bot">Instance of the bot this command is operating for</param>
        /// <param name="update">Update for the bot</param>
        /// <returns><value>true</value> if this handler should get the update; otherwise <value>false</value></returns>
        public abstract bool CanHandleUpdate(IBot bot, Update update);

        /// <summary>
        /// Handles the update for bot. This method will be called only if CanHandleUpdate returns <value>true</value>
        /// </summary>
        /// <param name="bot">Instance of the bot this command is operating for</param>
        /// <param name="update">The update to be handled</param>
        /// <returns>Result of handling this update</returns>
        public abstract Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update);
    }
}
