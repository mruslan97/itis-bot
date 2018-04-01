using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.BotStorage
{
    public interface IBotDataStorage
    {
        Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(long chatId);
        Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, long chatId);
    }
}