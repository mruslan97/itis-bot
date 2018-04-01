using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleServices.Core.Models.Interfaces;
using Telegram.Bot.Types;

namespace ScheduleBot.AspHost.BotStorage
{
    public interface IBotDataStorage
    {
        Task<IEnumerable<IScheduleGroup>> GetGroupsForChatAsync(Chat chat);
        Task<bool> TryAddGroupToChatAsync(IScheduleGroup scheduleGroup, Chat chat);
    }
}