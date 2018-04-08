using System.Threading.Tasks;
using ScheduleBot.AspHost.Commads.CommandArgs;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.Commads
{
    public class TestCommand : CommandBase<DefaultCommandArgs>
    {
        public TestCommand() : base("itis")
        {
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DefaultCommandArgs args)
        {
            var answer = @"╔══╦════╦══╦═══╗
╚╣╠╣╔╗╔╗╠╣╠╣╔═╗║
░║║╚╝║║╚╝║║║╚══╗
░║║░░║║░░║║╚══╗║
╔╣╠╗░║║░╔╣╠╣╚═╝║
╚══╝░╚╝░╚══╩═══╝
" + "\n🔝🔝🔝🔝🔝🔝";
            await Bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                answer);

            return UpdateHandlingResult.Handled;
        }
    }
}