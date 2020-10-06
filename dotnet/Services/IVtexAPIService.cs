using ShipStation.Models;
using System.Threading.Tasks;

namespace ShipStation.Services
{
    public interface IVtexAPIService
    {
        Task<bool> ProcessNotification(HookNotification hookNotification);
        Task<bool> CreateOrUpdateHook();
        Task<VtexOrder> GetOrderInformation(string orderId);
        Task<OrderInvoiceNotificationResponse> OrderInvoiceNotification(string orderId, OrderInvoiceNotificationRequest orderInvoice);
        Task<bool> SetOrderStatus(string orderId, string orderStatus);
        Task<bool> ProcessShipNotification(ListShipmentsResponse shipmentsResponse);
        Task<ListAllDocksResponse> ListAllDocks();
        Task<ListAllWarehousesResponse> ListAllWarehouses();
    }
}