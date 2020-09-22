using ShipStation.Models;
using System.Threading.Tasks;

namespace ShipStation.Services
{
    public interface IVtexAPIService
    {
        Task<bool> ProcessNotification(HookNotification hookNotification);
        Task<bool> CreateOrUpdateHook();
    }
}