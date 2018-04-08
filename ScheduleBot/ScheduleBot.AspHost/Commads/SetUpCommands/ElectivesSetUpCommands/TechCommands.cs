using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.Commads.SetUpCommands.ElectivesSetUpCommands
{
    public class GetTechGroupsCommand : GetElectiveGroupsCommand
    {
        public GetTechGroupsCommand(IBotDataStorage storage, IScheduleService scheduler,
            IKeyboardsFactory keyboards) : base(ScheduleGroupType.PickedTech, "tech", "Выбери свой технологический трек.", "У тебя не нашлось технологического трека, прости. Хотя, может это к лучшему?",
            storage, scheduler, keyboards, "gettechs")
        {
        }
    }

    public class SetUpTechGroupCommand : SetUpElectiveGroupCommand
    {
        public SetUpTechGroupCommand(IBotDataStorage storage, IScheduleService scheduler,
            IKeyboardsFactory keyboards, ILogger<SetUpTechGroupCommand> logger = null) :
            base(ScheduleGroupType.PickedTech, storage, scheduler, keyboards, "settech", logger)
        {
        }
    }
}