using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;

namespace ScheduleBot.AspHost.BotStorage
{
    public interface IBotDataStorage
    {
        Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(long chatId);
        Task AddGroupToChat(IScheduleGroup scheduleGroup, long chatId);
    }
}