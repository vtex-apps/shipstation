using ShipStation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShipStation.Data
{
    public interface IShipStationRepository
    {
        Task<MerchantSettings> GetMerchantSettings();
        Task<List<ListStoresResponse>> GetShipStationStores();
        Task SaveShipStationStoreList(List<ListStoresResponse> listOfStores);
        Task<DateTime> GetLastShipmentCheck();
        Task SetLastShipmentCheck(DateTime lastCheck);
    }
}