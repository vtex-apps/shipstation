using ShipStation.Models;
using System;
using System.Threading.Tasks;

namespace ShipStation.Services
{
    public interface IVtexAPIService
    {
        Task<bool> ProcessNotification(HookNotification hookNotification);
        Task<bool> ProcessNotification(AllStatesNotification allStatesNotification);
        Task<bool> CreateOrUpdateHook();
        Task<VtexOrder> GetOrderInformation(string orderId);
        Task<OrderInvoiceNotificationResponse> OrderInvoiceNotification(string orderId, OrderInvoiceNotificationRequest orderInvoice);
        Task<bool> SetOrderStatus(string orderId, string orderStatus);
        Task<bool> ProcessShipNotification(ListShipmentsResponse shipmentsResponse);
        Task<ListAllDocksResponse[]> ListAllDocks();
        Task<ListAllWarehousesResponse[]> ListAllWarehouses();
        Task<string> ValidateShipments(string date);
        Task<string> ValidateOrderShipments(string orderId);
        Task<string> CheckCancelledOrders(DateTime date);
    }
}