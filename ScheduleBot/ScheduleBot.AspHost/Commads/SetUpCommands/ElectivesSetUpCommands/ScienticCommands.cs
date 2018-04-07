using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleBot.AspHost.Keyboards;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.Commads.SetUpCommands.ElectivesSetUpCommands
{
    public class GetScienticGroupsCommand : GetElectiveGroupsCommand
    {
        public GetScienticGroupsCommand(IBotDataStorage storage, IScheduleService scheduler,
            IKeyboardsFactory keyboards) : base(ScheduleGroupType.PickedScientic, "sci", "Выбери свой научный трек.", "У тебя не нашлось научного трека, прости. Хотя, может это к лучшему?",
            storage, scheduler, keyboards, "getscis")
        {
        }
    }

    public class SetUpScienticGroupCommand : SetUpElectiveGroupCommand
    {
        public SetUpScienticGroupCommand(IBotDataStorage storage, IScheduleService scheduler,
            IKeyboardsFactory keyboards) :
            base(ScheduleGroupType.PickedScientic, storage, scheduler, keyboards, "setsci")
        {
        }
    }
}