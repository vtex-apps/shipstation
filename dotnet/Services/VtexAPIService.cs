using ShipStation.Data;
using ShipStation.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using System.Linq;

namespace ShipStation.Services
{
    public class VtexAPIService : IVtexAPIService
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IShipStationRepository _shipStationRepository;
        private readonly IShipStationAPIService _shipStationAPIService;        
        private readonly string _applicationName;

        public VtexAPIService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IShipStationRepository shipStationRepository, IShipStationAPIService shipStationAPIService)
        {
            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._shipStationRepository = shipStationRepository ??
                               throw new ArgumentNullException(nameof(shipStationRepository));

            this._shipStationAPIService = shipStationAPIService ??
                               throw new ArgumentNullException(nameof(shipStationAPIService));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<bool> CreateOrUpdateHook()
        {
            // POST https://{accountName}.{environment}.com.br/api/orders/hook/config
            string baseUrl = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.FORWARDED_HOST];

            HookNotification createOrUpdateHookResponse = new HookNotification();
            OrderHook orderHook = new OrderHook
            {
                Filter = new Filter
                {
                    Status = new List<string>
                    {
                        ShipStationConstants.VtexOrderStatus.ReadyForHandling,
                        //ShipStationConstants.VtexOrderStatus.ApprovePayment,
                        //ShipStationConstants.VtexOrderStatus.AuthorizeFullfilment,
                        //ShipStationConstants.VtexOrderStatus.Cancel,
                        //ShipStationConstants.VtexOrderStatus.Canceled,
                        //ShipStationConstants.VtexOrderStatus.CancellationRequested,
                        //ShipStationConstants.VtexOrderStatus.Handling,
                        //ShipStationConstants.VtexOrderStatus.Invoice,
                        //ShipStationConstants.VtexOrderStatus.InvoiceAfterCancellationDeny,
                        //ShipStationConstants.VtexOrderStatus.Invoiced,
                        //ShipStationConstants.VtexOrderStatus.OnOrderCompleted,
                        //ShipStationConstants.VtexOrderStatus.OrderAccepted,
                        //ShipStationConstants.VtexOrderStatus.OrderCompleted,
                        //ShipStationConstants.VtexOrderStatus.OrderCreated,
                        //ShipStationConstants.VtexOrderStatus.OrderCreateError,
                        //ShipStationConstants.VtexOrderStatus.OrderCreationError,
                        //ShipStationConstants.VtexOrderStatus.PaymentApproved,
                        //ShipStationConstants.VtexOrderStatus.PaymentDenied,
                        //ShipStationConstants.VtexOrderStatus.PaymentPending,
                        //ShipStationConstants.VtexOrderStatus.Replaced,
                        //ShipStationConstants.VtexOrderStatus.RequestCancel,
                        //ShipStationConstants.VtexOrderStatus.StartHanding,
                        //ShipStationConstants.VtexOrderStatus.WaitingForOrderAuthorization,
                        //ShipStationConstants.VtexOrderStatus.WaitingForSellerDecision,
                        //ShipStationConstants.VtexOrderStatus.WindowToCancel
                    }
                },
                Hook = new Hook
                {
                    Headers = new Headers
                    {
                        Key = ShipStationConstants.ENDPOINT_KEY
                    },
                    Url = new Uri($"https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}")
                }
            };

            var jsonSerializedOrderHook = JsonConvert.SerializeObject(orderHook);
            //Console.WriteLine($"Hook = {jsonSerializedOrderHook}");
            Console.WriteLine($"Url = https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/orders/hook/config"),
                Content = new StringContent(jsonSerializedOrderHook, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[-] CreateOrUpdateHook Response {response.StatusCode} Content = '{responseContent}' [-]");
            _context.Vtex.Logger.Info("CreateOrUpdateHook", "Response", $"[{response.StatusCode}] {responseContent}");
            //if (response.IsSuccessStatusCode)
            //{
            //    createOrUpdateHookResponse = JsonConvert.DeserializeObject<HookNotification>(responseContent);
            //}

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> VerifyHook()
        {
            bool verified = false;
            // POST https://{accountName}.{environment}.com.br/api/orders/hook/config
            string baseUrl = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.FORWARDED_HOST];

            OrderHook verifyHookResponse = new OrderHook();
            OrderHook orderHook = new OrderHook
            {
                Filter = new Filter
                {
                    Status = new List<string>
                    {
                        ShipStationConstants.VtexOrderStatus.ReadyForHandling,
                    }
                },
                Hook = new Hook
                {
                    Headers = new Headers
                    {
                        Key = ShipStationConstants.ENDPOINT_KEY
                    },
                    Url = new Uri($"https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}")
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/orders/hook/config")
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            _context.Vtex.Logger.Info("VerifyHook", "Response", $"[{response.StatusCode}] {responseContent}");
            if (response.IsSuccessStatusCode)
            {
                verifyHookResponse = JsonConvert.DeserializeObject<OrderHook>(responseContent);
                if (verifyHookResponse.Hook.Url.Equals($"https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}")
                    && verifyHookResponse.Hook.Headers.Key.Equals(ShipStationConstants.ENDPOINT_KEY))
                {
                    verified = true;
                }
            }

            return verified;
        }

        public async Task<VtexOrder> GetOrderInformation(string orderId)
        {
            //Console.WriteLine("------- Headers -------");
            //foreach (var header in this._httpContextAccessor.HttpContext.Request.Headers)
            //{
            //    Console.WriteLine($"{header.Key}: {header.Value}");
            //}
            //Console.WriteLine($"http://{this._httpContextAccessor.HttpContext.Request.Headers[Constants.VTEX_ACCOUNT_HEADER_NAME]}.{Constants.ENVIRONMENT}.com.br/api/checkout/pvt/orders/{orderId}");

            VtexOrder vtexOrder = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/checkout/pvt/orders/{orderId}")
                };

                request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
                //request.Headers.Add(Constants.ACCEPT, Constants.APPLICATION_JSON);
                //request.Headers.Add(Constants.CONTENT_TYPE, Constants.APPLICATION_JSON);
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
                //Console.WriteLine($"Token = '{authToken}'");
                if (authToken != null)
                {
                    request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(ShipStationConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                //StringBuilder sb = new StringBuilder();

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    vtexOrder = JsonConvert.DeserializeObject<VtexOrder>(responseContent);
                    Console.WriteLine($"GetOrderInformation: [{response.StatusCode}] ");
                }
                else
                {
                    Console.WriteLine($"GetOrderInformation: [{response.StatusCode}] '{responseContent}'");
                    _context.Vtex.Logger.Info("GetOrderInformation", null, $"Order# {orderId} [{response.StatusCode}] '{responseContent}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrderInformation Error: {ex.Message}");
                _context.Vtex.Logger.Error("GetOrderInformation", null, $"Order# {orderId} Error", ex);
            }

            return vtexOrder;
        }

        public async Task<bool> ProcessNotification(HookNotification hookNotification)
        {
            bool success = true;
            VtexOrder vtexOrder = null;

            switch (hookNotification.Domain)
            {
                case ShipStationConstants.Domain.Fulfillment:
                    //VtexOrder vtexOrder = await this.GetOrderInformation(hookNotification.OrderId);
                    //success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                    switch (hookNotification.State)
                    {
                        case ShipStationConstants.VtexOrderStatus.ReadyForHandling:
                            vtexOrder = await this.GetOrderInformation(hookNotification.OrderId);
                            success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                            if(success)
                            {
                                success = await this.SetOrderStatus(hookNotification.OrderId, ShipStationConstants.VtexOrderStatus.StartHanding);
                            }
                            break;
                        //case ShipStationConstants.VtexOrderStatus.ApprovePayment:
                        case ShipStationConstants.VtexOrderStatus.Cancel:
                        //case ShipStationConstants.VtexOrderStatus.Handling:
                        //case ShipStationConstants.VtexOrderStatus.Invoice:
                        case ShipStationConstants.VtexOrderStatus.Invoiced:
                        case ShipStationConstants.VtexOrderStatus.OnOrderCompleted:
                        case ShipStationConstants.VtexOrderStatus.OrderCreated:
                        case ShipStationConstants.VtexOrderStatus.PaymentPending:
                            vtexOrder = await this.GetOrderInformation(hookNotification.OrderId);
                            success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                            break;
                        default:
                            Console.WriteLine($"State {hookNotification.State} not implemeted.");
                            _context.Vtex.Logger.Info("ProcessNotification", null, $"State {hookNotification.State} not implemeted.");
                            break;
                    }
                    break;
                case ShipStationConstants.Domain.Marketplace:
                    Console.WriteLine($"Marketplace not implemeted.");
                    _context.Vtex.Logger.Info("ProcessNotification", null, $"Marketplace not implemeted.");
                    break;
                default:
                    Console.WriteLine($"Domain {hookNotification.Domain} not implemeted.");
                    _context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {hookNotification.Domain} not implemeted.");
                    break;
            }

            return success;
        }

        /// <summary>
        /// Once the order is invoiced, the seller should use this request to send the invoice information to the marketplace.
        /// We strongly recommend that you always send the object of the invoiced items.With this practice, rounding errors will be avoided.
        /// It is not allowed to use the same invoiceNumber in more than one request to the Order Invoice Notification endpoint.
        /// Be aware that this endpoint is also used by the seller to send the order tracking information.This, however, should be done in a separate moment, once the seller has the tracking information.
        /// https://{accountName}.{environment}.com.br/api/oms/pvt/orders/orderId/invoice
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderInvoice"></param>
        /// <returns>OrderInvoiceNotificationResponse</returns>
        public async Task<OrderInvoiceNotificationResponse> OrderInvoiceNotification(string orderId, OrderInvoiceNotificationRequest orderInvoice)
        {
            OrderInvoiceNotificationResponse orderInvoiceNotificationResponse = null;
            string jsonSerializedInvoice = JsonConvert.SerializeObject(orderInvoice);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/oms/pvt/orders/{orderId}/invoice"), // RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexinternal.com.br/api/oms/pvt/orders/{orderId}/invoice"),
                Content = new StringContent(jsonSerializedInvoice, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"OrderInvoiceNotification [{response.StatusCode}] {responseContent}");
            if (response.IsSuccessStatusCode)
            {
                orderInvoiceNotificationResponse = JsonConvert.DeserializeObject<OrderInvoiceNotificationResponse>(responseContent);
            }

            return orderInvoiceNotificationResponse;
        }

        public async Task<bool> SetOrderStatus(string orderId, string orderStatus)
        {
            bool success = false;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/oms/pvt/orders/{orderId}/{orderStatus}")
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"SetOrderStatus [{response.StatusCode}] {responseContent}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ProcessShipNotification(ListShipmentsResponse shipmentsResponse)
        {
            bool success = false;
            StringBuilder sb = new StringBuilder();

            foreach(Shipment shipment in shipmentsResponse.Shipments)
            {
                string orderId = shipment.OrderNumber;
                Console.WriteLine($"Processing Shipment for Order#{orderId} [{shipment.CarrierCode}] {shipment.TrackingNumber}");
                sb.AppendLine($"Processing Shipment for Order#{orderId} [{shipment.CarrierCode}] {shipment.TrackingNumber}");
                if (shipment.ShipmentItems != null)
                {
                    VtexOrder vtexOrder = await this.GetOrderInformation(orderId);
                    long orderTotal = vtexOrder.Totals.Sum(t => t.Value);
                    long shippingTotal = vtexOrder.Totals.Where(t => t.Name == "Shipping").Select(d => d.Value).FirstOrDefault();
                    long taxTotal = vtexOrder.Totals.Where(t => t.Name == "Tax").Select(d => d.Value).FirstOrDefault();

                    OrderInvoiceNotificationRequest request = new OrderInvoiceNotificationRequest
                    {
                        Courier = shipment.CarrierCode,
                        TrackingNumber = shipment.TrackingNumber,
                        Type = ShipStationConstants.InvoiceType.OUTPUT,
                        InvoiceNumber = shipment.ShipmentId.ToString(),
                        InvoiceValue = 0,
                        IssuanceDate = shipment.ShipDate.ToString(),
                        Items = new List<InvoiceItem>(),
                        TrackingUrl = null
                    };

                    foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
                    {
                        string sku = shipmentItem.Sku;
                        Console.WriteLine($"Item '{shipmentItem.Name}' {sku} ({shipmentItem.Quantity}) ${shipmentItem.UnitPrice}");
                        sb.AppendLine($"    Item '{shipmentItem.Name}' {sku} ({shipmentItem.Quantity}) ${shipmentItem.UnitPrice}");

                        LogisticsInfo logisticsInfo = vtexOrder.ShippingData.LogisticsInfo.Where(l => l.ItemId.Equals(sku)).FirstOrDefault();
                        Sla sla = logisticsInfo.Slas.Where(s => s.Id.Equals(logisticsInfo.SelectedSla)).FirstOrDefault();

                        long itemPrice = ToCents(shipmentItem.UnitPrice);
                        long itemTax = sla.Tax;

                        InvoiceItem invoiceItem = new InvoiceItem
                        {
                            Id = shipmentItem.Sku,
                            Price = itemPrice,
                            Quantity = shipmentItem.Quantity
                        };

                        request.Items.Add(invoiceItem);
                        request.InvoiceValue += (itemPrice + itemTax);
                        Console.WriteLine($"request.InvoiceValue = {request.InvoiceValue}");
                    }

                    // Don't charge more than order total
                    request.InvoiceValue = Math.Min(request.InvoiceValue, orderTotal);

                    OrderInvoiceNotificationResponse response = await this.OrderInvoiceNotification(orderId, request);
                    success = response != null;
                }
            }

            _context.Vtex.Logger.Info("ProcessShipNotification", null, sb.ToString());

            return success;
        }

        public async Task<ListAllDocksResponse> ListAllDocks()
        {
            ListAllDocksResponse listAllDocksResponse = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/logistics/pvt/configuration/docks")
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ListAllDocks [{response.StatusCode}] {responseContent}");
            if (response.IsSuccessStatusCode)
            {
                listAllDocksResponse = JsonConvert.DeserializeObject<ListAllDocksResponse>(responseContent);
            }

            return listAllDocksResponse;
        }

        public async Task<ListAllWarehousesResponse> ListAllWarehouses()
        {
            ListAllWarehousesResponse listAllWarehousesResponse = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/logistics/pvt/configuration/docks")
            };

            request.Headers.Add(ShipStationConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(ShipStationConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(ShipStationConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(ShipStationConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(ShipStationConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ListAllDocks [{response.StatusCode}] {responseContent}");
            if (response.IsSuccessStatusCode)
            {
                listAllWarehousesResponse = JsonConvert.DeserializeObject<ListAllWarehousesResponse>(responseContent);
            }

            return listAllWarehousesResponse;
        }

        private long ToCents(double asDollars)
        {
            return (long)asDollars * 100;
        }
    }
}
