using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.BotStorage
{
    public interface IBotDataStorage
    {
        Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(long chatId);
        bool TryAddGroupToChat(IScheduleGroup scheduleGroup, long chatId);
    }
}