using ShipStation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShipStation.Data
{
    public interface IShipStationRepository
    {
        Task<MerchantSettings> GetMerchantSettings();
    }
}