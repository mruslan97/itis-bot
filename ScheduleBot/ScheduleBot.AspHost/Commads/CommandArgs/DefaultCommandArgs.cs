using Telegram.Bot.Framework.Abstractions;

namespace ScheduleBot.AspHost.Commads.CommandArgs
{
    public class DefaultCommandArgs : ICommandArgs
    {
        
            public string RawInput { get; set; }

            public string ArgsInput { get; set; }
        
    }
}