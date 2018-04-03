using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScheduleBot.AspHost.BotStorage
{
    public interface INotifiactionSender
    {
        Task SendNotificationsForIdsAsync(IEnumerable<long> ids);
    }
}