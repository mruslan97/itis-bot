using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScheduleBot.AspHost.BotServices.Interfaces
{
    public interface INotifiactionSender
    {
        Task SendNotificationsForIdsAsync(IEnumerable<long> ids, string message);
    }
}