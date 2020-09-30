﻿using ShipStation.Models;
using System.Threading.Tasks;

namespace ShipStation.Services
{
    public interface IShipStationAPIService
    {
        Task<bool> CreateUpdateOrder(VtexOrder vtexOrder);
        Task<string> SubscribeToWebhook(string hookEvent);
        Task<string> ProcessResourceUrl(string resourceUrl);
        Task<ListOrdersResponse> ListOrders(string queryPrameters);
        Task<ListShipmentsResponse> ListShipments(string queryPrameters);
        Task<ListFulfillmentsResponse> ListFulFillments(string queryPrameters);
        Task<CreateWarehouseResponse> CreateWarehouse(CreateWarehouseRequest createWarehouseRequest);
    }
}