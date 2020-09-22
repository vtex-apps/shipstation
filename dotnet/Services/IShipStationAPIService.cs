using ShipStation.Models;
using System.Threading.Tasks;

namespace ShipStation.Services
{
    public interface IShipStationAPIService
    {
        Task<bool> CreateUpdateOrder(VtexOrder vtexOrder);
    }
}