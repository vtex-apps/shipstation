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
            //Console.WriteLine($"Url = https://{baseUrl}/{ShipStationConstants.APP_NAME}/{ShipStationConstants.ENDPOINT_KEY}");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/orders/hook/config"),
                Content = new StringContent(jsonSerializedOrderHook, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
            };

            Console.WriteLine($"request url = {request.RequestUri}");
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
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.{ShipStationConstants.ENVIRONMENT}.com.br/api/oms/pvt/orders/{orderId}")
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
                            if (vtexOrder != null)
                            {
                                MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
                                if (!merchantSettings.MarketplaceOnly ||
                                    (merchantSettings.MarketplaceOnly && vtexOrder.Origin != null && vtexOrder.Origin.Equals(ShipStationConstants.Domain.Marketplace)))
                                {
                                    success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                                    if (success && merchantSettings.UpdateOrderStatus)
                                    {
                                        //success = await this.SetOrderStatus(allStatesNotification.OrderId, ShipStationConstants.VtexOrderStatus.StartHanding);
                                        bool response = await this.SetOrderStatus(hookNotification.OrderId, ShipStationConstants.VtexOrderStatus.StartHanding);
                                        if (!response)
                                        {
                                            _context.Vtex.Logger.Info("ProcessNotification", null, $"Failed to set Status to start-handling for order {hookNotification.OrderId}.");
                                            //Console.WriteLine($"SetOrderStatus [{response}]");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                success = false;
                            }

                            break;
                        //case ShipStationConstants.VtexOrderStatus.ApprovePayment:
                        case ShipStationConstants.VtexOrderStatus.Cancel:
                            //case ShipStationConstants.VtexOrderStatus.Handling:
                            //case ShipStationConstants.VtexOrderStatus.Invoice:
                            //case ShipStationConstants.VtexOrderStatus.Invoiced:
                            //case ShipStationConstants.VtexOrderStatus.OnOrderCompleted:
                            //case ShipStationConstants.VtexOrderStatus.OrderCreated:
                            //case ShipStationConstants.VtexOrderStatus.PaymentPending:
                            vtexOrder = await this.GetOrderInformation(hookNotification.OrderId);
                            //if (vtexOrder != null && vtexOrder.Origin != null && vtexOrder.Origin.Equals(ShipStationConstants.Domain.Marketplace))
                            {
                                success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                            }

                            break;
                        default:
                            //Console.WriteLine($"State {hookNotification.State} not implemeted.");
                            //_context.Vtex.Logger.Info("ProcessNotification", null, $"State {hookNotification.State} not implemeted.");
                            break;
                    }
                    break;
                case ShipStationConstants.Domain.Marketplace:
                    //Console.WriteLine($"Marketplace not implemeted.");
                    //_context.Vtex.Logger.Info("ProcessNotification", null, $"Marketplace not implemeted.");
                    break;
                default:
                    //Console.WriteLine($"Domain {hookNotification.Domain} not implemeted.");
                    //_context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {hookNotification.Domain} not implemeted.");
                    break;
            }

            return success;
        }

        public async Task<bool> ProcessNotification(AllStatesNotification allStatesNotification)
        {
            bool success = true;
            VtexOrder vtexOrder = null;

            switch (allStatesNotification.Domain)
            {
                case ShipStationConstants.Domain.Fulfillment:
                    //VtexOrder vtexOrder = await this.GetOrderInformation(hookNotification.OrderId);
                    //success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                    switch (allStatesNotification.CurrentState)
                    {
                        case ShipStationConstants.VtexOrderStatus.ReadyForHandling:
                            vtexOrder = await this.GetOrderInformation(allStatesNotification.OrderId);
                            if (vtexOrder != null)
                            {
                                MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
                                if (string.IsNullOrEmpty(merchantSettings.ApiKey) || string.IsNullOrEmpty(merchantSettings.ApiSecret))
                                {
                                    _context.Vtex.Logger.Info("ProcessNotification", null, "Missing Credentials.");
                                }
                                else
                                {
                                    if (!merchantSettings.MarketplaceOnly ||
                                        (merchantSettings.MarketplaceOnly && vtexOrder.Origin != null && vtexOrder.Origin.Equals(ShipStationConstants.Domain.Marketplace)))
                                    {
                                        success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                                        Console.WriteLine($"CreateUpdateOrder returned {success} for order '{allStatesNotification.OrderId}'");
                                        _context.Vtex.Logger.Info("ProcessNotification", null, $"CreateUpdateOrder returned {success} for order '{allStatesNotification.OrderId}'");
                                        if (success && merchantSettings.UpdateOrderStatus)
                                        {
                                            //success = await this.SetOrderStatus(allStatesNotification.OrderId, ShipStationConstants.VtexOrderStatus.StartHanding);
                                            bool response = false;
                                            try
                                            {
                                                response = await this.SetOrderStatus(allStatesNotification.OrderId, ShipStationConstants.VtexOrderStatus.StartHanding);
                                            }
                                            catch (Exception ex)
                                            {
                                                _context.Vtex.Logger.Error("ProcessNotification", "SetOrderStatus", $"Error setting status to start-handling for order {allStatesNotification.OrderId}", ex);
                                            }

                                            _context.Vtex.Logger.Info("ProcessNotification", null, $"Set Status to start-handling for order {allStatesNotification.OrderId} = {response}.");
                                            if (!response)
                                            {
                                                _context.Vtex.Logger.Info("ProcessNotification", null, $"Failed to set Status to start-handling for order {allStatesNotification.OrderId}.");
                                                //Console.WriteLine($"SetOrderStatus [{response}]");
                                            }
                                        }
                                        else
                                        {
                                            _context.Vtex.Logger.Info("ProcessNotification", null, $"Order {allStatesNotification.OrderId} CreateUpdateOrder={success} Settings 'UpdateOrderStatus'={merchantSettings.UpdateOrderStatus}.");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                success = false;
                                Console.WriteLine($"GetOrderInformation returned Null for order '{allStatesNotification.OrderId}'");
                            }

                            try
                            {
                                // Check for any orders that were cancelled in ShipStation
                                await CheckCancelledOrders();
                            }
                            catch (Exception ex)
                            {
                                _context.Vtex.Logger.Error("ProcessNotification", "CheckCancelledOrders", $"Error checking for Cancelled orders", ex);
                            }

                            break;
                        //case ShipStationConstants.VtexOrderStatus.ApprovePayment:
                        case ShipStationConstants.VtexOrderStatus.Cancel:
                            //case ShipStationConstants.VtexOrderStatus.Handling:
                            //case ShipStationConstants.VtexOrderStatus.Invoice:
                            //case ShipStationConstants.VtexOrderStatus.Invoiced:
                            //case ShipStationConstants.VtexOrderStatus.OnOrderCompleted:
                            //case ShipStationConstants.VtexOrderStatus.OrderCreated:
                            //case ShipStationConstants.VtexOrderStatus.PaymentPending:
                            vtexOrder = await this.GetOrderInformation(allStatesNotification.OrderId);
                            //if (vtexOrder != null && vtexOrder.Origin != null && vtexOrder.Origin.Equals(ShipStationConstants.Domain.Marketplace))
                            {
                                success = await this._shipStationAPIService.CreateUpdateOrder(vtexOrder);
                            }

                            break;
                        default:
                            Console.WriteLine($"State {allStatesNotification.CurrentState} not implemeted.");
                            //_context.Vtex.Logger.Info("ProcessNotification", null, $"State {hookNotification.State} not implemeted.");
                            break;
                    }
                    break;
                case ShipStationConstants.Domain.Marketplace:
                    Console.WriteLine($"Marketplace not implemeted.");
                    //_context.Vtex.Logger.Info("ProcessNotification", null, $"Marketplace not implemeted.");
                    break;
                default:
                    Console.WriteLine($"Domain {allStatesNotification.Domain} not implemeted.");
                    //_context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {hookNotification.Domain} not implemeted.");
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
            //Console.WriteLine("SetOrderStatus DISABLED!");
            //return true;

            bool success = false;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/oms/pvt/orders/{orderId}/{orderStatus}")
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

            Console.WriteLine($"SetOrderStatus to {orderStatus} for Order '{orderId}' [{response.StatusCode}] {responseContent}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ProcessShipNotification(ListShipmentsResponse shipmentsResponse)
        {
            bool success = false;
            StringBuilder sb = new StringBuilder();
            MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            string orderId = string.Empty;

            foreach (Shipment shipment in shipmentsResponse.Shipments)
            {
                if (merchantSettings.UseSequenceAsOrderNumber)
                {
                    orderId = shipment.OrderKey;
                }
                else
                {
                    orderId = shipment.OrderNumber;
                }

                Console.WriteLine($"Processing Shipment for Order#{orderId} [{shipment.CarrierCode}] {shipment.TrackingNumber}");
                sb.AppendLine($"Processing Shipment for Order#{orderId} [{shipment.CarrierCode}] {shipment.TrackingNumber}");
                if (shipment.ShipmentItems != null)
                {
                    VtexOrder vtexOrder = await this.GetOrderInformation(orderId);
                    if (vtexOrder != null)
                    {
                        long orderTotal = vtexOrder.Totals.Sum(t => t.Value);
                        long shippingTotal = vtexOrder.Totals.Where(t => t.Name == "Shipping").Select(d => d.Value).FirstOrDefault();
                        long taxTotal = vtexOrder.Totals.Where(t => t.Name == "Tax").Select(d => d.Value).FirstOrDefault();
                        long orderItemQnty = vtexOrder.Items.Sum(i => i.Quantity);
                        long shippedItemQnty = vtexOrder.PackageAttachment.Packages.SelectMany(p => p.Items).Sum(i => i.Quantity);
                        long totalInvoice = vtexOrder.PackageAttachment.Packages.Sum(i => i.InvoiceValue);
                        long itemQntyThisShipment = shipment.ShipmentItems.Sum(s => s.Quantity);

                        OrderInvoiceNotificationRequest request = new OrderInvoiceNotificationRequest
                        {
                            Courier = shipment.CarrierCode,
                            TrackingNumber = shipment.TrackingNumber,
                            Type = ShipStationConstants.InvoiceType.OUTPUT,
                            InvoiceNumber = shipment.ShipmentId.ToString(),
                            InvoiceValue = 0L,
                            IssuanceDate = shipment.ShipDate.ToString(),
                            Items = new List<InvoiceItem>(),
                            TrackingUrl = null
                        };

                        foreach (ShipmentItem shipmentItem in shipment.ShipmentItems)
                        {
                            string sku = shipmentItem.Sku;
                            Console.WriteLine($"Item '{shipmentItem.Name}' '{sku}' ({shipmentItem.Quantity}) ${shipmentItem.UnitPrice}");
                            sb.AppendLine($"Item '{shipmentItem.Name}' '{sku}' ({shipmentItem.Quantity}) ${shipmentItem.UnitPrice}");

                            //LogisticsInfo logisticsInfo = vtexOrder.ShippingData.LogisticsInfo.Where(l => l.ItemId.Equals(sku)).FirstOrDefault();
                            //Sla sla = logisticsInfo.Slas.Where(s => s.Id.Equals(logisticsInfo.SelectedSla)).FirstOrDefault();

                            //VtexOrderItem item = vtexOrder.Items.Where(i => i.Id.Equals(shipmentItem.Sku)).FirstOrDefault();
                            //long itemTax = 0l;
                            //foreach (PriceTag priceTag in item.PriceTags)
                            //{
                            //    string name = priceTag.Name.ToLower();
                            //    if (name.Contains("tax@") || name.Contains("taxhub@"))
                            //    {
                            //        if (priceTag.IsPercentual ?? false)
                            //        {
                            //            itemTax += (long)(item.SellingPrice * priceTag.RawValue);
                            //        }
                            //        else
                            //        {
                            //            itemTax += priceTag.Value;
                            //        }
                            //    }
                            //}

                            long itemPrice = ToCents(shipmentItem.UnitPrice) * shipmentItem.Quantity;
                            long itemTax = ToCents(shipmentItem.TaxAmount ?? 0d);
                            long shippingCost = ToCents(shipmentItem.ShippingAmount ?? 0d);

                            InvoiceItem invoiceItem = new InvoiceItem
                            {
                                Id = shipmentItem.Sku,
                                Price = itemPrice,
                                Quantity = shipmentItem.Quantity
                            };

                            request.Items.Add(invoiceItem);
                            request.InvoiceValue += (itemPrice + itemTax + shippingCost);
                            Console.WriteLine($"request.InvoiceValue = {request.InvoiceValue}");
                            sb.AppendLine($"{sku}:{ToCents(shipmentItem.UnitPrice)}x{shipmentItem.Quantity}={itemPrice} Tax={itemTax} Shipping={shippingCost}");
                        }

                        sb.AppendLine($"InvoiceValue = {request.InvoiceValue}");
                        // Don't charge more than order total
                        //request.InvoiceValue = Math.Min(request.InvoiceValue, orderTotal);
                        //if(request.InvoiceValue < orderTotal)
                        //{
                        //    Console.WriteLine($"request.InvoiceValue < orderTotal : {request.InvoiceValue} < {orderTotal}");
                        //    if (shippedItemQnty + itemQntyThisShipment == orderItemQnty)
                        //    {
                        //        request.InvoiceValue = orderTotal - totalInvoice;
                        //        sb.AppendLine($"Order Complete: InvoiceValue = {request.InvoiceValue}");
                        //        Console.WriteLine($"Order Complete: InvoiceValue = {request.InvoiceValue}");
                        //    }
                        //}

                        OrderInvoiceNotificationResponse response = await this.OrderInvoiceNotification(orderId, request);
                        success = response != null;
                        sb.AppendLine($"Response='{JsonConvert.SerializeObject(response)}'");
                        _context.Vtex.Logger.Info("ProcessShipNotification", orderId, JsonConvert.SerializeObject(request));
                    }
                    else
                    {
                        sb.AppendLine($"Order {orderId} not found.");
                    }
                }
            }

            _context.Vtex.Logger.Info("ProcessShipNotification", null, sb.ToString());

            return success;
        }

        public async Task<ListAllDocksResponse[]> ListAllDocks()
        {
            ListAllDocksResponse[] listAllDocksResponse = null;
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
                listAllDocksResponse = JsonConvert.DeserializeObject<ListAllDocksResponse[]>(responseContent);
            }

            return listAllDocksResponse;
        }

        public async Task<ListAllWarehousesResponse[]> ListAllWarehouses()
        {
            ListAllWarehousesResponse[] listAllWarehousesResponse = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/logistics/pvt/configuration/warehouses")
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
            Console.WriteLine($"ListAllWarehouses [{response.StatusCode}] {responseContent}");
            if (response.IsSuccessStatusCode)
            {
                listAllWarehousesResponse = JsonConvert.DeserializeObject<ListAllWarehousesResponse[]>(responseContent);
            }

            return listAllWarehousesResponse;
        }

        public async Task<string> ValidateShipments(string date)
        {
            StringBuilder sb = new StringBuilder();
            ListShipmentsResponse shipments = await _shipStationAPIService.ListShipments($"shipDateStart={date}&shipDateEnd={date}&includeShipmentItems=true");
            Console.WriteLine($"ListShipmentsResponse pages={shipments.Pages}");
            if (shipments != null)
            {
                sb.AppendLine($"{shipments.Shipments.Count} shipments");
                foreach (Shipment shipment in shipments.Shipments)
                {
                    string orderId = shipment.OrderNumber;
                    sb.AppendLine(orderId);
                    sb.AppendLine($"items={shipment.ShipmentItems.Count}");
                    VtexOrder vtexOrder = await this.GetOrderInformation(orderId);
                    if (vtexOrder != null)
                    {
                        string orderState = vtexOrder.Status;
                        Console.WriteLine($"{orderId} {orderState}");
                        sb.AppendLine($"{orderId} {orderState}");
                        if (!orderState.Equals(ShipStationConstants.VtexOrderStatus.Invoiced))
                        {
                            bool invoiceExists = vtexOrder.PackageAttachment.Packages.Any(p => p.InvoiceNumber.Equals(shipment.ShipmentId.ToString()));
                            if (!invoiceExists)
                            {
                                ListShipmentsResponse listShipmentsTemp = new ListShipmentsResponse();
                                listShipmentsTemp.Shipments = new List<Shipment>();
                                listShipmentsTemp.Shipments.Add(shipment);
                                bool success = await this.ProcessShipNotification(listShipmentsTemp);
                                sb.AppendLine($"Order {orderId} {success}");
                            }
                            else
                            {
                                sb.AppendLine($"invoice {shipment.ShipmentId} exists.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($" Could not load {orderId}");
                        sb.AppendLine($"Could not load {orderId}");
                    }
                }
            }

            return sb.ToString();
        }

        public async Task<string> ValidateOrderShipments(string orderId)
        {
            StringBuilder sb = new StringBuilder();
            ListShipmentsResponse shipments = await _shipStationAPIService.ListShipments($"orderNumber={orderId}&includeShipmentItems=true");
            Console.WriteLine($"ListShipmentsResponse pages={shipments.Pages}");
            if (shipments != null)
            {
                sb.AppendLine($"{shipments.Shipments.Count} shipments");
                foreach (Shipment shipment in shipments.Shipments)
                {
                    sb.AppendLine($"items={shipment.ShipmentItems.Count}");
                    VtexOrder vtexOrder = await this.GetOrderInformation(orderId);
                    if (vtexOrder != null)
                    {
                        string orderState = vtexOrder.Status;
                        Console.WriteLine($"{orderId} {orderState}");
                        sb.AppendLine($"{orderId} {orderState}");
                        if (!orderState.Equals(ShipStationConstants.VtexOrderStatus.Invoiced))
                        {
                            bool invoiceExists = vtexOrder.PackageAttachment.Packages.Any(p => p.InvoiceNumber.Equals(shipment.ShipmentId.ToString()));
                            if (!invoiceExists)
                            {
                                ListShipmentsResponse listShipmentsTemp = new ListShipmentsResponse();
                                listShipmentsTemp.Shipments = new List<Shipment>();
                                listShipmentsTemp.Shipments.Add(shipment);
                                bool success = await this.ProcessShipNotification(listShipmentsTemp);
                                sb.AppendLine($"Order {orderId} {success}");
                            }
                            else
                            {
                                sb.AppendLine($"invoice {shipment.ShipmentId} exists.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($" Could not load {orderId}");
                        sb.AppendLine($"Could not load {orderId}");
                    }
                }
            }

            return sb.ToString();
        }

        public async Task CheckCancelledOrders()
        {
            int windowInMinutes = 10;
            DateTime lastCheck = await _shipStationRepository.GetLastCancelledOrderCheck();
            if(lastCheck.AddMinutes(windowInMinutes) < DateTime.Now)
            {
                string response = await CheckCancelledOrders(DateTime.Now);
                await _shipStationRepository.SetLastCancelledOrderCheck(DateTime.Now);
            }
            else
            {
                Console.WriteLine("Skipping check for cancelled orders");
            }
        }

        public async Task<string> CheckCancelledOrders(DateTime date)
        {
            bool updated = false;
            int daysToCheck = 2;
            StringBuilder sb = new StringBuilder();
            ListOrdersResponse listOrdersResponse = await _shipStationAPIService.ListOrders($"modifyDateStart={date.AddDays(-daysToCheck)}&modifyDateEnd={date}&orderStatus={ShipStationConstants.ShipStationOrderStatus.Canceled}&pageSize=500&sortBy=ModifyDate&sortDir=DESC");
            Console.WriteLine($"CheckCancelledOrders pages={listOrdersResponse.Pages}");
            if (listOrdersResponse != null)
            {
                sb.AppendLine($"{listOrdersResponse.Orders.Count} cancelled orders");
                foreach (ShipStationOrder order in listOrdersResponse.Orders)
                {
                    string orderId = order.OrderNumber;
                    sb.AppendLine(orderId);
                    VtexOrder vtexOrder = await this.GetOrderInformation(orderId);
                    if (vtexOrder != null)
                    {
                        string orderState = vtexOrder.Status;
                        Console.WriteLine($"{orderId} {orderState}");
                        sb.AppendLine($"{orderId} {orderState}");
                        if (!orderState.Equals(ShipStationConstants.VtexOrderStatus.Canceled))
                        {
                            long totalItemQuantitySS = order.Items.Sum(i => i.Quantity);
                            long totalItemQuantityVtex = vtexOrder.Items.Sum(i => i.Quantity);
                            if (totalItemQuantitySS == totalItemQuantityVtex)
                            {
                                updated = await SetOrderStatus(orderId, ShipStationConstants.VtexOrderStatus.Cancel);
                                _context.Vtex.Logger.Info("CheckCancelledOrders", "CancelOrder", $"Order {order} cancelled? {updated}");
                            }
                            else
                            {
                                ChangeOrderRequest changeOrderRequest = new ChangeOrderRequest
                                {
                                    OrderId = orderId,
                                    Reason = ShipStationConstants.ORDER_CHANGE_REASON,
                                    ItemsRemoved = new List<ChangeItem>(),
                                    RequestId = order.ModifyDate
                                };

                                if (vtexOrder.ChangesAttachment != null && vtexOrder.ChangesAttachment.ChangesData != null)
                                {
                                    var itemsRemovedVtex = vtexOrder.ChangesAttachment.ChangesData.SelectMany(c => c.ItemsRemoved);
                                    ListOrdersResponse listOrdersByOrderNumberResponse = await _shipStationAPIService.ListOrders($"orderNumber={orderId}");
                                    var cancelledItemsShipstation = listOrdersByOrderNumberResponse.Orders.Where(o => o.OrderStatus.Equals(ShipStationConstants.ShipStationOrderStatus.Canceled)).SelectMany(i => i.Items);
                                    var notCancelledItemsShipstation = listOrdersByOrderNumberResponse.Orders.Where(o => !o.OrderStatus.Equals(ShipStationConstants.ShipStationOrderStatus.Canceled)).SelectMany(i => i.Items);

                                    foreach (OrderItem cancelledItem in order.Items)
                                    {
                                        long totalCancelledForSku = cancelledItemsShipstation.Where(o => o.Sku.Equals(cancelledItem.Sku)).Sum(i => i.Quantity);
                                        long qntyAlreadyRemovedForSku = itemsRemovedVtex.Where(o => o.Id.Equals(cancelledItem.Sku)).Sum(i => i.Quantity);
                                        Console.WriteLine($"[-] | [-] | [-] | [-] | [-]     ({cancelledItem.Sku}): {qntyAlreadyRemovedForSku} < {totalCancelledForSku}");
                                        if (qntyAlreadyRemovedForSku < totalCancelledForSku)
                                        {

                                            ChangeItem changeItem = new ChangeItem
                                            {
                                                Id = cancelledItem.LineItemKey,
                                                Price = (long)(cancelledItem.UnitPrice * 100) * cancelledItem.Quantity,
                                                Quantity = cancelledItem.Quantity
                                            };

                                            changeOrderRequest.ItemsRemoved.Add(changeItem);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (OrderItem cancelledItem in order.Items)
                                    {
                                        ChangeItem changeItem = new ChangeItem
                                        {
                                            Id = cancelledItem.LineItemKey,
                                            Price = (long)(cancelledItem.UnitPrice * 100) * cancelledItem.Quantity,
                                            Quantity = cancelledItem.Quantity
                                        };

                                        changeOrderRequest.ItemsRemoved.Add(changeItem);
                                    }
                                }

                                if (changeOrderRequest.ItemsRemoved.Count > 0)
                                {
                                    ChangeOrderResponse changeOrderResponse = await ChangeOrder(orderId, changeOrderRequest);
                                    updated = changeOrderResponse != null;
                                    _context.Vtex.Logger.Info("CheckCancelledOrders", "ChangeOrder", $"ChangeOrderRequest: {JsonConvert.SerializeObject(changeOrderRequest)}  ChangeOrderResponse: {JsonConvert.SerializeObject(changeOrderResponse)}");
                                }
                                else
                                {
                                    Console.WriteLine("CheckCancelledOrders - Skipping...");
                                }
                            }

                            sb.AppendLine($"Order {orderId} updated? {updated}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($" Could not load {orderId}");
                        sb.AppendLine($"Could not load {orderId}");
                    }
                }
            }

            return sb.ToString();
        }

        public async Task<ChangeOrderResponse> ChangeOrder(string orderId, ChangeOrderRequest changeOrderRequest)
        {
            ChangeOrderResponse changeOrderResponse = null;
            string jsonSerializedRequest = JsonConvert.SerializeObject(changeOrderRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[ShipStationConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/oms/pvt/orders/{orderId}/changes"),
                Content = new StringContent(jsonSerializedRequest, Encoding.UTF8, ShipStationConstants.APPLICATION_JSON)
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
            Console.WriteLine($"ChangeOrder for Order '{orderId}' [{response.StatusCode}] {responseContent}");
            if(response.IsSuccessStatusCode)
            {
                changeOrderResponse = JsonConvert.DeserializeObject<ChangeOrderResponse>(responseContent);
            }

            return changeOrderResponse;
        }

        private long ToCents(double asDollars)
        {
            return (long)Math.Round(asDollars * 100);
        }
    }
}
